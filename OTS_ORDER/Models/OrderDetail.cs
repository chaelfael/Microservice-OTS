using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTS_ORDER.Models
{
    [Table("ORDER_DETAILS")]
    public class OrderDetail
    {
        [Key]
        [Column("ORDER_DETAIL_ID")]
        public long OrderDetailId { get; set; }

        [Required]
        [Column("TICKET_NUMBER")]
        [StringLength(100)]
        public string TicketNumber { get; set; } = default!;

        [Required]
        [Column("ORDER_ID")]
        public long OrderId { get; set; }

        // Foreign Key linking back to Order
        [ForeignKey(nameof(OrderId))]
        [InverseProperty(nameof(Models.Order.OrderDetails))]
        public virtual Order Order { get; set; } = default!;
    }
}