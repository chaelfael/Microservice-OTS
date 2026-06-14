using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OTS_ORDER.Models; // Ensure your Order, OrderDetail, and Payment models are referenced

namespace OTS_ORDER.Data
{
    public static class PrepDb
    {
        public static void SeedData(OrderDbContext context, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            // Check if Orders already exist instead of TicketClasses
            if (!context.Orders.Any())
            {
                Console.WriteLine("--> Seeding Orders, Details, and Payments...");
                
                context.Orders.AddRange(
                    // Seed Order 1: A completed VIP order
                    new Order 
                    { 
                        OrderNo = "ORD-1001", 
                        CustomerEmail = "michael@example.com", 
                        OrderDate = DateTime.UtcNow, 
                        TotalAmount = 500000, 
                        OrderStatus = "PAID",
                        OrderDetails = new List<OrderDetail>
                        {
                            new OrderDetail { TicketNumber = "VIP-001" }
                        },
                        Payments = new List<Payment>
                        {
                            new Payment { PaymentDate = DateTime.UtcNow, PaymentMethod = "CREDIT_CARD", PaymentStatus = "SUCCESS" }
                        }
                    },
                    
                    // Seed Order 2: A pending Regular order with 2 tickets
                    new Order 
                    { 
                        OrderNo = "ORD-1002", 
                        CustomerEmail = "john.doe@example.com", 
                        OrderDate = DateTime.UtcNow, 
                        TotalAmount = 300000, 
                        OrderStatus = "PENDING",
                        OrderDetails = new List<OrderDetail>
                        {
                            new OrderDetail { TicketNumber = "REG-050" },
                            new OrderDetail { TicketNumber = "REG-051" }
                        },
                        Payments = new List<Payment>
                        {
                            new Payment { PaymentDate = DateTime.UtcNow, PaymentMethod = "BANK_TRANSFER", PaymentStatus = "PENDING" }
                        }
                    }
                );

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> We already have order data");
            }
        }
    }
}