using OTS_SCHEDULE.DTO;

namespace OTS_SCHEDULE.Interface
{
    public interface ISchdlTrService
    {
        Task<string> CreateSchedule(OrderInitiatedEventObj orderInitiatedEventObj);
        Task<string> UpdateScheduleStatus(string orderNo, string newStatus);
    }
}