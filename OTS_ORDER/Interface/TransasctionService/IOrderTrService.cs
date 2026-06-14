using OTS_ORDER.Dtos;

namespace OTS_ORDER.Interface
{
    public interface IOrderTrService
    {
        Task<string> PlaceOrder(ReqPlaceOrderObj reqPlaceOrderObj);
        Task<string> AddOrderDetails(AfterOrderInitiatedEventObj reqPlaceOrderObj, string orderNo);
        Task<string> ExpireOrder(ExpireCancelOrderPayload expireData);
        Task<string> CancelOrder(ExpireCancelOrderPayload expireData);
        Task<string> OrderPayment(ReqOrderPaymentObj reqObj);
    }
}