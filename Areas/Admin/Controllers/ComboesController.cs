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
    public class ComboesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComboesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Comboes
        public async Task<IActionResult> Index()
        {
            var combos = await _context.Combos
                .Include(c => c.ComboDetails)
                    .ThenInclude(cd => cd.FoodItem)
                .ToListAsync();
            return View(combos);
        }

        // GET: Admin/Comboes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var combo = await _context.Combos
                .Include(c => c.ComboDetails)
                    .ThenInclude(cd => cd.FoodItem)
                .FirstOrDefaultAsync(c => c.ComboId == id);

            if (combo == null) return NotFound();

            return View(combo);
        }

        // GET: Admin/Comboes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Comboes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ComboId,ComboName,Description,DiscountPercentage")] Combo combo, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "combos", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // đảm bảo thư mục tồn tại
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    combo.ImageUrl = "/uploads/combos/" + fileName;
                }

                _context.Add(combo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(combo);
        }

        // GET: Admin/Comboes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos.FindAsync(id);
            if (combo == null)
            {
                return NotFound();
            }
            return View(combo);
        }

        // POST: Admin/Comboes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComboId,ComboName,Description,DiscountPercentage")] Combo combo, IFormFile ImageFile)
        {
            if (id != combo.ComboId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "combos", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // đảm bảo thư mục tồn tại
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        combo.ImageUrl = "/uploads/combos/" + fileName;
                    }

                    _context.Update(combo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComboExists(combo.ComboId))
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
            return View(combo);
        }

        // GET: Admin/Comboes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .Include(c => c.ComboDetails)
                    .ThenInclude(cd => cd.FoodItem)
                .FirstOrDefaultAsync(c => c.ComboId == id);
            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        // POST: Admin/Comboes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo != null)
            {
                _context.Combos.Remove(combo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComboExists(int id)
        {
            return _context.Combos.Any(e => e.ComboId == id);
        }
    }
}
