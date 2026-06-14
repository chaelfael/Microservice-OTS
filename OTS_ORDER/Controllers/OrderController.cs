using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OTS_ORDER.Dtos; // Using your 'Dtos' namespace
using OTS_ORDER.Interface;

namespace OTS_ORDER.Controllers
{
    // AdIns Style: Versioning and dynamic routing
    [ApiVersion("1")]
    [Route("api/order/v{version:apiVersion}/Order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderTrService _orderTrService;
        private readonly IOrderTrObService _orderTrObService;

        public OrderController(IOrderTrService orderTrService, IOrderTrObService orderTrObService)
        {
            _orderTrService = orderTrService;
            _orderTrObService = orderTrObService;
        }

        [HttpPost("PlaceOrder")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> PlaceOrder([FromBody] ReqPlaceOrderObj req)
        {
            try
            {
                // This triggers the DB save AND the RabbitMQ publish we just built
                string orderNo = await _orderTrService.PlaceOrder(req);

                // Standardized response object to match your enterprise sample
                return Ok(new
                {
                    StatusCode = "200",
                    Message = "Order Successfully Initiated",
                    TrxNo = orderNo
                });
            }
            catch (Exception ex)
            {
                // Log the error locally for debugging your thesis experiments
                Console.WriteLine($"[OrderController Error]: {ex.Message}");

                return StatusCode(500, new
                {
                    StatusCode = "500",
                    Message = "An internal error occurred while processing the order."
                });
            }
        }

        [HttpPost("CancelOrder")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> CancelOrder([FromBody] ExpireCancelOrderPayload expireData)
        {
            try
            {
                // This triggers the DB save AND the RabbitMQ publish we just built
                string orderNo = await _orderTrService.CancelOrder(expireData);

                // Standardized response object to match your enterprise sample
                return Ok(new
                {
                    StatusCode = "200",
                    Message = "Order Successfully Cancelled",
                    TrxNo = orderNo
                });
            }
            catch (Exception ex)
            {
                // Log the error locally for debugging your thesis experiments
                Console.WriteLine($"[OrderController Error]: {ex.Message}");

                return StatusCode(500, new
                {
                    StatusCode = "500",
                    Message = "An internal error occurred while processing the order."
                });
            }
        }

        [HttpPost("OrderPayment")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> OrderPayment([FromBody] ReqOrderPaymentObj req)
        {
            try
            {
                // This triggers the DB save AND the RabbitMQ publish we just built
                string orderNo = await _orderTrService.OrderPayment(req);

                // Standardized response object to match your enterprise sample
                return Ok(new
                {
                    StatusCode = "200",
                    Message = "Order Successfully Paid",
                    TrxNo = orderNo
                });
            }
            catch (Exception ex)
            {
                // Log the error locally for debugging your thesis experiments
                Console.WriteLine($"[OrderController Error]: {ex.Message}");

                return StatusCode(500, new
                {
                    StatusCode = "500",
                    Message = "An internal error occurred while processing the order."
                });
            }
        }
        [HttpGet("GetOrderByOrderNo/{orderNo}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> GetOrderByOrderNo(string orderNo)
        {
            try
            {
                // Notice I changed this back to _orderTrService to match your earlier constructor!
                var order = await _orderTrObService.GetOrderByOrderNo(orderNo);

                // 1. The Not Found Check (Business Logic)
                if (order == null)
                {
                    return NotFound(new
                    {
                        StatusCode = "404",
                        Message = $"Order with OrderNo '{orderNo}' was not found."
                    });
                }

                // 2. The Success Return
                // Because your service already maps it to ResOrderByOrderNoObj, 
                // we can just return it directly!
                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController Error]: {ex.Message}");

                return StatusCode(500, new
                {
                    StatusCode = "500",
                    Message = "An internal error occurred while retrieving the order."
                });
            }
        }
    }
}