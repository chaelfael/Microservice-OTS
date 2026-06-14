
using System.Text.Json;
using OTS_SCHEDULE.Data;
using OTS_SCHEDULE.DTO;
using OTS_SCHEDULE.Interface;
using OTS_SCHEDULE.models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OTS_SCHEDULE.Services
{
    public class SchdlTrService : ISchdlTrService
    {
        private readonly SchdlDbContext _context;
        private readonly IMessageBusClient _messageBusClient;
        private readonly IConfiguration _configuration;

        public SchdlTrService(SchdlDbContext context, IMessageBusClient messageBusClient, IConfiguration configuration)
        {
            _context = context;
            _messageBusClient = messageBusClient;
            _configuration = configuration;
            
        }

        public async Task<string> CreateSchedule(OrderInitiatedEventObj reqObj)
        {
            // 1. Fetch the timeout from appsettings.json or Docker Environment Variables
            // If it doesn't exist, it defaults to 300 seconds (5 minutes)
            int timeoutSeconds = _configuration.GetValue<int>("OrderTimeoutSeconds", 300);

            // 2. Create the schedule using AddSeconds instead of AddMinutes
            var newSchedule = new ScheduleTask
            {
                OrderNo = reqObj.OrderNo,
                ExecuteAt = DateTime.UtcNow.AddSeconds(timeoutSeconds),
                Status = "WAITING_EXECUTION"
            };

            _context.ScheduleTasks.Add(newSchedule);
            await _context.SaveChangesAsync();

            // (Note: We removed the immediate RabbitMQ publish here, as discussed in the previous step!)

            return $"Schedule created for OrderNo: {reqObj.OrderNo}";
        }
        public async Task<string> UpdateScheduleStatus(string orderNo, string newStatus)
        {
            var schedule = await _context.ScheduleTasks.FirstOrDefaultAsync(s => s.OrderNo == orderNo);
            if (schedule == null)
            {
                return $"Schedule with OrderNo: {orderNo} not found.";
                throw new Exception($"Schedule with OrderNo: {orderNo} not found.");
            }

            schedule.Status = newStatus;
            await _context.SaveChangesAsync();

            return $"Schedule for OrderNo: {orderNo} updated to status: {newStatus}";
        }

        

    }
}