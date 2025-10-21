using ASM_1.Data;
using ASM_1.Models.Food;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Controllers
{
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new MenuOverviewViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Combos = await _context.Combos.Include(c => c.ComboDetails!).ThenInclude(cd => cd.FoodItem).ToListAsync(),
                FoodItems = await _context.FoodItems.Include(f => f.Category).ToListAsync()
            };

            return View(model);
        }
        public async Task<IActionResult> Index1()
        {
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
