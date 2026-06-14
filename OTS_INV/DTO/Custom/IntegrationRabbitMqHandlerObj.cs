namespace OTS_INV.DTO
{
    public class IntegrationRabbitMqHandlerObj
    {
        public string SystemFrom { get; set; }          // e.g., "OTS_ORDER"
        public string IntegrationMapCode { get; set; }  // e.g., "ORDER_INITIATED"
        public string TrxNo { get; set; }               // e.g., "ORD-20260414-8F3A"
        public string Payload { get; set; }             // The serialized Event Object
    }
}