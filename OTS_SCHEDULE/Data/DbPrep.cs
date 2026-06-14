using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OTS_SCHEDULE.models;

namespace OTS_SCHEDULE.Data
{
    public static class PrepDb
    {
        public static void SeedData(SchdlDbContext context, bool isProd)
        {
            if (isProd)
            {
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            if (!context.ScheduleTasks.Any())
            {
                Console.WriteLine("--> Seeding ScheduleTasks...");

                context.ScheduleTasks.AddRange(
                    new ScheduleTask { 
                        OrderNo = "ORD-1001", 
                        ExecuteAt = DateTime.UtcNow.AddMinutes(15), 
                        Status = "PENDING" 
                    },
                    new ScheduleTask { 
                        OrderNo = "ORD-1002", 
                        ExecuteAt = DateTime.UtcNow.AddMinutes(30), 
                        Status = "PENDING" 
                    }
                );

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> We already have schedule data");
            }
        }
    }
}