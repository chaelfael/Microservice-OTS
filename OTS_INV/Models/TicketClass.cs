using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTS_INV.models
{
    [Table("TICKET_CLASS")]
    public class TicketClass
    {
        [Key]
        [Column("TICKET_CLASS_ID")]
        public long TicketClassId { get; set; }

        [Required]
        [Column("TICKET_CLASS_CODE")]
        [StringLength(100)]
        public string TicketClassCode { get; set; } = default!;

        [Required]
        [Column("TICKET_CLASS_NAME")]
        [StringLength(1000)]
        public string TicketClassName { get; set; } = default!;

        [Required]
        [Column("PRICE")]
        public decimal Price { get; set; }

        [Required]
        [Column("CAPACITY")]
        public int Capacity { get; set; }


        [InverseProperty(nameof(Ticket.TicketClass))]
        public virtual ICollection<Ticket> Tickets { get; set; } 
    }
}