using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Controllers
{
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TableCodeService _tableCodeService;

        public FoodController(ApplicationDbContext context, TableCodeService tableCodeService)
        {
            _context = context;
            _tableCodeService = tableCodeService;
        }

        [HttpGet("{tableCode}")]
        public async Task<IActionResult> Index(string tableCode)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null)
            {
                return RedirectToAction("InvalidTable");
            }

            var table = await _context.Tables.FirstOrDefaultAsync(b => b.TableId == tableId);
            if (table == null)
            {
                return RedirectToAction("InvalidTable");
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserSessionId")))
            {
                HttpContext.Session.SetString("UserSessionId", Guid.NewGuid().ToString("N"));
            }

            var model = new MenuOverviewViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Combos = await _context.Combos.Include(c => c.ComboDetails!).ThenInclude(cd => cd.FoodItem).ToListAsync(),
                FoodItems = await _context.FoodItems.Include(f => f.Category).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var foodItem = await _context.FoodItems
                .Include(f => f.Category)
                .Include(f => f.FoodOptions)
                .FirstOrDefaultAsync(f => f.FoodItemId == id);
            if (foodItem == null)
            {
                return NotFound();
            }
            return View(foodItem);
        }
    }
}
