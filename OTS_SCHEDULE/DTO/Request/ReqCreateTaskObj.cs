using System;
using System.ComponentModel.DataAnnotations;

namespace OTS_SCHEDULE.Dtos
{
    public class ReqCreateTaskObj
    {
        [Required]
        public string OrderNo { get; set; } = default!;

        [Required]
        public int DelayInMinutes { get; set; }
    }
}