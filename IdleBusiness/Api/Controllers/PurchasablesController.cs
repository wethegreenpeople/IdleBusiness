using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdleBusiness.Data;
using IdleBusiness.Helpers;
using IdleBusiness.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IdleBusiness.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasablesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly BusinessHelper _businessHelper;
        private readonly PurchasableHelper _purchasableHelper;

        public PurchasablesController(ApplicationDbContext context, ILogger<PurchasablesController> logger)
        {
            _context = context;
            _logger = logger;
            _businessHelper = new BusinessHelper(_context, _logger);
            _purchasableHelper = new PurchasableHelper(_context);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/purchasable/getpurchase")]
        public async Task<IActionResult> GetPurchasablesWithBusinessAdjustments(string businessId, string purchasableTypeId)
        {
            _logger.LogTrace($"Get all available purchasables");
            var business = await _context.Business
                .Include(s => s.BusinessPurchases)
                .Include(s => s.Sector)
                .SingleOrDefaultAsync(s => s.Id == Convert.ToInt32(businessId));

            var purchasables = await _context
                .Purchasables
                .Where(s => s.PurchasableTypeId == Convert.ToInt32(purchasableTypeId))
                .Include(s => s.PurchasableUpgrade)
                    .ThenInclude(s => s.PurchasableUpgrade) // Hard locks us into X number of upgrades, but I'm not sure of a better way to accomplish this right now
                .Include(s => s.Type)
                .OrderBy(s => s.IsSinglePurchase)
                .ThenBy(s => s.UnlocksAtTotalEarnings)
                .Select(s => PurchasableHelper.AdjustPurchasableCostWithSectorBonus(s, business))
                .ToListAsync();

            // Dont' want to serialize the business purchases
            var adjustedPurchasbles = purchasables
                .Select(s => 
                { 
                    var amountOfPurchases = s?.BusinessPurchases?.FirstOrDefault(d => d.PurchaseId == s.Id)?.AmountOfPurchases ?? 0; 
                    s.BusinessPurchases = null; 
                    return new { Purchasable = s, AmountOfPurchases = amountOfPurchases }; 
                })
                .ToList();

            var json = JsonConvert.SerializeObject(adjustedPurchasbles, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            return Ok(json);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/purchasable/purchaseItem")]
        public async Task<IActionResult> PurchaseItem(string businessId, string purchasableId, string purchaseAmount)
        {
            _logger.LogTrace($"{businessId} buying {purchasableId}");

            var business = await _context.Business
                .Include(s => s.BusinessPurchases)
                    .ThenInclude(s => s.Purchase)
                .Include(s => s.Sector)
                .SingleOrDefaultAsync(s => s.Id == Convert.ToInt32(businessId));
            var purchasable = (await _context.Purchasables
                .Include(s => s.Type)
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Id == Convert.ToInt32(purchasableId) && s.UnlocksAtTotalEarnings <= business.LifeTimeEarnings));

            if (purchasable == null) return NotFound();
            if (Convert.ToInt32(purchaseAmount) == 0) Ok(JsonResponseHelper.PurchaseResponseWithPurchase(business, business.BusinessPurchases.SingleOrDefault(s => s.PurchaseId == purchasable.Id), null));
            purchasable = PurchasableHelper.AdjustPurchasableCostWithSectorBonus(purchasable, business);

            try
            {
                await _businessHelper.UpdateGainsSinceLastCheckIn(business.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue while trying to update gains");
                return StatusCode(500, JsonResponseHelper.PurchaseResponseWithPurchase(business, business.BusinessPurchases.SingleOrDefault(s => s.PurchaseId == purchasable.Id), null));
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }

            if (!PurchasableHelper.EnsurePurchaseIsValid(purchasable, business, Convert.ToInt32(purchaseAmount)))
                return Ok(JsonResponseHelper.PurchaseResponseWithPurchase(business, business.BusinessPurchases.SingleOrDefault(s => s.PurchaseId == purchasable.Id), null));

            business = await _purchasableHelper.ApplyItemStatsToBussiness(purchasable, business, Convert.ToInt32(purchaseAmount));
            business.LastCheckIn = DateTime.UtcNow;

            _context.Business.Update(business);

            var purchaseJson = await _purchasableHelper.PerformSpecialOnPurchaseActions(purchasable, business);

            if (purchasable.IsGlobalPurchase)
                await _purchasableHelper.ApplyGlobalPurchaseBonus(purchasable, business);
            if (purchasable.PurchasableTypeId == 4 && purchasable.AmountAvailable > 0)
            {
                var trackedPurchase = await _context.Purchasables.SingleOrDefaultAsync(s => s.Id == Convert.ToInt32(purchasableId));
                trackedPurchase.AmountAvailable -= Convert.ToInt32(purchaseAmount);
                _context.Purchasables.Update(trackedPurchase);
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(JsonResponseHelper.PurchaseResponseWithPurchase(business, business.BusinessPurchases.SingleOrDefault(s => s.PurchaseId == purchasable.Id), purchaseJson));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue while trying to buy item");
                return StatusCode(500, JsonResponseHelper.PurchaseResponseWithPurchase(business, business.BusinessPurchases.SingleOrDefault(s => s.PurchaseId == purchasable.Id), null));
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("/api/purchasable/createmarketplaceitem")]
        public async Task<IActionResult> CreateMarketplaceItem(int businessId, string itemName, int productionAmount)
        {
            if (string.IsNullOrEmpty(itemName)) return StatusCode(400, "Item name is required");

            try
            {
                var business = await _context.Business.SingleOrDefaultAsync(s => s.Id == businessId);

                var costToProduce = (business.LifeTimeEarnings * .10) * (100.0 / productionAmount);
                var costOfItem = (costToProduce / productionAmount) * .1;
                var cpsGain = ((costOfItem * .01) / productionAmount) > 1000 ? 1000 : (costOfItem * .01) / productionAmount;

                if (business == null) return StatusCode(500);
                if (business.LifeTimeEarnings < 1000000000) return StatusCode(400, "Cannot create market place items until you have reached 1 bn in lifetime earnings");
                if (business.Cash < costToProduce) return StatusCode(400, "You do not have enough cash to produce this item");
                if (await _context.Purchasables.AnyAsync(s => s.CreatedByBusinessId == businessId)) return StatusCode(400, "You've already created a marketplace item");

                var item = new Purchasable()
                {
                    Name = itemName,
                    CashModifier = cpsGain,
                    Cost = costOfItem,
                    PerOwnedModifier = 0.05,
                    PurchasableTypeId = (int)PurchasableTypeEnum.Marketplace,
                    AmountAvailable = productionAmount,
                    CreatedByBusinessId = businessId
                };

                _context.Purchasables.Add(item);
                await _context.SaveChangesAsync();

                return Ok($"Succesfully created {itemName} in the marketplace");
            }
            catch { return StatusCode(500); }


        }
    }
}
