using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASM_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TablesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITableTrackerService _tableTracker;

        public TablesController(ApplicationDbContext context, ITableTrackerService tableTracker)
        {
            _context = context;
            _tableTracker = tableTracker;
        }

        // GET: Admin/Tables
        public async Task<IActionResult> Index()
        {
            var tables = await _context.Tables.ToListAsync();

            foreach (var t in tables)
            {
                int guestCount = _tableTracker.GetGuestCount(t.TableId);
                t.Status = guestCount < t.SeatCount ? "Available" : "Full";
            }

            return View(tables);
        }

        // GET: Admin/Tables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables
                .FirstOrDefaultAsync(m => m.TableId == id);
            if (table == null)
            {
                return NotFound();
            }

            int guestCount = _tableTracker.GetGuestCount(table.TableId);
            table.Status = guestCount < table.SeatCount ? "Available" : "Full";

            return View(table);
        }

        // GET: Admin/Tables/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Tables/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TableId,TableName,SeatCount")] Table table)
        {
            if (ModelState.IsValid)
            {
                table.Status = "Available";
                _context.Add(table);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(table);
        }

        // GET: Admin/Tables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }
            return View(table);
        }

        // POST: Admin/Tables/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TableId,TableName,SeatCount")] Table table)
        {
            if (id != table.TableId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int guestCount = _tableTracker.GetGuestCount(table.TableId);
                    table.Status = guestCount < table.SeatCount ? "Available" : "Full";

                    _context.Update(table);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableExists(table.TableId))
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
            return View(table);
        }

        // GET: Admin/Tables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables
                .FirstOrDefaultAsync(m => m.TableId == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        // POST: Admin/Tables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table != null)
            {
                _context.Tables.Remove(table);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TableExists(int id)
        {
            return _context.Tables.Any(e => e.TableId == id);
        }
    }
}
