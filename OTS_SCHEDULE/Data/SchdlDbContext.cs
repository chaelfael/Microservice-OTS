using Microsoft.EntityFrameworkCore;
using OTS_SCHEDULE.models;

namespace OTS_SCHEDULE.Data
{
    public class SchdlDbContext : DbContext
    {
         public SchdlDbContext(DbContextOptions<SchdlDbContext>opt) : base (opt)
        {
            
        }

        public DbSet<ScheduleTask> ScheduleTasks { get; set; }
     
        
    }
}
