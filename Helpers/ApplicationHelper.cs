using IdleBusiness.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class ApplicationHelper
    {
        public static async Task<bool> TrySaveChangesConcurrentAsync(ApplicationDbContext context)
        {
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            { }

            return false;
        }
    }
}
