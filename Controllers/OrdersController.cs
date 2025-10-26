using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TableCodeService _tableCodeService;
        private readonly UserSessionService _userSessionService;

        public OrdersController(
            ApplicationDbContext context,
            TableCodeService tableCodeService,
            UserSessionService userSessionService)
        {
            _context = context;
            _tableCodeService = tableCodeService;
            _userSessionService = userSessionService;
        }

        [HttpGet("orders/details/{id:int}")]
        public async Task<IActionResult> Details(int id, string? tableCode)
        {
            var result = await LoadOrderAsync(id, tableCode);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        [HttpGet("orders/details/{id:int}/json")]
        public async Task<IActionResult> DetailsData(int id, string? tableCode)
        {
            var result = await LoadOrderAsync(id, tableCode);
            if (result == null)
            {
                return NotFound();
            }

            var json = new
            {
                order = new
                {
                    id = result.OrderId,
                    code = result.OrderCode,
                    table = result.TableName,
                    tableCode = result.TableCode,
                    placedAt = result.CreatedAt,
                    updatedAt = result.UpdatedAt,
                    status = result.Status.ToString(),
                    total = result.TotalAmount,
                    paymentMethod = result.PaymentMethod,
                    note = result.Note,
                    items = result.Items.Select(i => new
                    {
                        id = i.OrderItemId,
                        name = i.Name,
                        quantity = i.Quantity,
                        status = i.Status.ToString(),
                        lineTotal = i.LineTotal,
                        note = i.Note,
                        options = i.Options
                    })
                }
            };

            return Json(json);
        }

        private async Task<OrderDetailsViewModel?> LoadOrderAsync(int id, string? tableCode)
        {
            if (string.IsNullOrWhiteSpace(tableCode))
            {
                return null;
            }

            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null)
            {
                return null;
            }

            var sessionId = _userSessionService.GetOrCreateUserSessionId(tableCode);

            var baseQuery = _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.FoodItem)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Options)
                .Where(o =>
                    o.OrderId == id &&
                    o.TableId == tableId);

            var order = await baseQuery
                .Where(o => o.UserSessionId == sessionId)
                .FirstOrDefaultAsync();

            order ??= await baseQuery.FirstOrDefaultAsync();

            if (order == null)
            {
                return null;
            }

            return new OrderDetailsViewModel
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                TableName = order.TableNameSnapshot,
                TableCode = tableCode,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Status = OrderStatusCalculator.Calculate(order.Items),
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                Note = order.Note,
                Items = order.Items
                    .OrderBy(i => i.CreatedAt)
                    .Select(i => new OrderDetailsItemViewModel
                    {
                        OrderItemId = i.OrderItemId,
                        Name = i.FoodItem?.Name ?? "Món",
                        Quantity = i.Quantity,
                        Status = i.Status,
                        LineTotal = i.LineTotal,
                        Note = i.Note,
                        Options = i.Options
                            .Select(o => !string.IsNullOrWhiteSpace(o.OptionValueNameSnap)
                                ? o.OptionValueNameSnap!
                                : o.OptionGroupNameSnap ?? string.Empty)
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .ToList()
                    })
                    .ToList()
            };
        }
    }
}
