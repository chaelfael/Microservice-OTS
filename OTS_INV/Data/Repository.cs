using System;
using System.Collections.Generic;
using System.Linq;
using OTS_INV.models;

namespace OTS_INV.Data
{
    public class Repository : IRepository
    {
        private readonly InvDbContext _context;

        // Inject your specific DbContext here
        public Repository(InvDbContext context)
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