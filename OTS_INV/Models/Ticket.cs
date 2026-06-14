using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTS_INV.models
{
    [Table("TICKET")]
    public class Ticket
    {
        [Key]
        [Column("TICKET_ID")]
        public long TicketId { get; set; }

        [Required]
        [Column("TICKET_NUMBER")]
        [StringLength(100)]
        public string TicketNumber { get; set; } = default!;

        [Required]
        [Column("STATUS")]
        public string Status { get; set; }
        [Required]
        [Column("RESERVED_BY_ORDER_NO")]
        public string ReservedByOrderNo { get; set; }

        // Foreign Key for TicketClass
        [Required]
        [Column("TICKET_CLASS_ID")]
        public long TicketClassId { get; set; }

        [ForeignKey(nameof(TicketClassId))]
        [InverseProperty(nameof(models.TicketClass.Tickets))]
        public virtual TicketClass TicketClass { get; set; } = default!;
    }
}