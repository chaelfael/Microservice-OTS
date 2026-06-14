using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OTS_INV.Data;
using OTS_INV.Interface;
using OTS_INV.Services;

namespace OTS_INV.Controllers
{
    [Route("api/internal/gatekeeper")]
    [ApiController]
    public class InternalGatekeeperController : ControllerBase
    {
        private readonly ITicketCacheService _cacheService;
        private readonly InvDbContext _context; // Inject your DB Context

        public InternalGatekeeperController(ITicketCacheService cacheService, InvDbContext context)
        {
            _cacheService = cacheService;
            _context = context;
        }

        // 1. The Self-Healing Initialization
        [HttpPost("init-sale/{ticketCode}")]
        public async Task<IActionResult> InitFlashSale(string ticketCode)
        {
            // The Ultimate Source of Truth: Count actual empty chairs
            int trueAvailableStock = await _context.Tickets
                .Where(t => t.TicketClass.TicketClassCode == ticketCode && t.Status == "AVAILABLE")
                .CountAsync();

            // Push that bulletproof number to Redis
            await _cacheService.InitializeTicketsAsync(ticketCode, trueAvailableStock);

            return Ok(new { message = $"Flash sale initialized. {trueAvailableStock} tickets loaded into Redis for {ticketCode}." });
        }

        // 2. The Gatekeeper Check (Called by Order Service)
        [HttpPost("reserve/{ticketCode}/{quantity}")]
        public async Task<IActionResult> RequestTicket(string ticketCode, int quantity)
        {
            var isGranted = await _cacheService.TryReserveTicketAsync(ticketCode, quantity);

            if (isGranted) return Ok(new { success = true });

            return BadRequest(new { success = false, message = "Sold Out!" });
        }
    }
}