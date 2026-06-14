using OTS_ORDER.Models;

namespace OTS_ORDER.Dtos
{
    public class ResOrderByOrderNoObj
    {
        public string OrderNo { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public List<ResOrderDetailDto> OrderDetails { get; set; } = new List<ResOrderDetailDto>();
        public string OrderStatus { get; set; } = default!;
    }
}