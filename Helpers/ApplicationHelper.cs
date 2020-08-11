using IdleBusiness.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class ApplicationHelper
    {
        private readonly ILogger _logger;
        public ApplicationHelper(ILogger logger)
        {
            _logger = logger;
        }

        public async Task TrySaveChangesConcurrentAsync(ApplicationDbContext context)
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue");
                throw new DbUpdateConcurrencyException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes");
                throw;
            }
        }
    }
}
