using System;
using System.Collections.Generic;
using System.Linq;
using OTS_SCHEDULE.models;

namespace OTS_SCHEDULE.Data
{
    public class Repository : IRepository
    {
        private readonly SchdlDbContext _context;

        // Inject your specific DbContext here
        public Repository(SchdlDbContext context)
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