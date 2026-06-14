namespace OTS_INV.DTO.Request.Response
{
    public class ResTicketClassObj
    {
        public long TicketClassId { get; set; }
        public string TicketClassCode { get; set; } = default!;
        public string TicketClassName { get; set; } = default!;
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        
        // Calculated stock to show users before they buy
        public int AvailableStock { get; set; } 
    }
}