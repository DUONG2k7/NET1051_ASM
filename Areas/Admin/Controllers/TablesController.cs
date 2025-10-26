using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASM_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TablesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITableTrackerService _tableTracker;
        private readonly IWebHostEnvironment _env;
        private readonly TableCodeService _tableCodeService;

        public TablesController(ApplicationDbContext context, ITableTrackerService tableTracker, IWebHostEnvironment env, TableCodeService tableCodeService)
        {
            _context = context;
            _tableTracker = tableTracker;
            _env = env;
            _tableCodeService = tableCodeService;
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

            string filePath = Path.Combine(_env.WebRootPath, "uploads", "qr", $"table_{id}.png");
            bool exists = System.IO.File.Exists(filePath);

            ViewBag.QrExists = exists;
            ViewBag.QrPath = exists ? $"/uploads/qr/table_{id}.png" : null;
            return View(table);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAll()
        {
            var tables = await _context.Tables.ToListAsync();
            string qrDir = Path.Combine(_env.WebRootPath, "uploads", "qr");
            Directory.CreateDirectory(qrDir);

            foreach (var table in tables)
            {
                string code = _tableCodeService.EncryptTableId(table.TableId);
                string baseurl = $"{Request.Scheme}://{Request.Host.Value}";
                string url = $"{baseurl}/{code}";
                string filePath = Path.Combine(qrDir, $"table_{table.TableId}.png");

                if (!System.IO.File.Exists(filePath))
                {
                    var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                    var pngQr = new PngByteQRCode(qrCodeData);
                    byte[] qrBytes = pngQr.GetGraphic(20);
                    await System.IO.File.WriteAllBytesAsync(filePath, qrBytes);
                }
            }

            TempData["Success"] = "Đã tạo QR cho tất cả bàn!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DownloadQR(int id)
        {
            string qrDir = Path.Combine(_env.WebRootPath, "uploads", "qr");
            string filePath = Path.Combine(qrDir, $"table_{id}.png");

            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = $"Không tìm thấy mã QR cho bàn {id}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Lấy tên file hiển thị khi tải về
            string fileName = $"table_{id}_QR.png";
            var mimeType = "image/png";

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, mimeType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Refresh(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            string qrDir = Path.Combine(_env.WebRootPath, "uploads", "qr");
            Directory.CreateDirectory(qrDir);
            string filePath = Path.Combine(qrDir, $"table_{id}.png");

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            string code = _tableCodeService.EncryptTableId(id);
            string baseurl = $"{Request.Scheme}://{Request.Host.Value}";
            string url = $"{baseurl}/{code}";

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = pngQr.GetGraphic(20);
            await System.IO.File.WriteAllBytesAsync(filePath, qrBytes);

            TempData["Success"] = $"Đã làm mới QR cho bàn {table.TableName}";
            return RedirectToAction(nameof(Details), new { id });
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
