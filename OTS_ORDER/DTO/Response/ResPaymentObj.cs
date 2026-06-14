using System;

namespace OTS_ORDER.Dtos
{
    public class ResPaymentObj
    {
        public long PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = default!;
        public string PaymentStatus { get; set; } = default!;
    }
}