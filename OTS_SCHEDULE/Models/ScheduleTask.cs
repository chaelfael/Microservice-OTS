using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTS_SCHEDULE.models
{
    [Table("SCHEDULE_TASKS")]
    public class ScheduleTask
    {
        [Key]
        [Column("TASK_ID")]
        public long TaskId { get; set; }

        [Required]
        [Column("ORDER_NO")]
        [StringLength(100)]
        public string OrderNo { get; set; } = default!;

        [Required]
        [Column("EXECUTE_AT")]
        public DateTime ExecuteAt { get; set; }

        [Required]
        [Column("STATUS")]
        [StringLength(50)]
        public string Status { get; set; } = default!; // e.g., PENDING, EXECUTED, CANCELLED
    }
}