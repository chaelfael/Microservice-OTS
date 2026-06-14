using OTS_SCHEDULE.DTO;

namespace OTS_SCHEDULE.Interface
{

    public interface IMessageBusClient
    {

        void PublishMessage(IntegrationRabbitMqHandlerObj messageEnvelope);
    }
}