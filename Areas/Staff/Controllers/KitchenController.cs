using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Chef")]
    public class KitchenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderNotificationService _orderNotificationService;

        public KitchenController(ApplicationDbContext context, OrderNotificationService orderNotificationService)
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
        public async Task<IActionResult> Start(int id)
        {
            var orderItem = await _context.OrderItems
                .Include(o => o.Invoice)
                .FirstOrDefaultAsync(o => o.OrderItemId == id);

            if (orderItem == null) return NotFound();

            if (orderItem.Status == OrderStatus.Pending || orderItem.Status == OrderStatus.Confirmed)
            {
                orderItem.Status = OrderStatus.In_Kitchen;

                if (orderItem.Invoice != null)
                {
                    // (option) nạp Orders->Items rồi cập nhật trạng thái invoice theo toàn bộ item
                    await _context.Entry(orderItem.Invoice)
                        .Collection(i => i.Orders)
                        .Query().Include(o => o.Items)
                        .LoadAsync();

                    UpdateInvoiceStatus(orderItem.Invoice);
                }

                await _context.SaveChangesAsync();
                await _orderNotificationService.RefreshAndBroadcastAsync(orderItem.OrderId);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReady(int id)
        {
            var orderItem = await _context.OrderItems
                .Include(o => o.Invoice)
                    .ThenInclude(i => i.Orders)
                        .ThenInclude(o2 => o2.Items)
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.OrderItemId == id);

            if (orderItem == null) return NotFound();

            orderItem.Status = OrderStatus.Ready;

            if (orderItem.Invoice != null)
            {
                UpdateInvoiceStatus(orderItem.Invoice);
            }

            await _context.SaveChangesAsync();
            await _orderNotificationService.RefreshAndBroadcastAsync(orderItem.OrderId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DashboardData()
        {
            var model = await BuildDashboardAsync();

            var result = new
            {
                pending = model.PendingOrders.Select(MapKitchenItemDto).ToList(),
                inProgress = model.InProgressOrders.Select(MapKitchenItemDto).ToList(),
                ready = model.ReadyOrders.Select(MapKitchenItemDto).ToList()
            };

            return Json(result);
        }

        private async Task<KitchenDashboardViewModel> BuildDashboardAsync()
        {
            var orderItems = await _context.OrderItems
                .Include(o => o.FoodItem)
                .Include(o => o.Invoice)
                .Include(o => o.Options)
                .Where(o => o.Status == OrderStatus.Pending
                         || o.Status == OrderStatus.In_Kitchen
                         || o.Status == OrderStatus.Ready)
                .OrderBy(o => o.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();

            return new KitchenDashboardViewModel
            {
                PendingOrders = orderItems.Where(o => o.Status == OrderStatus.Pending)
                                             .Select(MapKitchenItem).ToList(),
                InProgressOrders = orderItems.Where(o => o.Status == OrderStatus.In_Kitchen)
                                             .Select(MapKitchenItem).ToList(),
                ReadyOrders = orderItems.Where(o => o.Status == OrderStatus.Ready)
                                             .Select(MapKitchenItem).ToList()
            };
        }

        private static KitchenOrderItemViewModel MapKitchenItem(OrderItem item)
        {
            var options = item.Options
                .Select(o => !string.IsNullOrWhiteSpace(o.OptionValueNameSnap)
                                ? o.OptionValueNameSnap!
                                : (o.OptionGroupNameSnap ?? string.Empty))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            return new KitchenOrderItemViewModel
            {
                OrderItemId = item.OrderItemId,
                InvoiceCode = item.Invoice?.InvoiceCode ?? "#",
                FoodName = item.FoodItem?.Name ?? "(Đã xóa)",
                Quantity = item.Quantity,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                Note = item.Note,
                Options = options
            };
        }

        private static object MapKitchenItemDto(KitchenOrderItemViewModel item) => new
        {
            id = item.OrderItemId,
            invoiceCode = item.InvoiceCode,
            foodName = item.FoodName,
            quantity = item.Quantity,
            status = item.Status.ToString(),
            createdAt = item.CreatedAt,
            note = item.Note,
            options = item.Options
        };

        private static void UpdateInvoiceStatus(Invoice invoice)
        {
            // Gộp tất cả OrderItem thuộc Invoice
            var allItems = invoice.Orders.SelectMany(o => o.Items);

            if (!allItems.Any())
            {
                invoice.Status = "Pending";
                return;
            }

            // Tất cả đã sẵn sàng/đã phục vụ/đã yêu cầu tính tiền/đã thanh toán
            if (allItems.All(oi => oi.Status == OrderStatus.Ready
                                 || oi.Status == OrderStatus.Served
                                 || oi.Status == OrderStatus.Requested_Bill
                                 || oi.Status == OrderStatus.Paid))
            {
                invoice.Status = "Ready";
            }
            // Có ít nhất một món đang vào bếp
            else if (allItems.Any(oi => oi.Status == OrderStatus.In_Kitchen))
            {
                invoice.Status = "In Kitchen";
            }
            // Mặc định
            else
            {
                invoice.Status = "Pending";
            }
        }
    }
}