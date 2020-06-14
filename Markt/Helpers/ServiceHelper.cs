using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markt.Models;

namespace Markt.Helpers
{
    public class ServiceHelper
    {
        private readonly ApplicationDbContext _context;

        public ServiceHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Do(Func<Task> func)
        {
            await func();

            var result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                throw new ArgumentException("Failed to save changes");
            }
        }

        public async Task Do(Action action)
        {
            action();

            var result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                throw new ArgumentException("Failed to save changes");
            }
        }
    }
}
