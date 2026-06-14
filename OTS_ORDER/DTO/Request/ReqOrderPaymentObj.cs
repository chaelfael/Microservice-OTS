namespace  OTS_ORDER.Dtos
{
    public class ReqOrderPaymentObj
    {
        public string OrderNo { get; set; } = default!;
        public decimal PaidAmount { get; set; }
    }
}