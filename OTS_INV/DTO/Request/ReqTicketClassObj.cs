using System;
using System.ComponentModel.DataAnnotations;

namespace OTS_INV.DTO.Request
{
    public class ReqCreateTicketClassObj
    {
        [Required]
        public long EventId { get; set; }

        [Required]
        public string TicketClassCode { get; set; } = default!;

        [Required]
        public string TicketClassName { get; set; } = default!;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Capacity { get; set; }
    }
}