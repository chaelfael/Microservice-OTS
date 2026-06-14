using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTS_ORDER.Models
{
    [Table("PAYMENTS")]
    public class Payment
    {
        [Key]
        [Column("PAYMENT_ID")]
        public long PaymentId { get; set; }

        [Required]
        [Column("ORDER_ID")]
        public long OrderId { get; set; }

        [Required]
        [Column("PAYMENT_DATE")]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Column("PAYMENT_METHOD")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = default!;
        
        
        [Required]
        [Column("PAID_AMT")]
        
        public decimal PaidAmt { get; set; } = default!;
        

        [Required]
        [Column("PAYMENT_STATUS")]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = default!;

        // Foreign Key linking back to Order
        [ForeignKey(nameof(OrderId))]
        [InverseProperty(nameof(Models.Order.Payments))]
        public virtual Order Order { get; set; } = default!;
    }
}