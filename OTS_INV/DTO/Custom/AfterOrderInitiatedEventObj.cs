using OTS_INV.models;

namespace OTS_INV.DTO
{
   
    public class AfterOrderInitiatedEventObj
    {
        
        public List<string> TicketNo { get; set; }
        public decimal Price { get; set; }
    }
}