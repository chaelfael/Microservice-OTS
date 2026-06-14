using Microsoft.EntityFrameworkCore;
using OTS_ORDER.Models;

namespace OTS_ORDER.Data
{
    public class OrderDbContext : DbContext
    {
         public OrderDbContext(DbContextOptions<OrderDbContext>opt) : base (opt)
        {
            
        }

        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        
    }
}
