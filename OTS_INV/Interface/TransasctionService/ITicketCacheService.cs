 using OTS_INV.DTO;

namespace OTS_INV.Interface{
 public interface ITicketCacheService
    {
        Task InitializeTicketsAsync(string ticketCode, int totalStock);
        Task<bool> TryReserveTicketAsync(string ticketCode, int quantity);
        Task ReturnTicketAsync(string ticketCode, int quantity); // NEW: The Healing Command
    }
}