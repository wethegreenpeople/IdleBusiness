using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdleBusiness.Data;
using IdleBusiness.Helpers;
using IdleBusiness.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IdleBusiness.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly BusinessHelper _businessHelper;
        private readonly EntrepreneurHelper _entrepreneurHelper;
        private readonly ApplicationHelper _applicationHelper;

        public BusinessController(ApplicationDbContext context, ILogger<BusinessController> logger)
        {
            _context = context;
            _logger = logger;
            _businessHelper = new BusinessHelper(_context, _logger);
            _entrepreneurHelper = new EntrepreneurHelper(_context, _logger);
            _applicationHelper = new ApplicationHelper(_logger);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("{businessId}", Name = "Get")]
        public async Task<IActionResult> GetBusiness(string businessId)
        {
            _logger.LogTrace($"Get business {businessId}");
            var business = await _context.Business
                .Include(s => s.Owner)
                .Include(s => s.BusinessInvestments)
                    .ThenInclude(s => s.Investment)
                        .ThenInclude(s => s.BusinessInvestments) // this is the level that contains both ends of the investment
                .Include(s => s.GroupInvestments)
                .SingleOrDefaultAsync(s => s.Id == Convert.ToInt32(businessId));
            business.BusinessScore = business.Owner.Score;

            return Ok(JsonConvert.SerializeObject(business, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/business/update")]
        public async Task<IActionResult> UpdateBusinessGains(string businessId)
        {
            async Task<Business> Update()
            {
                var business = await _businessHelper.UpdateGainsSinceLastCheckIn(Convert.ToInt32(businessId));
                business.Owner = await _entrepreneurHelper.UpdateEntrepreneurScore(Convert.ToInt32(businessId));
                return business;
            }
            try
            {
                var business = await Update();
                return Ok(JsonConvert.SerializeObject(business, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
            catch { return StatusCode(500); }
            
            
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/business/leaderboard")]
        public async Task<IActionResult> GetTopBusinesses(int amountOfResults)
        {
            try
            {
                var topBusinesses = _context.Business.
                    Include(s => s.Owner)
                    .OrderByDescending(s => s.Owner.Score)
                    .Take(amountOfResults);
                // Modify the business score with the owner's score. These might be two seperate scores at some point
                // so I don't want to just persist the owner's score inside the business object
                await topBusinesses.ForEachAsync(s => s.BusinessScore = s.Owner.Score);

                return Ok(JsonConvert.SerializeObject(topBusinesses, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
            catch { return StatusCode(500); }


        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/business/messages")]
        public async Task<IActionResult> GetBusinessMessages(int businessId, int amountOfResults)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(s => s.ReceivingBusinessId == businessId)
                    .OrderByDescending(s => s.Id)
                    .Take(amountOfResults)
                    .ToListAsync();

                return Ok(JsonConvert.SerializeObject(messages, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
            catch { return StatusCode(500); }


        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("/api/business/invest")]
        public async Task<IActionResult> InvestInBusiness(int investingBusinessId, int investedBusinessId, double investmentAmount)
        {
            try
            {
                if (investingBusinessId == investedBusinessId) return StatusCode(400, "You cannot invest in yourself");

                var investingBusiness = await _context.Business.SingleAsync(s => s.Id == investingBusinessId);
                var investedBusiness = await _context.Business.SingleAsync(s => s.Id == investedBusinessId);
                if (investedBusiness.LifeTimeEarnings < 1000000) return StatusCode(400, "Cannot invest until you have earned 1,000,000 lifetime");
                if (investmentAmount <= 0 || investmentAmount > investingBusiness.CashPerSecond) return StatusCode(400, "You do not have enough to invest");

                var investment = new Investment()
                {
                    InvestmentAmount = investmentAmount,
                    InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                    InvestedBusinessCashAtInvestment = investedBusiness.Cash,
                    InvestedBusinessCashPerSecondAtInvestment = investedBusiness.CashPerSecond,
                    InvestmentType = InvestmentType.Investment,
                };
                var investorBusinessInvestment = new BusinessInvestment()
                {
                    Investment = investment,
                    InvestmentDirection = InvestmentDirection.Investor,
                    InvestmentType = InvestmentType.Investment
                };
                var investeeBusinessInvestment = new BusinessInvestment()
                {
                    Investment = investment,
                    InvestmentDirection = InvestmentDirection.Investee,
                    InvestmentType = InvestmentType.Investment
                };

                investingBusiness.BusinessInvestments.Add(investorBusinessInvestment);
                investedBusiness.BusinessInvestments.Add(investeeBusinessInvestment);

                investingBusiness.CashPerSecond -= investmentAmount;
                investedBusiness.CashPerSecond += investmentAmount;

                _context.Business.Update(investingBusiness);
                _context.Business.Update(investedBusiness);
                await _applicationHelper.TrySaveChangesConcurrentAsync(_context);

                return StatusCode(200);

            }
            catch { return StatusCode(500); }


        }
    }
}
