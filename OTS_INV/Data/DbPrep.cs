using Bogus;
using Microsoft.EntityFrameworkCore;
using OTS_INV.models;


namespace OTS_INV.Data
{
    public static class PrepDb
    {
        public static void SeedData(InvDbContext context, bool isProd)
        {
            Console.WriteLine("--> Applying Migrations...");
            context.Database.Migrate();

            // =================================================================
            // 1. SEED THE TICKET CLASSES (The Blueprints)
            // =================================================================
            if (!context.TicketClasses.Any())
            {
                Console.WriteLine("--> Seeding Ticket Classes...");
                
                var classesToSeed = new List<TicketClass>
                {
                    new TicketClass { TicketClassCode = "VIP", TicketClassName = "VIP Section", Capacity = 10000, Price = 1500000 },
                    new TicketClass { TicketClassCode = "REGULAR", TicketClassName = "Festival", Capacity = 10000, Price = 500000 },
                    new TicketClass { TicketClassCode = "VVIP", TicketClassName = "VVIP Backstage", Capacity = 10000, Price = 3000000 }
                };

                context.TicketClasses.AddRange(classesToSeed);
                
                // We MUST save here so PostgreSQL generates the TicketClassId for each row!
                context.SaveChanges(); 
                Console.WriteLine("--> Ticket Classes seeded successfully.");
            }

            // =================================================================
            // 2. SEED THE PHYSICAL TICKETS (The Inventory)
            // =================================================================
            if (!context.Tickets.Any())
            {
                Console.WriteLine("--> Seeding Physical Tickets based on Class Capacity...");
                
                // Fetch the classes from the DB so we have the real TicketClassIds
                var ticketClasses = context.TicketClasses.ToList();
                var allTicketsToInsert = new List<Ticket>();

                foreach (var tClass in ticketClasses)
                {
                    Console.WriteLine($"--> Generating exactly {tClass.Capacity} tickets for {tClass.TicketClassCode}...");

                    var ticketFaker = new Faker<Ticket>()
                        .RuleFor(t => t.TicketNumber, f => f.Commerce.Ean13()) // Fake barcode
                        .RuleFor(t => t.TicketClassId, f => tClass.TicketClassId) // Uses the correct Foreign Key!
                        .RuleFor(t => t.Status, f => "AVAILABLE")
                        .RuleFor(t => t.ReservedByOrderNo, f => "");

                    // Generate EXACTLY the number of rows as the Capacity
                    var fakeTickets = ticketFaker.Generate(tClass.Capacity);
                    allTicketsToInsert.AddRange(fakeTickets);
                }

                // Bulk insert all combined tickets
                Console.WriteLine($"--> Bulk inserting {allTicketsToInsert.Count} total tickets into PostgreSQL...");
                context.Tickets.AddRange(allTicketsToInsert);
                context.SaveChanges();

                Console.WriteLine("--> Data Seeding Complete. Inventory perfectly matches Capacity.");
            }
            else
            {
                Console.WriteLine("--> Database already contains ticket data. Skipping seeding.");
            }
        }
    }
}