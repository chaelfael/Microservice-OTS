using System;
using System.Collections.Generic;
using System.Linq;
using OTS_ORDER.Models;

namespace OTS_ORDER.Data
{
    public class Repository : IRepository
    {
        private readonly OrderDbContext _context;

        // Inject your specific DbContext here
        public Repository(OrderDbContext context)
        {
            _context = context;
        }

        public bool SaveChanges()
        {
            return (_context.SaveChanges() >= 0);
        }

        // --- EVENTS ---
    
   

      
    }
}