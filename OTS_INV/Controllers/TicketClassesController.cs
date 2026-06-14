using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using OTS_INV.Data;
using Asp.Versioning;

// Note: If you physically moved this file to the Order Service project, 
// make sure to change this namespace to OTS_ORDER.Controllers
namespace OTS_INV.Controllers
{
    [ApiController]
    [ApiVersion("1")] // Explicitly define this as version 1.0 of the API
    [Route("v1/Ticket")]
    public class TicketController : ControllerBase
    {
        private readonly InvDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public TicketController(InvDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        [HttpGet]
        [MapToApiVersion("1")]
        public async Task<IActionResult> GetAllTicketClasses()
        {
            Console.WriteLine("--> [v1.0] Fetching ticket names from DB and quantities from Redis...");

            // 1. Fetch the static ticket details (Names, IDs) from PostgreSQL
            var ticketClasses = await _context.TicketClasses.ToListAsync();

            if (ticketClasses == null || !ticketClasses.Any())
            {
                return NotFound(new { message = "No ticket classes found in the database." });
            }

            // 2. Prepare for a high-performance batch fetch from Redis
            var db = _redis.GetDatabase();

            // Generate the exact keys Redis expects (e.g., "ticket_qty:1", "ticket_qty:2")
            var redisKeys = ticketClasses.Select(tc => (RedisKey)$"TicketStock:{tc.TicketClassCode}").ToArray();

            // Fetch ALL quantities from Redis in a single, lightning-fast network call
            var redisValues = await db.StringGetAsync(redisKeys);

            // 3. Combine the DB names with the live Redis quantities
            var responsePayload = ticketClasses.Select((tc, index) =>
            {
                var redisQuantity = redisValues[index];

                // Fallback to 0 if the key is missing or empty in Redis
                int availableQty = redisQuantity.HasValue ? (int)redisQuantity : 0;

                return new
                {
                    TicketId = tc.TicketClassId,
                    TicketClassName = tc.TicketClassName,
                    TicketClassCode = tc.TicketClassCode, // The crucial bridge to the Order Service
                    AvailableTickets = availableQty       // Renamed as you requested

                };
            }).ToList();

            // Returns a 200 OK with the optimized JSON array
            return Ok(responsePayload);
        }
    }
}