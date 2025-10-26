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
            var tables = await _context.Tables
                .Include(t => t.TableInvoices)
                .ThenInclude(ti => ti.Invoice)
                .ToListAsync();

            foreach (var t in tables)
            {
                if (t.Status == "Merged")
                    continue;

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

        [HttpPost]
        public async Task<IActionResult> MergeTables(int[] tableIds)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Lấy hóa đơn đang mở của từng bàn
                var activeInvoices = await _context.TableInvoices
                    .Include(ti => ti.Invoice)
                    .Where(ti => tableIds.Contains(ti.TableId) && ti.Invoice.Status == "Open")
                    .ToListAsync();

                if (activeInvoices.Count < 2)
                {
                    TempData["ErrorMessage"] = "Cần ít nhất 2 bàn có hóa đơn mở để gộp.";
                    return RedirectToAction(nameof(Index));
                }

                // ✅ Tạo hóa đơn gộp
                var mergedInvoice = new Invoice
                {
                    InvoiceCode = "MERGE-" + DateTime.Now.Ticks,
                    CreatedDate = DateTime.Now,
                    Status = "Open",
                    Notes = "Hóa đơn gộp",
                    IsMerged = true
                };
                _context.Invoices.Add(mergedInvoice);
                await _context.SaveChangesAsync();

                // Gắn các bàn vào hóa đơn gộp
                foreach (var ti in activeInvoices)
                {
                    var oldInvoice = ti.Invoice;

                    oldInvoice.Status = "Merged";
                    oldInvoice.MergeGroupId = mergedInvoice.InvoiceId;

                    var oldDetails = await _context.InvoiceDetails
                        .Where(d => d.InvoiceId == oldInvoice.InvoiceId)
                        .ToListAsync();

                    foreach (var detail in oldDetails)
                    {
                        var existingDetail = await _context.InvoiceDetails
                            .FirstOrDefaultAsync(d =>
                                d.InvoiceId == mergedInvoice.InvoiceId &&
                                d.FoodItemId == detail.FoodItemId);

                        if (existingDetail != null)
                        {
                            // Nếu món này đã có trong hóa đơn gộp → cộng dồn số lượng
                            existingDetail.Quantity += detail.Quantity;
                        }
                        else
                        {
                            // Nếu chưa có → thêm mới
                            var newDetail = new InvoiceDetail
                            {
                                InvoiceId = mergedInvoice.InvoiceId,
                                FoodItemId = detail.FoodItemId,
                                Quantity = detail.Quantity,
                                UnitPrice = detail.UnitPrice
                            };
                            _context.InvoiceDetails.Add(newDetail);
                        }
                    }

                    _context.TableInvoices.Add(new TableInvoice
                    {
                        TableId = ti.TableId,
                        InvoiceId = mergedInvoice.InvoiceId,
                        MergeGroupId = mergedInvoice.InvoiceId,
                        OldInvoiceId = oldInvoice.InvoiceId
                    });

                    var table = await _context.Tables.FindAsync(ti.TableId);
                    if (table != null)
                    {
                        table.Status = "Merged";
                        _context.Tables.Update(table);
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["SuccessMessage"] = "Đã gộp bàn thành công.";
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["ErrorMessage"] = "Lỗi khi gộp bàn: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SplitTables(int mergedInvoiceId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var mergedInvoice = await _context.Invoices
                    .Include(i => i.TableInvoices)
                    .Include(i => i.InvoiceDetails)
                    .FirstOrDefaultAsync(i => i.InvoiceId == mergedInvoiceId && i.IsMerged);

                if (mergedInvoice == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hóa đơn gộp.";
                    return RedirectToAction(nameof(Index));
                }

                // Duyệt qua từng bàn trong hóa đơn gộp
                foreach (var tableInvoice in mergedInvoice.TableInvoices)
                {
                    if (tableInvoice.OldInvoiceId != null)
                    {
                        // 🔙 Khôi phục hóa đơn cũ
                        var oldInvoice = await _context.Invoices.FindAsync(tableInvoice.OldInvoiceId);
                        if (oldInvoice != null)
                        {
                            oldInvoice.Status = "Open";
                            oldInvoice.MergeGroupId = null;
                            tableInvoice.InvoiceId = oldInvoice.InvoiceId;
                            tableInvoice.MergeGroupId = null;
                            tableInvoice.OldInvoiceId = null;

                            _context.Update(oldInvoice);
                        }
                    }
                    else
                    {
                        // Nếu bàn này không có hóa đơn cũ, tạo hóa đơn mới
                        var newInvoice = new Invoice
                        {
                            InvoiceCode = "SPLIT-" + tableInvoice.TableId + "-" + DateTime.Now.Ticks,
                            CreatedDate = DateTime.Now,
                            Status = "Open",
                            Notes = $"Tách từ hóa đơn gộp {mergedInvoice.InvoiceCode}"
                        };
                        _context.Invoices.Add(newInvoice);
                        await _context.SaveChangesAsync();

                        tableInvoice.InvoiceId = newInvoice.InvoiceId;
                        tableInvoice.MergeGroupId = null;
                        tableInvoice.OldInvoiceId = null;
                    }

                    var table = await _context.Tables.FindAsync(tableInvoice.TableId);
                    if (table != null)
                    {
                        table.Status = "Available";
                        _context.Tables.Update(table);
                    }
                }

                mergedInvoice.Status = "Split";
                mergedInvoice.IsMerged = false;
                _context.Update(mergedInvoice);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["SuccessMessage"] = "Đã tách bàn thành công.";
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["ErrorMessage"] = "Lỗi khi tách bàn: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("TableId,TableName,SeatCount")] Table updatedTable)
        {
            if (id != updatedTable.TableId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 🔹 Load entity gốc từ DB
                    var table = await _context.Tables.FindAsync(id);
                    if (table == null)
                        return NotFound();

                    // 🔹 Cập nhật các trường được phép
                    table.TableName = updatedTable.TableName;
                    table.SeatCount = updatedTable.SeatCount;

                    // 🔹 Giữ nguyên trạng thái "Merged", chỉ cập nhật nếu chưa gộp
                    if (table.Status != "Merged")
                    {
                        int guestCount = _tableTracker.GetGuestCount(table.TableId);
                        table.Status = guestCount < table.SeatCount ? "Available" : "Full";
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bàn thành công.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật: " + ex.Message;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(updatedTable);
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
