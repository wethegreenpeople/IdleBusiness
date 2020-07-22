using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IdleBusiness.Models;
using IdleBusiness.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using IdleBusiness.Views.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;
using IdleBusiness.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdleBusiness.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly BusinessHelper _businessHelper;
        private readonly PurchasableHelper _purchasableHelper;
        private readonly ApplicationHelper _appHelper;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _businessHelper = new BusinessHelper(_context, _logger);
            _purchasableHelper = new PurchasableHelper(_context);
            _signInManager = signInManager;
            _appHelper = new ApplicationHelper(_logger);
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeIndexVM();
            Entrepreneur user = null;
            if (User.Identity.IsAuthenticated)
            {
                user = await GetCurrentEntrepreneur();
                if (user.Business != null)
                {
                    var business = await _businessHelper.UpdateGainsSinceLastCheckIn(user.Business.Id);

                    viewModel.Business = business;

                    viewModel.Purchasables = _context
                        .Purchasables
                        .Where(s => !s.IsUpgrade)
                        .Include(s => s.Type)
                        .Include(s => s.PurchasableUpgrade)
                            .ThenInclude(s => s.PurchasableUpgrade) // Hard locks us into X number of upgrades, but I'm not sure of a better wya to accomplish this right now
                        .OrderBy(s => s.UnlocksAtTotalEarnings)
                        .Select(s => PurchasableHelper.SwapPurchaseForUpgradeIfAlreadyBought(s, user.Business))
                        .Select(s => PurchasableHelper.AdjustPurchasableCostWithSectorBonus(s, user.Business))
                        .ToList();

                    viewModel.InvestmentsInBusiness = await _businessHelper.GetInvestmentsInCompany(business.Id);
                }
            }

            viewModel.MostSuccessfulBusinesses = _context.Business.Where(s => s.Cash != 0 && s.Name != null).OrderByDescending(s => s.Cash).Take(5).ToList();
            viewModel.AvailableSectors = _context.Sectors.Select(s => new SelectListItem() { Value = s.Id.ToString(), Text = $"{s.Name} ({s.Description})" }).ToList();

            if (user != null && user.Business != null) viewModel.PurchasedItems = user.Business.BusinessPurchases.Select(s => (s.Purchase, s.AmountOfPurchases)).ToList();

            ViewData["DisplayMessageBadge"] = true;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBusiness(string businessName, int businessSector)
        {
            var user = await GetCurrentEntrepreneur();
            user.Business = new Business() { Name = businessName, Cash = 100, MaxEmployeeAmount = 50, LastCheckIn = DateTime.UtcNow, };
            _context.Entrepreneurs.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new business: {businessName} - {businessId}", user.Business.Name, user.Business.Id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseItem(int purchasableId, int purchaseCount)
        {
            var user = await GetCurrentEntrepreneur();
            var purchasable =  (await _context.Purchasables
                .Include(s => s.Type)
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Id == purchasableId && s.UnlocksAtTotalEarnings <= user.Business.LifeTimeEarnings));
            if (purchasable == null) return Ok();
            purchasable = PurchasableHelper.AdjustPurchasableCostWithSectorBonus(purchasable, user.Business);

            await _businessHelper.UpdateGainsSinceLastCheckIn(user.Business.Id);

            if (!PurchasableHelper.EnsurePurchaseIsValid(purchasable, user.Business, purchaseCount))
                return Ok();

            user.Business = await _purchasableHelper.ApplyItemStatsToBussiness(purchasable, user.Business, purchaseCount);
            user.Business.LastCheckIn = DateTime.UtcNow;

            _context.Entrepreneurs.Update(user);

            await _purchasableHelper.PerformSpecialOnPurchaseActions(purchasable, user.Business);

            if (purchasable.IsGlobalPurchase)
                await _purchasableHelper.ApplyGlobalPurchaseBonus(purchasable, user.Business);

            if (!await _appHelper.TrySaveChangesConcurrentAsync(_context)) return StatusCode(500);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddSectorToBusiness(int businessId, int businessSector)
        {
            var user = await GetCurrentEntrepreneur();
            if (user.Business.Id != businessId) return Ok();
            var sector = await _context.Sectors.SingleOrDefaultAsync(s => s.Id == businessSector);
            user.Business.Sector = sector;
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> CreateGuestAccount()
        {
            var business = new Business();
            business.Name = GenerateBusinessName();
            var user = new Entrepreneur()
            {
                Business = business,
                EmailConfirmed = true,
                LockoutEnabled = false,
                TwoFactorEnabled = false,
                UserName = business.Name,
            };

            _context.Entrepreneurs.Add(user);
            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(user, true);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [NonAction]
        private async Task<Entrepreneur> GetCurrentEntrepreneur()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Entrepreneurs
                .Include(s => s.Business)
                    .ThenInclude(s => s.BusinessPurchases)
                        .ThenInclude(s => s.Purchase)
                .Include(s => s.Business.Sector)
                .Include(s => s.Business.ReceivedMessages)
                .SingleOrDefaultAsync(s => s.Id == userId);
        }

        [NonAction]
        private string GenerateBusinessName()
        {
            string[] adjectives = { "Black", "White", "Gray", "Brown", "Red", "Pink", "Crimson", "Carnelian", "Orange", "Yellow", "Ivory", "Cream", "Green", "Viridian", "Aquamarine", "Cyan", "Blue", "Cerulean", "Azure", "Indigo", "Navy", "Violet", "Purple", "Lavender", "Magenta", "Rainbow", "Iridescent", "Spectrum", "Prism", "Bold", "Vivid", "Pale", "Clear", "Glass", "Translucent", "Misty", "Dark", "Light", "Gold", "Silver", "Copper", "Bronze", "Steel", "Iron", "Brass", "Mercury", "Zinc", "Chrome", "Platinum", "Titanium", "Nickel", "Lead", "Pewter", "Rust", "Metal", "Stone", "Quartz", "Granite", "Marble", "Alabaster", "Agate", "Jasper", "Pebble", "Pyrite", "Crystal", "Geode", "Obsidian", "Mica", "Flint", "Sand", "Gravel", "Boulder", "Basalt", "Ruby", "Beryl", "Scarlet", "Citrine", "Sulpher", "Topaz", "Amber", "Emerald", "Malachite", "Jade", "Abalone", "Lapis", "Sapphire", "Diamond", "Peridot", "Gem", "Jewel", "Bevel", "Coral", "Jet", "Ebony", "Wood", "Tree", "Cherry", "Maple", "Cedar", "Branch", "Bramble", "Rowan", "Ash", "Fir", "Pine", "Cactus", "Alder", "Grove", "Forest", "Jungle", "Palm", "Bush", "Mulberry", "Juniper", "Vine", "Ivy", "Rose", "Lily", "Tulip", "Daffodil", "Honeysuckle", "Fuschia", "Hazel", "Walnut", "Almond", "Lime", "Lemon", "Apple", "Blossom", "Bloom", "Crocus", "Rose", "Buttercup", "Dandelion", "Iris", "Carnation", "Fern", "Root", "Branch", "Leaf", "Seed", "Flower", "Petal", "Pollen", "Orchid", "Mangrove", "Cypress", "Sequoia", "Sage", "Heather", "Snapdragon", "Daisy", "Mountain", "Hill", "Alpine", "Chestnut", "Valley", "Glacier", "Forest", "Grove", "Glen", "Tree", "Thorn", "Stump", "Desert", "Canyon", "Dune", "Oasis", "Mirage", "Well", "Spring", "Meadow", "Field", "Prairie", "Grass", "Tundra", "Island", "Shore", "Sand", "Shell", "Surf", "Wave", "Foam", "Tide", "Lake", "River", "Brook", "Stream", "Pool", "Pond", "Sun", "Sprinkle", "Shade", "Shadow", "Rain", "Cloud", "Storm", "Hail", "Snow", "Sleet", "Thunder", "Lightning", "Wind", "Hurricane", "Typhoon", "Dawn", "Sunrise", "Morning", "Noon", "Twilight", "Evening", "Sunset", "Midnight", "Night", "Sky", "Star", "Stellar", "Comet", "Nebula", "Quasar", "Solar", "Lunar", "Planet", "Meteor", "Sprout", "Pear", "Plum", "Kiwi", "Berry", "Apricot", "Peach", "Mango", "Pineapple", "Coconut", "Olive", "Ginger", "Root", "Plain", "Fancy", "Stripe", "Spot", "Speckle", "Spangle", "Ring", "Band", "Blaze", "Paint", "Pinto", "Shade", "Tabby", "Brindle", "Patch", "Calico", "Checker", "Dot", "Pattern", "Glitter", "Glimmer", "Shimmer", "Dull", "Dust", "Dirt", "Glaze", "Scratch", "Quick", "Swift", "Fast", "Slow", "Clever", "Fire", "Flicker", "Flash", "Spark", "Ember", "Coal", "Flame", "Chocolate", "Vanilla", "Sugar", "Spice", "Cake", "Pie", "Cookie", "Candy", "Caramel", "Spiral", "Round", "Jelly", "Square", "Narrow", "Long", "Short", "Small", "Tiny", "Big", "Giant", "Great", "Atom", "Peppermint", "Mint", "Butter", "Fringe", "Rag", "Quilt", "Truth", "Lie", "Holy", "Curse", "Noble", "Sly", "Brave", "Shy", "Lava", "Foul", "Leather", "Fantasy", "Keen", "Luminous", "Feather", "Sticky", "Gossamer", "Cotton", "Rattle", "Silk", "Satin", "Cord", "Denim", "Flannel", "Plaid", "Wool", "Linen", "Silent", "Flax", "Weak", "Valiant", "Fierce", "Gentle", "Rhinestone", "Splash", "North", "South", "East", "West", "Summer", "Winter", "Autumn", "Spring", "Season", "Equinox", "Solstice", "Paper", "Motley", "Torch", "Ballistic", "Rampant", "Shag", "Freckle", "Wild", "Free", "Chain", "Sheer", "Crazy", "Mad", "Candle", "Ribbon", "Lace", "Notch", "Wax", "Shine", "Shallow", "Deep", "Bubble", "Harvest", "Fluff", "Venom", "Boom", "Slash", "Rune", "Cold", "Quill", "Love", "Hate", "Garnet", "Zircon", "Power", "Bone", "Void", "Horn", "Glory", "Cyber", "Nova", "Hot", "Helix", "Cosmic", "Quark", "Quiver", "Holly", "Clover", "Polar", "Regal", "Ripple", "Ebony", "Wheat", "Phantom", "Dew", "Chisel", "Crack", "Chatter", "Laser", "Foil", "Tin", "Clever", "Treasure", "Maze", "Twisty", "Curly", "Fortune", "Fate", "Destiny", "Cute", "Slime", "Ink", "Disco", "Plume", "Time", "Psychadelic", "Relic", "Fossil", "Water", "Savage", "Ancient", "Rapid", "Road", "Trail", "Stitch", "Button", "Bow", "Nimble", "Zest", "Sour", "Bitter", "Phase", "Fan", "Frill", "Plump", "Pickle", "Mud", "Puddle", "Pond", "River", "Spring", "Stream", "Battle", "Arrow", "Plume", "Roan", "Pitch", "Tar", "Cat", "Dog", "Horse", "Lizard", "Bird", "Fish", "Saber", "Scythe", "Sharp", "Soft", "Razor", "Neon", "Dandy", "Weed", "Swamp", "Marsh", "Bog", "Peat", "Moor", "Muck", "Mire", "Grave", "Fair", "Just", "Brick", "Puzzle", "Skitter", "Prong", "Fork", "Dent", "Dour", "Warp", "Luck", "Coffee", "Split", "Chip", "Hollow", "Heavy", "Legend", "Hickory", "Mesquite", "Nettle", "Rogue", "Charm", "Prickle", "Bead", "Sponge", "Whip", "Bald", "Frost", "Fog", "Oil", "Veil", "Cliff", "Volcano", "Rift", "Maze", "Proud", "Dew", "Mirror", "Shard", "Salt", "Pepper", "Honey", "Thread", "Bristle", "Ripple", "Glow", "Zenith" };
            string[] nouns = { "head", "crest", "crown", "tooth", "fang", "horn", "frill", "skull", "bone", "tongue", "throat", "voice", "nose", "snout", "chin", "eye", "sight", "seer", "speaker", "singer", "song", "chanter", "howler", "chatter", "shrieker", "shriek", "jaw", "bite", "biter", "neck", "shoulder", "fin", "wing", "arm", "lifter", "grasp", "grabber", "hand", "paw", "foot", "finger", "toe", "thumb", "talon", "palm", "touch", "racer", "runner", "hoof", "fly", "flier", "swoop", "roar", "hiss", "hisser", "snarl", "dive", "diver", "rib", "chest", "back", "ridge", "leg", "legs", "tail", "beak", "walker", "lasher", "swisher", "carver", "kicker", "roarer", "crusher", "spike", "shaker", "charger", "hunter", "weaver", "crafter", "binder", "scribe", "muse", "snap", "snapper", "slayer", "stalker", "track", "tracker", "scar", "scarer", "fright", "killer", "death", "doom", "healer", "saver", "friend", "foe", "guardian", "thunder", "lightning", "cloud", "storm", "forger", "scale", "hair", "braid", "nape", "belly", "thief", "stealer", "reaper", "giver", "taker", "dancer", "player", "gambler", "twister", "turner", "painter", "dart", "drifter", "sting", "stinger", "venom", "spur", "ripper", "swallow", "devourer", "knight", "lady", "lord", "queen", "king", "master", "mistress", "prince", "princess", "duke", "dutchess", "samurai", "ninja", "knave", "slave", "servant", "sage", "wizard", "witch", "warlock", "warrior", "jester", "paladin", "bard", "trader", "sword", "shield", "knife", "dagger", "arrow", "bow", "fighter", "bane", "follower", "leader", "scourge", "watcher", "cat", "panther", "tiger", "cougar", "puma", "jaguar", "ocelot", "lynx", "lion", "leopard", "ferret", "weasel", "wolverine", "bear", "raccoon", "dog", "wolf", "kitten", "puppy", "cub", "fox", "hound", "terrier", "coyote", "hyena", "jackal", "pig", "horse", "donkey", "stallion", "mare", "zebra", "antelope", "gazelle", "deer", "buffalo", "bison", "boar", "elk", "whale", "dolphin", "shark", "fish", "minnow", "salmon", "ray", "fisher", "otter", "gull", "duck", "goose", "crow", "raven", "bird", "eagle", "raptor", "hawk", "falcon", "moose", "heron", "owl", "stork", "crane", "sparrow", "robin", "parrot", "cockatoo", "carp", "lizard", "gecko", "iguana", "snake", "python", "viper", "boa", "condor", "vulture", "spider", "fly", "scorpion", "heron", "oriole", "toucan", "bee", "wasp", "hornet", "rabbit", "bunny", "hare", "brow", "mustang", "ox", "piper", "soarer", "flasher", "moth", "mask", "hide", "hero", "antler", "chill", "chiller", "gem", "ogre", "myth", "elf", "fairy", "pixie", "dragon", "griffin", "unicorn", "pegasus", "sprite", "fancier", "chopper", "slicer", "skinner", "butterfly", "legend", "wanderer", "rover", "raver", "loon", "lancer", "glass", "glazer", "flame", "crystal", "lantern", "lighter", "cloak", "bell", "ringer", "keeper", "centaur", "bolt", "catcher", "whimsey", "quester", "rat", "mouse", "serpent", "wyrm", "gargoyle", "thorn", "whip", "rider", "spirit", "sentry", "bat", "beetle", "burn", "cowl", "stone", "gem", "collar", "mark", "grin", "scowl", "spear", "razor", "edge", "seeker", "jay", "ape", "monkey", "gorilla", "koala", "kangaroo", "yak", "sloth", "ant", "roach", "weed", "seed", "eater", "razor", "shirt", "face", "goat", "mind", "shift", "rider", "face", "mole", "vole", "pirate", "llama", "stag", "bug", "cap", "boot", "drop", "hugger", "sargent", "snagglefoot", "carpet", "curtain" };

            Random rand = new Random();
            var adj = adjectives[rand.Next(0, adjectives.Length)];
            var noun = adjectives[rand.Next(0, nouns.Length)];
            return $"{adj}-{noun}";
        }
    }
}
