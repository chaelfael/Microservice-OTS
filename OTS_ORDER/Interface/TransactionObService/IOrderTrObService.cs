using OTS_ORDER.Dtos;

namespace OTS_ORDER.Interface
{
    public interface IOrderTrObService
    {
      Task<ResOrderByOrderNoObj> GetOrderByOrderNo (string orderNo);
    }
}