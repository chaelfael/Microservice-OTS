using Microsoft.EntityFrameworkCore;
using OTS_INV.models;

namespace OTS_INV.Data
{
    public class InvDbContext : DbContext
    {
         public InvDbContext(DbContextOptions<InvDbContext>opt) : base (opt)
        {
            
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketClass> TicketClasses { get; set; }
        
    }
}
