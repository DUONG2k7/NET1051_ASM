using ASM_1.Data;
using ASM_1.Models.Food;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASM_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Invoices
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Discount)
                .Include(i => i.TableInvoices)
                    .ThenInclude(ti => ti.Table)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.FoodItem)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            return View(invoices);
        }

        // GET: Admin/Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Discount)
                .Include(i => i.TableInvoices)
                    .ThenInclude(ti => ti.Table)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.FoodItem)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.InvoiceDetailFoodOptions).ThenInclude(fo => fo.FoodOption)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();
            //invoice.TotalAmount = invoice.InvoiceDetails.Sum(x => x.SubTotal);
            //if (invoice.Discount != null && invoice.Discount.Percent > 0)
            //    invoice.FinalAmount = invoice.TotalAmount - invoice.Discount.MaxAmount;
            //else
            //    invoice.FinalAmount = invoice.TotalAmount;

            return View(invoice);
        }

        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Discount)
                .Include(i => i.Orders)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            // --- Tạo nội dung hóa đơn dạng text (HTML đơn giản) ---
            var sb = new StringBuilder();
            sb.AppendLine("<html><body>");
            sb.AppendLine($"<h2 style='text-align:center;'>HÓA ĐƠN THANH TOÁN</h2>");
            sb.AppendLine($"<p><strong>Mã hóa đơn:</strong> {invoice.InvoiceCode}</p>");
            sb.AppendLine($"<p><strong>Ngày tạo:</strong> {invoice.CreatedDate:dd/MM/yyyy HH:mm}</p>");
            sb.AppendLine($"<p><strong>Trạng thái:</strong> {invoice.Status}</p>");
            sb.AppendLine("<hr>");
            sb.AppendLine("<table border='1' cellspacing='0' cellpadding='5' style='width:100%; border-collapse:collapse;'>");
            sb.AppendLine("<tr><th>Tên món</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr>");

            foreach (var order in invoice.Orders)
            {
                foreach (var item in order.Items)
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>{item.FoodItem?.Name ?? "(Ẩn danh)"}</td>");
                    sb.AppendLine($"<td>{item.Quantity}</td>");
                    sb.AppendLine($"<td>{item.UnitBasePrice:C0}</td>");
                    sb.AppendLine($"<td>{item.LineTotal:C0}</td>");
                    sb.AppendLine("</tr>");
                }
            }

            sb.AppendLine("</table>");
            sb.AppendLine("<hr>");
            sb.AppendLine($"<p><strong>Tổng tiền:</strong> {invoice.TotalAmount:C0}</p>");
            sb.AppendLine($"<p><strong>Thành tiền:</strong> {invoice.FinalAmount:C0}</p>");
            if (invoice.Discount != null)
                sb.AppendLine($"<p><strong>Mã giảm giá:</strong> {invoice.Discount.Code}</p>");
            sb.AppendLine($"<p><strong>Ghi chú:</strong> {invoice.Notes}</p>");
            sb.AppendLine("<p style='text-align:center; margin-top:40px;'><em>Cảm ơn quý khách!</em></p>");
            sb.AppendLine("</body></html>");

            // --- Chuyển sang byte[] ---
            byte[] fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
            string fileName = $"Invoice_{invoice.InvoiceCode}.doc";

            return File(fileBytes, "application/msword", fileName);
        }

        public async Task<IActionResult> Statistics()
        {
            var invoices = await _context.Invoices.ToListAsync();

            var totalInvoices = invoices.Count;
            var totalRevenue = invoices.Sum(i => i.FinalAmount);
            var totalPaid = invoices.Where(i => i.Status == "Paid").Sum(i => i.FinalAmount);
            var totalOpen = invoices.Where(i => i.Status == "Open").Sum(i => i.FinalAmount);
            var totalCanceled = invoices.Where(i => i.Status == "Canceled").Sum(i => i.FinalAmount);

            // Doanh thu theo ngày (7 ngày gần nhất)
            var revenueByDay = invoices
                .GroupBy(i => i.CreatedDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(i => i.FinalAmount)
                })
                .OrderBy(g => g.Date)
                .TakeLast(7)
                .ToList();

            var statusCounts = invoices
                .GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.TotalInvoices = totalInvoices;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.TotalOpen = totalOpen;
            ViewBag.TotalCanceled = totalCanceled;
            ViewBag.RevenueByDay = revenueByDay;
            ViewBag.StatusCounts = statusCounts;

            return View();
        }
    }
}
