using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdleBusiness.Data;
using IdleBusiness.Models;
using IdleBusiness.Views.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdleBusiness.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var messages = (await GetCurrentEntrepreneur()).Business.ReceivedMessages.ToList();
            var vm = new MessageIndexVM()
            {
                Messages = messages,
            };

            _context.Messages.UpdateRange(messages.Select(s => { s.ReadByBusiness = true; return s; }));
            await _context.SaveChangesAsync();
            return View(vm);
        }

        [NonAction]
        private async Task<Entrepreneur> GetCurrentEntrepreneur()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Entrepreneurs
                .Include(s => s.Business)
                    .ThenInclude(s => s.ReceivedMessages)
                .SingleOrDefaultAsync(s => s.Id == userId);
        }
    }
}
