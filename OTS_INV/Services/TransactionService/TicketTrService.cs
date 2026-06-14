
using System.Text.Json;
using OTS_INV.Data;
using OTS_INV.DTO;
using OTS_INV.Interface;
using OTS_INV.models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OTS_INV.Services
{
    public class TicketTrService : ITicketTrService
    {
        private readonly InvDbContext _context;
        private readonly IMessageBusClient _messageBusClient;
        private readonly ITicketCacheService _cacheService;

        public TicketTrService(InvDbContext context, IMessageBusClient messageBusClient, ITicketCacheService cacheService)
        {
            _context = context;
            _messageBusClient = messageBusClient;
            _cacheService = cacheService;
        }

        public async Task<string> PlaceTickets(OrderInitiatedEventObj reqObj)
        {
            // 1. Fetch the tickets WITH their linked class data
            List<Ticket> AvalTicket = await _context.Tickets
                .Include(t => t.TicketClass) // <-- THE FIX: Tells EF Core to load the TicketClass so Price isn't null!
                .Where(t => t.TicketClass.TicketClassCode == reqObj.CategoryCode && t.Status == "AVAILABLE")
                .OrderBy(t => t.TicketId)    // <-- THE FIX: Solves the yellow warning
                .Take(reqObj.Quantity)
                .ToListAsync();

            if (AvalTicket.Count < reqObj.Quantity)
            {
                throw new Exception("Not enough tickets available");
            }

            // 2. Process the lock
            foreach (var ticket in AvalTicket)
            {
                ticket.Status = "RESERVED"; // Changed from "SOLD" to "RESERVED" per standard soft-lock logic
                ticket.ReservedByOrderNo = reqObj.OrderNo;
            }

            // This will now work perfectly because .TicketClass is no longer null!
            decimal price = AvalTicket.Sum(t => t.TicketClass.Price);

            // 3. Save the changes to the database
            await _context.SaveChangesAsync();
            Console.WriteLine($"--> SUCCESS: Reserved {reqObj.Quantity} tickets for {reqObj.OrderNo}.");

            // 4. Publish the success message back to the Message Bus
            try
            {
                var eventMessage = new AfterOrderInitiatedEventObj
                {
                    TicketNo = AvalTicket.Select(t => t.TicketNumber).ToList(),
                    Price = price
                };

                var envelope = new IntegrationRabbitMqHandlerObj
                {
                    SystemFrom = "OTS_INV",
                    IntegrationMapCode = "AFTER_ORDER_INITIATED",
                    TrxNo = reqObj.OrderNo,
                    Payload = JsonSerializer.Serialize(eventMessage)
                };

                _messageBusClient.PublishMessage(envelope);
                Console.WriteLine($"--> Published AFTER_ORDER_INITIATED for {reqObj.OrderNo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Failed to publish message: {ex.Message}");
            }

            return reqObj.OrderNo;
        }

        public async Task<string> CancelExpiredOrder(ExpireCancelOrderPayload request)
        {
            var orderNo = request.OrderNo;

            // 1. Fetch the tickets that are reserved by this order number
            List<Ticket> reservedTickets = await _context.Tickets
                 .Include(t => t.TicketClass)
                 .Where(t => t.ReservedByOrderNo == orderNo && t.Status == "RESERVED")
                 .ToListAsync();

            if (reservedTickets.Count == 0)
            {
                Console.WriteLine($"--> No reserved tickets found for OrderNo: {orderNo}");
                return orderNo;
            }
            var ticketClassCode = reservedTickets.First().TicketClass.TicketClassCode;

            // 2. Update the status back to AVAILABLE and clear the ReservedByOrderNo
            foreach (var ticket in reservedTickets)
            {
                // Cold Storage Update (PostgreSQL)
                ticket.Status = "AVAILABLE";
                ticket.ReservedByOrderNo = "";

            }
            await _cacheService.ReturnTicketAsync(ticketClassCode, reservedTickets.Count);

            // 3. Save the changes to the database
            await _context.SaveChangesAsync();

            Console.WriteLine($"--> SUCCESS: Released {reservedTickets.Count} tickets for expired/cancelled order {orderNo}. Redis Incremented.");

            return orderNo;
        }
        public async Task<string> UpdateTicketPayment(string orderNo)
        {
            // 1. Fetch the tickets that are reserved by this order number
            List<Ticket> reservedTickets = await _context.Tickets
                .Where(t => t.ReservedByOrderNo == orderNo && t.Status == "RESERVED")
                .ToListAsync();

            if (reservedTickets.Count == 0)
            {
                Console.WriteLine($"--> No reserved tickets found for OrderNo: {orderNo}");
                return orderNo;
            }

            foreach (var ticket in reservedTickets)
            {
                ticket.Status = "SOLD";
            }

            // 3. Save the changes to the database
            await _context.SaveChangesAsync();
            Console.WriteLine($"--> SUCCESS: Released {reservedTickets.Count} tickets for expired/cancelled order {orderNo}.");

            return orderNo;
        }


    }
}