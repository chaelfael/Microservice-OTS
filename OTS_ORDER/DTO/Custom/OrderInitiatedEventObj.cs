namespace OTS_ORDER.Dtos
{
   
    public class OrderInitiatedEventObj
    {
        public string OrderNo { get; set; }
        public string CategoryCode { get; set; }
         public int Quantity { get; set; }
    }
}