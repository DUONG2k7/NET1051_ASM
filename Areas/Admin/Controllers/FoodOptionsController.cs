using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM_1.Data;
using ASM_1.Models.Food;

namespace ASM_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FoodOptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodOptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/FoodOptions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FoodOptions.Include(f => f.FoodItem).Include(f => f.OptionType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/FoodOptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodOption = await _context.FoodOptions
                .Include(f => f.FoodItem)
                .Include(f => f.OptionType)
                .FirstOrDefaultAsync(m => m.FoodOptionId == id);
            if (foodOption == null)
            {
                return NotFound();
            }

            return View(foodOption);
        }

        // GET: Admin/FoodOptions/Create
        public IActionResult Create()
        {
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "FoodItemId", "Name");
            return View();
        }

        // POST: Admin/FoodOptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FoodOptionId,FoodItemId,OptionName,ExtraPrice,OptionType,StockQuantity")] FoodOption foodOption)
        {
            if (ModelState.IsValid)
            {
                foodOption.IsAvailable = foodOption.StockQuantity > 0;
                _context.Add(foodOption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "FoodItemId", "Name", foodOption.FoodItemId);
            return View(foodOption);
        }

        // GET: Admin/FoodOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodOption = await _context.FoodOptions.FindAsync(id);
            if (foodOption == null)
            {
                return NotFound();
            }
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "FoodItemId", "Name", foodOption.FoodItemId);
            return View(foodOption);
        }

        // POST: Admin/FoodOptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FoodOptionId,FoodItemId,OptionName,ExtraPrice,OptionType,StockQuantity")] FoodOption foodOption)
        {
            if (id != foodOption.FoodOptionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    foodOption.IsAvailable = foodOption.StockQuantity > 0;
                    _context.Update(foodOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodOptionExists(foodOption.FoodOptionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "FoodItemId", "Name", foodOption.FoodItemId);
            return View(foodOption);
        }

        // GET: Admin/FoodOptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodOption = await _context.FoodOptions
                .Include(f => f.FoodItem)
                .FirstOrDefaultAsync(m => m.FoodOptionId == id);
            if (foodOption == null)
            {
                return NotFound();
            }

            return View(foodOption);
        }

        // POST: Admin/FoodOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodOption = await _context.FoodOptions.FindAsync(id);
            if (foodOption != null)
            {
                _context.FoodOptions.Remove(foodOption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FoodOptionExists(int id)
        {
            return _context.FoodOptions.Any(e => e.FoodOptionId == id);
        }
    }
}
