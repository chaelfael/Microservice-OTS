using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTS_ORDER.Models
{
    [Table("ORDERS")]
    public class Order
    {
        [Key]
        [Column("ORDER_ID")]
        public long OrderId { get; set; }

        [Required]
        [Column("ORDER_NO")]
        [StringLength(100)]
        public string OrderNo { get; set; } = default!;

        [Required]
        [Column("CUSTOMER_EMAIL")]
        [StringLength(255)]
        public string CustomerEmail { get; set; } = default!;

        [Required]
        [Column("ORDER_DATE")]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column("TOTAL_AMOUNT")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column("ORDER_STATUS")]
        [StringLength(50)]
        public string OrderStatus { get; set; } = default!;

        // Navigation properties
        [InverseProperty(nameof(Payment.Order))]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [InverseProperty(nameof(OrderDetail.Order))]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}