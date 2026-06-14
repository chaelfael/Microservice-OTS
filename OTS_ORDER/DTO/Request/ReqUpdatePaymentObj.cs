using System.ComponentModel.DataAnnotations;

namespace OTS_ORDER.Dtos
{
    public class ReqUpdatePaymentDto
    {
        [Required]
        public string PaymentMethod { get; set; } = default!;

        [Required]
        public string PaymentStatus { get; set; } = default!; // e.g., SUCCESS, FAILED
    }
}