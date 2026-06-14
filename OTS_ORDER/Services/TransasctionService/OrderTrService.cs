using OTS_ORDER.Data;
using OTS_ORDER.Models;
using OTS_ORDER.Dtos;
using OTS_ORDER.Interface;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Net.Http; // ADDED for IHttpClientFactory

namespace OTS_ORDER.Services
{
    public class OrderTrService : IOrderTrService
    {
        private readonly OrderDbContext _context;
        private readonly IMessageBusClient _messageBusClient;
        private readonly IHttpClientFactory _httpClientFactory; // ADDED

        // Injected the IHttpClientFactory into the constructor
        public OrderTrService(OrderDbContext context, IMessageBusClient messageBusClient, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _messageBusClient = messageBusClient;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> PlaceOrder(ReqPlaceOrderObj reqObj)
        {
            // ================================================================
            // 1. THE BOUNCER CHECK (Fast Lane to Redis via Inventory API)
            // ================================================================
            var client = _httpClientFactory.CreateClient("InventoryClient");
            
            // Call the Inventory Service to atomically decrement the ticket stock
            var bouncerResponse = await client.PostAsync(
               $"/api/internal/gatekeeper/reserve/{reqObj.CategoryCode}/{reqObj.Quantity}", null);

            if (!bouncerResponse.IsSuccessStatusCode)
            {
                var errorContent = await bouncerResponse.Content.ReadAsStringAsync();
                
                // ADDED: Print the exact HTTP Status Code (e.g., 404, 500, 400)
                throw new Exception($"Flash Sale Check Failed. Status: {(int)bouncerResponse.StatusCode} ({bouncerResponse.StatusCode}). Details: '{errorContent}'");
            }

            // ================================================================
            // 2. THE BASE LOGIC (Local DB Transaction)
            // ================================================================
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            string orderNo = $"ORD-{datePart}-{randomPart}";

            var newOrder = new Order
            {
                OrderNo = orderNo,
                CustomerEmail = reqObj.Email,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0,
                OrderStatus = "INITIATED"
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            // ================================================================
            // 3. THE MICROSERVICE ROUTING (RabbitMQ Envelope)
            // ================================================================
            try
            {
                var eventMessage = new OrderInitiatedEventObj
                {
                    OrderNo = orderNo,
                    CategoryCode = reqObj.CategoryCode,
                    Quantity = reqObj.Quantity
                };

                // Pack it into the standard envelope
                var envelope = new IntegrationRabbitMqHandlerObj
                {
                    SystemFrom = "OTS_ORDER",
                    IntegrationMapCode = "ORDER_INITIATED",
                    TrxNo = orderNo,
                    Payload = JsonSerializer.Serialize(eventMessage)
                };

                // Publish to the queue
                _messageBusClient.PublishMessage(envelope);
            }
            catch (Exception ex)
            {
                // UPDATED: Strict exception throwing instead of WriteLine
                throw new Exception($"Failed to publish ORDER_INITIATED message to RabbitMQ: {ex.Message}", ex);
            }

            return orderNo;
        }

        public async Task<string> AddOrderDetails(AfterOrderInitiatedEventObj reqObj, string orderNo)
        {
            Order order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNo == orderNo);

            if (order != null)
            {
                order.OrderDetails = reqObj.TicketNo.Select(ticket => new OrderDetail
                {
                    TicketNumber = ticket,
                }).ToList();
                
                order.OrderStatus = "WAITING_PAYMENT";
                order.TotalAmount = reqObj.Price;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return order.OrderNo;
            }
            
            throw new Exception($"Order with OrderNo {orderNo} not found");
        }

        public async Task<string> ExpireOrder(ExpireCancelOrderPayload expireData)
        {
            Order order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNo == expireData.OrderNo);

            if (order != null)
            {
                order.OrderStatus = "EXPIRED";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return order.OrderNo;
            }
            

            return null;
        }

        public async Task<string> CancelOrder(ExpireCancelOrderPayload expireData)
        {
            Order order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNo == expireData.OrderNo);

            if (order == null)
            {
                throw new Exception($"Order with OrderNo {expireData.OrderNo} not found for cancellation.");
            }
            
            if (order.OrderStatus == "CANCELLED" || order.OrderStatus == "EXPIRED")
            {
                throw new Exception($"Order with OrderNo {expireData.OrderNo} has already been cancelled or expired.");
            }
          
            order.OrderStatus = "CANCELLED";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            
            try
            {
                var eventMessage = new ExpireCancelOrderPayload
                {
                    OrderNo = expireData.OrderNo,
                };

                // Pack it into the standard envelope
                var envelope = new IntegrationRabbitMqHandlerObj
                {
                    SystemFrom = "OTS_ORDER",
                    IntegrationMapCode = "ORDER_CANCELLED",
                    TrxNo = expireData.OrderNo,
                    Payload = JsonSerializer.Serialize(eventMessage)
                };

                // Publish to the queue
                _messageBusClient.PublishMessage(envelope);
            }
            catch (Exception ex)
            {
                // UPDATED: Strict exception throwing instead of WriteLine
                throw new Exception($"Failed to publish ORDER_CANCELLED message to RabbitMQ: {ex.Message}", ex);
            }

            return order.OrderNo;
        }

        public async Task<string> OrderPayment(ReqOrderPaymentObj reqObj)
        {
            // 1. Find the Order
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNo == reqObj.OrderNo);
            
            if (order == null) 
                throw new Exception($"Order validation failed: OrderNo '{reqObj.OrderNo}' not found.");
                
            if (order.OrderStatus == "PAID") 
                throw new Exception($"Payment rejected: OrderNo '{reqObj.OrderNo}' has already been paid.");
                
            if (order.OrderStatus == "CANCELLED") 
                throw new Exception($"Payment rejected: OrderNo '{reqObj.OrderNo}' is expired or cancelled.");

            // 2. Validate Amount
            if (reqObj.PaidAmount != order.TotalAmount)
            {
                throw new Exception($"Payment rejected: The amount paid (Rp {reqObj.PaidAmount}) does not match the Order Total (Rp {order.TotalAmount}).");
            }

            // 3. Success Logic
            var successfulPayment = new Payment
            {
                OrderId = order.OrderId,
                PaidAmt = reqObj.PaidAmount,
                PaymentMethod = "BANK_TRANSFER",
                PaymentDate = DateTime.UtcNow,
                PaymentStatus = "SUCCESS"
            };

            _context.Payments.Add(successfulPayment);
            order.OrderStatus = "PAID";
            await _context.SaveChangesAsync();

            // 4. Shout to RabbitMQ
            try 
            {
                var payload = new { OrderNo = order.OrderNo };
                var envelope = new IntegrationRabbitMqHandlerObj
                {
                    SystemFrom = "OTS_ORDER",
                    IntegrationMapCode = "PAYMENT_SUCCESS",
                    TrxNo = order.OrderNo,
                    Payload = JsonSerializer.Serialize(payload)
                };

                _messageBusClient.PublishMessage(envelope);
            }
            catch (Exception ex)
            {
                // UPDATED: Strict exception throwing instead of WriteLine
                throw new Exception($"Failed to publish PAYMENT_SUCCESS message to RabbitMQ: {ex.Message}", ex);
            }

            return "Payment exactly matched and was successful!";
        }
    }
}