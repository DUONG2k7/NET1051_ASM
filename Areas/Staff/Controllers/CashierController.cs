using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASM_1.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Cashier")]
    public class CashierController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderNotificationService _orderNotificationService;

        public CashierController(ApplicationDbContext context, OrderNotificationService orderNotificationService)
        {
            _context = context;
            _orderNotificationService = orderNotificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await BuildDashboardAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkServed(int invoiceId)
        {
            // Invoice -> Orders -> Items
            var invoice = await _context.Invoices
                .Include(i => i.Orders)
                    .ThenInclude(o => o.Items)
                .AsSplitQuery()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null) return NotFound();

            var items = invoice.Orders.SelectMany(o => o.Items);

            foreach (var orderItem in items.Where(oi => oi.Status == OrderStatus.Ready))
            {
                orderItem.Status = OrderStatus.Served;
            }

            // Tùy policy của bạn, có thể chỉ set Served khi TẤT CẢ item đã Served.
            invoice.Status = "Served";

            await _context.SaveChangesAsync();

            // Phát sự kiện theo Order (đúng quan hệ hiện có)
            var orderIds = invoice.Orders.Select(o => o.OrderId).Distinct().ToList();
            foreach (var orderId in orderIds)
            {
                await _orderNotificationService.RefreshAndBroadcastAsync(orderId);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Orders)
                    .ThenInclude(o => o.Items)
                .AsSplitQuery()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null) return NotFound();

            foreach (var orderItem in invoice.Orders.SelectMany(o => o.Items))
            {
                orderItem.Status = OrderStatus.Paid;
            }

            invoice.Status = "Paid";

            await _context.SaveChangesAsync();

            var orderIds = invoice.Orders.Select(o => o.OrderId).Distinct().ToList();
            foreach (var orderId in orderIds)
            {
                await _orderNotificationService.RefreshAndBroadcastAsync(orderId);
            }

            return RedirectToAction(nameof(Print), new { invoiceId });
        }

        [HttpGet]
        public async Task<IActionResult> Print(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Orders)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(oi => oi.FoodItem)
                .Include(i => i.Orders)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(oi => oi.Options)
                .AsSplitQuery()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null) return NotFound();

            var model = MapInvoice(invoice);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DashboardData()
        {
            var model = await BuildDashboardAsync();

            var result = new
            {
                waiting = model.WaitingInvoices.Select(MapInvoiceDto).ToList(),
                ready = model.ReadyInvoices.Select(MapInvoiceDto).ToList(),
                completed = model.CompletedInvoices.Select(MapInvoiceDto).ToList()
            };

            return Json(result);
        }

        private async Task<CashierDashboardViewModel> BuildDashboardAsync()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Orders)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(oi => oi.FoodItem)
                .Include(i => i.Orders)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(oi => oi.Options)
                .OrderByDescending(i => i.CreatedDate)
                .AsSplitQuery()
                .ToListAsync();

            var model = new CashierDashboardViewModel
            {
                // Nhóm theo trạng thái của chính Invoice (string) như bạn đang dùng
                WaitingInvoices = invoices
                    .Where(i => i.Status == "Pending" || i.Status == "InKitchen")
                    .Select(MapInvoice)
                    .ToList(),

                ReadyInvoices = invoices
                    .Where(i => i.Status == "Ready" || i.Status == "Served")
                    .Select(MapInvoice)
                    .ToList(),

                CompletedInvoices = invoices
                    .Where(i => i.Status == "Paid")
                    .Select(MapInvoice)
                    .ToList()
            };

            return model;
        }

        private static InvoiceSummaryViewModel MapInvoice(Invoice invoice)
        {
            var model = new InvoiceSummaryViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode,
                Status = invoice.Status,
                CreatedDate = invoice.CreatedDate,
                FinalAmount = invoice.FinalAmount,

                // Tự suy ra "is prepaid" từ phương thức thanh toán của bất kỳ Order nào
                // (không đụng tới model Invoice)
                IsPrepaid = invoice.Orders.Any(o =>
                                o.PaymentMethod is "momo" or "zalopay" or "vnpay")
            };

            var allItems = invoice.Orders.SelectMany(o => o.Items);

            model.Items = allItems
                .Select(orderItem =>
                {
                    var options = orderItem.Options
                        .Select(o => !string.IsNullOrWhiteSpace(o.OptionValueNameSnap)
                                        ? o.OptionValueNameSnap!
                                        : (o.OptionGroupNameSnap ?? string.Empty))
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();

                    return new OrderLineItemViewModel
                    {
                        OrderItemId = orderItem.OrderItemId,
                        ItemName = orderItem.FoodItem?.Name ?? "(Đã xóa)",
                        Quantity = orderItem.Quantity,
                        Status = orderItem.Status,
                        Note = orderItem.Note,
                        Options = options
                    };
                })
                .ToList();

            return model;
        }

        private static object MapInvoiceDto(InvoiceSummaryViewModel invoice)
        {
            return new
            {
                id = invoice.InvoiceId,
                code = invoice.InvoiceCode,
                status = invoice.Status,
                createdAt = invoice.CreatedDate,
                finalAmount = invoice.FinalAmount,
                isPrepaid = invoice.IsPrepaid,
                items = invoice.Items.Select(MapLineItemDto).ToList()
            };
        }

        private static object MapLineItemDto(OrderLineItemViewModel item)
        {
            return new
            {
                id = item.OrderItemId,
                name = item.ItemName,
                quantity = item.Quantity,
                status = item.Status.ToString(),
                note = item.Note,
                options = item.Options
            };
        }
    }
}