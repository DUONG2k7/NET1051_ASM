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
    public class ComboDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComboDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ComboDetails
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ComboDetails.Include(c => c.Combo).Include(c => c.FoodItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/ComboDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comboDetail = await _context.ComboDetails
                .Include(c => c.Combo)
                .Include(c => c.FoodItem)
                .FirstOrDefaultAsync(m => m.ComboDetailId == id);
            if (comboDetail == null)
            {
                return NotFound();
            }

            return View(comboDetail);
        }

        // GET: Admin/ComboDetails/Create
        public IActionResult Create(ComboDetail comboDetail)
        {
            ViewBag.AllFoodItems = new SelectList(_context.FoodItems, "FoodItemId", "Name", comboDetail.FoodItemId);
            ViewBag.ComboId = new SelectList(_context.Combos, "ComboId", "ComboName", comboDetail.ComboId);
            ViewBag.DiscountId = new SelectList(_context.Discounts, "DiscountId", "Code");

            return View(new Combo());
        }

        // POST: Admin/ComboDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ComboId")] ComboDetail comboDetail,
                                                List<int> SelectedFoodItems,
                                                List<int> Quantities)
        {
            if (ModelState.IsValid)
            {
                if (SelectedFoodItems != null && SelectedFoodItems.Count > 0)
                {
                    for (int i = 0; i < SelectedFoodItems.Count; i++)
                    {
                        var detail = new ComboDetail
                        {
                            ComboId = comboDetail.ComboId,
                            FoodItemId = SelectedFoodItems[i],
                            Quantity = Quantities != null && i < Quantities.Count && Quantities[i] > 0 ? Quantities[i] : 1
                        };
                        _context.Add(detail);
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ComboId"] = new SelectList(_context.Combos, "ComboId", "ComboName", comboDetail.ComboId);
            ViewData["FoodItemId"] = new SelectList(_context.FoodItems, "FoodItemId", "Name");
            return View(comboDetail);
        }

        // GET: Admin/ComboDetails/Edit/5
        public async Task<IActionResult> Edit(int? comboId)
        {
            if (comboId == null)
            {
                return NotFound();
            }

            // Lấy danh sách chi tiết combo
            var comboDetails = await _context.ComboDetails
                .Where(cd => cd.ComboId == comboId)
                .Include(cd => cd.FoodItem)
                .Include(cd => cd.Combo)
                .ToListAsync();

            if (comboDetails == null)
            {
                return NotFound();
            }

            // Lấy danh sách món ăn
            ViewBag.AllFoodItems = await _context.FoodItems
                .Select(fi => new SelectListItem
                {
                    Value = fi.FoodItemId.ToString(),
                    Text = fi.Name
                })
                .ToListAsync();

            ViewBag.ComboId = comboId;

            return View(comboDetails);
        }

        // POST: Admin/ComboDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int comboId, List<int> SelectedFoodItems, List<int> Quantities)
        {
            if (SelectedFoodItems == null || !SelectedFoodItems.Any())
            {
                ModelState.AddModelError("", "Bạn phải chọn ít nhất 1 món ăn.");
            }

            if (ModelState.IsValid)
            {
                // Xóa tất cả ComboDetail cũ của combo
                var oldDetails = _context.ComboDetails.Where(cd => cd.ComboId == comboId);
                _context.ComboDetails.RemoveRange(oldDetails);

                // Thêm ComboDetail mới
                for (int i = 0; i < SelectedFoodItems!.Count; i++)
                {
                    var detail = new ComboDetail
                    {
                        ComboId = comboId,
                        FoodItemId = SelectedFoodItems[i],
                        Quantity = Quantities != null && i < Quantities.Count && Quantities[i] > 0 ? Quantities[i] : 1
                    };
                    _context.Add(detail);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ComboId = comboId;
            ViewBag.AllFoodItems = new SelectList(_context.FoodItems, "FoodItemId", "Name");
            return View();
        }

        // GET: Admin/ComboDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comboDetail = await _context.ComboDetails
                .Include(c => c.Combo)
                .Include(c => c.FoodItem)
                .FirstOrDefaultAsync(m => m.ComboDetailId == id);
            if (comboDetail == null)
            {
                return NotFound();
            }

            return View(comboDetail);
        }

        // POST: Admin/ComboDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comboDetail = await _context.ComboDetails.FindAsync(id);
            if (comboDetail != null)
            {
                _context.ComboDetails.Remove(comboDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComboDetailExists(int id)
        {
            return _context.ComboDetails.Any(e => e.ComboDetailId == id);
        }
    }
}
