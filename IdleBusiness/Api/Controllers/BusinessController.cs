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
        [HttpGet("/api/business/businessbyname")]
        public async Task<IActionResult> GetABusinessByName(string businessName)
        {
            var business = await _context.Business
                .Include(s => s.Owner)
                .Include(s => s.BusinessInvestments)
                    .ThenInclude(s => s.Investment)
                        .ThenInclude(s => s.BusinessInvestments) // this is the level that contains both ends of the investment
                .Include(s => s.GroupInvestments)
                .SingleOrDefaultAsync(s => s.Name == businessName);
            if (business == null) return StatusCode(404, "Business not found");
            business.BusinessScore = business.Owner.Score;
            return Ok(JsonConvert.SerializeObject(business, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }

        // Not "perfect" random, but good enough
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/business/randombusiness")]
        public async Task<IActionResult> GetRandomBusinesses(int amountOfBusinesses)
        {
            IQueryable<Business> GetBusinesses()
            {
                var rand = new System.Random();
                var skip = (int)(rand.NextDouble() * (_context.Business.Count() - amountOfBusinesses));
                var businesses = _context.Business
                    .OrderBy(b => b.Id)
                    .Skip(skip)
                    .Include(s => s.Owner)
                    .Include(s => s.BusinessInvestments)
                        .ThenInclude(s => s.Investment)
                            .ThenInclude(s => s.BusinessInvestments) // this is the level that contains both ends of the investment
                    .Take(amountOfBusinesses);

                if (businesses == null || businesses.Count() < amountOfBusinesses) return GetBusinesses();

                return businesses;
            }

            var businesses = GetBusinesses();
            if (amountOfBusinesses <= 0) return StatusCode(400, "Invalid amount of businesses");
            return Ok(JsonConvert.SerializeObject(businesses, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
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

                var serialized = JsonConvert.SerializeObject(messages, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                // Going through and marking the requested messages as read
                messages.ForEach(s => s.ReadByBusiness = true);
                _context.Messages.UpdateRange(messages);
                await _context.SaveChangesAsync();

                return Ok(serialized);
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
                if (investmentAmount > investingBusiness.CashPerSecond) return StatusCode(400, "You do not have enough to invest");
                if (investmentAmount <= 0) return StatusCode(400, "You must invest more than $0");

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

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("/api/business/espionage")]
        public async Task<IActionResult> EspionageBusiness(int attackingBusinessId, int defendingBusinessId)
        {
            var attackingBusiness = await _context.Business.SingleOrDefaultAsync(s => s.Id == attackingBusinessId);
            var companyToEspionage = await _context.Business.SingleOrDefaultAsync(s => s.Id == defendingBusinessId);
            if (companyToEspionage.AmountEmployed < 70) return StatusCode(400, "The business you are trying to espionage is too small. You cannot espionage a business until they have 70 employees.");
            if (attackingBusiness.Cash < attackingBusiness.EspionageCost) return StatusCode(400, "You do not have enough cash to commit this espionage");
            if (attackingBusiness.Id == defendingBusinessId) return StatusCode(400, "You cannot espionage yourself");

            attackingBusiness.Cash -= attackingBusiness.EspionageCost;
            _context.Business.Update(attackingBusiness);
            await _context.SaveChangesAsync();

            var rand = new Random();
            if (((attackingBusiness.EspionageChance * 100) - (companyToEspionage.EspionageDefense * 100)) < rand.Next(0, 100)) return StatusCode(200, "Unsuccessful Espionage");

            var espionageAmountPercentage = CalculateEspionagePercentage(companyToEspionage, attackingBusiness);
            var espionageAmount = companyToEspionage.CashPerSecond * espionageAmountPercentage;

            var investment = new Investment()
            {
                InvestmentAmount = (espionageAmount),
                InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                InvestmentType = InvestmentType.Espionage,
            };
            var investorBusinessInvestment = new BusinessInvestment()
            {
                InvestmentType = InvestmentType.Espionage,
                Investment = investment,
                InvestmentDirection = InvestmentDirection.Investor,
            };
            var investeeBusinessInvestment = new BusinessInvestment()
            {
                InvestmentType = InvestmentType.Espionage,
                Investment = investment,
                InvestmentDirection = InvestmentDirection.Investee,
            };

            companyToEspionage.CashPerSecond = espionageAmount;
            companyToEspionage.EspionageDefense += .05F;

            attackingBusiness.BusinessInvestments.Add(investorBusinessInvestment);
            companyToEspionage.BusinessInvestments.Add(investeeBusinessInvestment);
            _context.Business.Update(companyToEspionage);
            _context.Business.Update(attackingBusiness);
            await _context.SaveChangesAsync();

            return StatusCode(200, "Successful Espionage!");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("/api/business/joinsector")]
        public async Task<IActionResult> JoinSector(int businessId, int sectorId)
        {
            var business = await _context.Business
                .Include(s => s.Sector)
                .SingleOrDefaultAsync(s => s.Id == businessId);
            if (business.LifeTimeEarnings < 10000000) return StatusCode(400, "You must grow more before you can join this sector.");
            if (business.Sector != null) return StatusCode(400, "You are already a part of a sector");

            var sector = await _context.Sectors.SingleOrDefaultAsync(s => s.Id == sectorId);
            business.Sector = sector;
            _context.Business.Update(business);
            await _context.SaveChangesAsync();

            return StatusCode(200, "Joined sector!");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/business/hasunreadmessages")]
        public async Task<IActionResult> HasUnreadMessages(int businessId)
        {
            var hasUnreadMessages = await _context.Messages.AnyAsync(s => s.ReceivingBusinessId == businessId && !s.ReadByBusiness);

            return Ok(hasUnreadMessages);
        }

        [NonAction]
        private double CalculateEspionagePercentage(Business businessToEspionage, Business espionagingBusiness)
        {
            var espionageAmount = espionagingBusiness.EspionageChance - businessToEspionage.EspionageDefense;
            if (espionageAmount <= 0) espionageAmount = 0;
            else if (espionageAmount >= .5) espionageAmount = .5;
            return espionageAmount;
        }

    }
}
