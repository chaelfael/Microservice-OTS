using OTS_INV.DTO;

namespace OTS_INV.Interface
{
    public interface ITicketTrService
    {
        Task<string> PlaceTickets(OrderInitiatedEventObj orderInitiatedEventObj);
        Task<string> CancelExpiredOrder(ExpireCancelOrderPayload request);
        Task<string> UpdateTicketPayment(string orderNo);
    }
}