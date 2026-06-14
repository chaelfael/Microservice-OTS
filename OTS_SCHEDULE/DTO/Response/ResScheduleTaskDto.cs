using System;

namespace OTS_SCHEDULE.Dtos
{
    public class ResScheduleTaskDto
    {
        public long TaskId { get; set; }
        public string OrderNo { get; set; } = default!;
        public DateTime ExecuteAt { get; set; }
        public string Status { get; set; } = default!;
    }
}