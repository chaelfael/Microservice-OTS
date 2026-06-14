

using Microsoft.AspNetCore.Mvc;
using OTS_SCHEDULE.Data;
using OTS_SCHEDULE.models;
using Microsoft.EntityFrameworkCore;

namespace OTS_SCHEDULE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchdlClassesController : ControllerBase
    {
        private readonly SchdlDbContext _context;

        // Inject the DbContext directly for this simple dummy controller
        public SchdlClassesController(SchdlDbContext context)
        {
            _context = context;
        }

        // GET: api/ticketclasses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleTask>>> GetAllOrders()
        {
            Console.WriteLine("--> Fetching all Orders from the database...");

            // Simply fetch everything from the Orders table that we seeded earlier
            var orders = await _context.ScheduleTasks.ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found in the database.");
            }

            // Returns a 200 OK with the raw JSON array of Orders
            return Ok(orders);
        }
    }
}