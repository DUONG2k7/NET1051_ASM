using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Controllers
{
    public class CartController : BaseController
    {
        private readonly TableCodeService _tableCodeService;
        private readonly UserSessionService _userSessionService;
        private readonly ITableTrackerService _tableTracker;
        private readonly OrderNotificationService _orderNotificationService;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public CartController(
            ApplicationDbContext context,
            TableCodeService tableCodeService,
            UserSessionService userSessionService,
            ITableTrackerService tableTracker,
            OrderNotificationService orderNotificationService)
            : base(context)
        {
            _tableCodeService = tableCodeService;
            _userSessionService = userSessionService;
            _tableTracker = tableTracker;
            _orderNotificationService = orderNotificationService;
        }

        [HttpGet("{tableCode}/cart")]
        public async Task<IActionResult> Index(string tableCode)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null) return RedirectToAction("InvalidTable");

            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);
            var cart = await GetCartAsync(userId);

            return View(cart.CartItems);
        }

        [HttpGet("{tableCode}/cart/count")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CartCountValue(string tableCode)
        {
            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);
            if (userId == null)
            {
                return Content("0", "text/plain");
            }

            var count = await _context.CartItems
                .Where(ci => ci.Cart != null && ci.Cart.UserID == userId)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;

            return Content(count.ToString(), "text/plain");
        }

        [HttpGet("{tableCode}/cart/check")]
        public async Task<IActionResult> Checkout(string tableCode)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null) return RedirectToAction("InvalidTable");

            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);
            var cart = await GetCartAsync(userId);

            if (!cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", new { tableCode });
            }

            if (TempData.ContainsKey("DiscountError"))
            {
                ViewBag.DiscountError = TempData["DiscountError"];
            }

            if (TempData.ContainsKey("LastDiscountCode"))
            {
                ViewBag.LastDiscountCode = TempData.Peek("LastDiscountCode")?.ToString();
            }

            TempData.Keep("LastDiscountCode");

            return View(cart.CartItems);
        }

        [HttpGet("{tableCode}/cart/success")]
        public IActionResult Success(string tableCode)
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Food", new { tableCode });
            }
            return View();
        }

        [HttpPost("{tableCode}/cart/place-order")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string tableCode, string? paymentMethod)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null)
            {
                TempData["ErrorMessage"] = "Mã bàn không hợp lệ.";
                return RedirectToAction("InvalidTable");
            }

            var table = await _context.Tables.AsNoTracking().FirstOrDefaultAsync(t => t.TableId == tableId);
            if (table == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin bàn.";
                return RedirectToAction("InvalidTable");
            }

            string normalizedPayment = NormalizePaymentMethod(paymentMethod);
            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index), new { tableCode });
            }

            var subtotal = cart.CartItems.Sum(x => x.UnitPrice * x.Quantity);
            decimal shipping = 0m; // tuỳ chính sách giao/nhận
            var finalAmount = subtotal + shipping;

            bool isPrepaid = normalizedPayment is "momo" or "zalopay" or "vnpay";
            var nowLocal = DateTime.Now;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var foodItemIds = cart.CartItems
                    .Select(ci => ci.ProductID)
                    .Distinct()
                    .ToList();

                var foodItems = await _context.FoodItems
                    .Where(f => foodItemIds.Contains(f.FoodItemId))
                    .ToDictionaryAsync(f => f.FoodItemId);

                foreach (var cartItem in cart.CartItems)
                {
                    if (!foodItems.TryGetValue(cartItem.ProductID, out var food))
                    {
                        await tx.RollbackAsync();
                        TempData["ErrorMessage"] = "Một món ăn trong giỏ đã không còn khả dụng.";
                        return RedirectToAction(nameof(Index), new { tableCode });
                    }

                    if (!food.IsAvailable || food.StockQuantity < cartItem.Quantity)
                    {
                        await tx.RollbackAsync();
                        var remaining = Math.Max(0, food.StockQuantity);
                        TempData["ErrorMessage"] = remaining == 0
                            ? $"Món \"{food.Name}\" đã hết hàng."
                            : $"Món \"{food.Name}\" chỉ còn {remaining} suất.";
                        return RedirectToAction(nameof(Index), new { tableCode });
                    }

                    food.StockQuantity -= cartItem.Quantity;
                    if (food.StockQuantity <= 0)
                    {
                        food.StockQuantity = 0;
                        food.IsAvailable = false;
                    }
                }

                var invoice = new Invoice
                {
                    InvoiceCode = NewInvoiceCode(),
                    CreatedDate = nowLocal,
                    TotalAmount = finalAmount,
                    FinalAmount = finalAmount,
                    Status = isPrepaid ? "Paid" : "Pending",
                    Notes = null
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                var order = new Order
                {
                    OrderCode = NewOrderCode(),
                    TableId = table.TableId,
                    TableNameSnapshot = string.IsNullOrWhiteSpace(table.TableName) ? $"Bàn {table.TableId}" : table.TableName,
                    UserSessionId = userId,
                    Status = OrderStatus.Pending,
                    Note = null,
                    TotalAmount = finalAmount,
                    PaymentMethod = normalizedPayment,
                    InvoiceId = invoice.InvoiceId,
                    CreatedAt = nowLocal,
                    UpdatedAt = nowLocal
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var createdItems = new List<(OrderItem OrderItem, CartItem CartItem)>();

                foreach (var ci in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        InvoiceId = invoice.InvoiceId,
                        FoodItemId = ci.ProductID,
                        Quantity = ci.Quantity,
                        UnitBasePrice = ci.UnitPrice,
                        LineTotal = ci.UnitPrice * ci.Quantity,
                        Note = string.IsNullOrWhiteSpace(ci.Note) ? null : ci.Note,
                        CreatedAt = DateTime.UtcNow
                    };

                    createdItems.Add((orderItem, ci));
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                var optionSnapshots = new List<OrderItemOption>();

                foreach (var (orderItem, cartItem) in createdItems)
                {
                    if (cartItem.Options != null && cartItem.Options.Count > 0)
                    {
                        foreach (var opt in cartItem.Options)
                        {
                            optionSnapshots.Add(new OrderItemOption
                            {
                                OrderItemId = orderItem.OrderItemId,
                                PriceDelta = 0m,
                                OptionGroupNameSnap = opt.OptionTypeName,
                                OptionValueNameSnap = opt.OptionName,
                                OptionValueCodeSnap = null,
                                OptionGroupId = null,
                                OptionValueId = null
                            });
                        }
                    }

                    if (isPrepaid)
                    {
                        _context.InvoiceDetails.Add(new InvoiceDetail
                        {
                            InvoiceId = invoice.InvoiceId,
                            FoodItemId = cartItem.ProductID,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.UnitPrice,
                            SubTotal = cartItem.UnitPrice * cartItem.Quantity
                        });
                    }
                }

                if (optionSnapshots.Count > 0)
                {
                    _context.OrderItemOptions.AddRange(optionSnapshots);
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                await _orderNotificationService.RefreshAndBroadcastAsync(order.OrderId, true);

                TempData["OrderSuccess"] = true;
                TempData["PaymentMethod"] = normalizedPayment;
                TempData["TableName"] = order.TableNameSnapshot;
                TempData["OrderCode"] = order.OrderCode;

                return RedirectToAction(nameof(Success), new { tableCode });
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi khi đặt hàng. Vui lòng thử lại.";
                return RedirectToAction(nameof(Checkout), new { tableCode });
            }
        }

        // ===== Helpers =====

        [HttpGet("cart/status/list")]
        public async Task<IActionResult> OrderStatusList(string? tableCode)
        {
            if (string.IsNullOrWhiteSpace(tableCode))
            {
                return Json(Array.Empty<object>());
            }

            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null)
            {
                return Json(Array.Empty<object>());
            }

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.TableId == tableId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(20)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderCode,
                    o.CreatedAt,
                    o.TableNameSnapshot,
                    o.TotalAmount,
                    ItemStatuses = o.Items.Select(i => i.Status).ToList()
                })
                .ToListAsync();

            var result = orders.Select(o => new
            {
                id = o.OrderId,
                code = o.OrderCode,
                placedAt = o.CreatedAt,
                table = o.TableNameSnapshot,
                sum = o.TotalAmount,
                status = OrderStatusCalculator.CalculateFromStatuses(o.ItemStatuses).ToString()
            });

            return Json(result);
        }

        [HttpGet("cart/my-orders")]
        public async Task<IActionResult> MyOrders(string? tableCode)
        {
            if (string.IsNullOrWhiteSpace(tableCode))
            {
                return Json(Array.Empty<object>());
            }

            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserSessionId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(20)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderCode,
                    o.CreatedAt,
                    o.TableNameSnapshot,
                    o.TotalAmount,
                    ItemStatuses = o.Items.Select(i => i.Status).ToList()
                })
                .ToListAsync();

            var result = orders.Select(o => new
            {
                id = o.OrderId,
                code = o.OrderCode,
                placedAt = o.CreatedAt,
                table = o.TableNameSnapshot,
                sum = o.TotalAmount,
                status = OrderStatusCalculator.CalculateFromStatuses(o.ItemStatuses).ToString()
            });

            return Json(result);
        }

        private static string NormalizePaymentMethod(string? method)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                return "cash";
            }

            return method.Trim().ToLowerInvariant() switch
            {
                "card" or "pos" => "card",
                "momo" => "momo",
                "cash" or "counter" or "cashier" => "cash",
                _ => "cash"
            };
        }

        private static string NewInvoiceCode()
        {
            // Ví dụ: INV-20251021-153045-ABC
            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var rnd = Guid.NewGuid().ToString("N")[..3].ToUpperInvariant();
            return $"INV-{ts}-{rnd}";
        }

        private static string NewOrderCode()
        {
            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var rnd = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"ORD-{ts}-{rnd}";
        }

        [HttpPost("{tableCode}/cart/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            string tableCode,
            int id,
            [FromForm] int[]? selectedOptionIds,
            [FromForm] string? selectionsJson,
            int quantity,
            string? note = null)
        {
            var foodItem = await _context.FoodItems
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FoodItemId == id);

            if (foodItem == null)
                return NotFound();

            if (!foodItem.IsAvailable || foodItem.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Món ăn đã hết hàng.";
                return RedirectToAction(nameof(FoodController.Detail), "Food", new { tableCode, slug = foodItem.Slug });
            }

            quantity = Math.Clamp(quantity, 1, 10);

            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            var (resolvedOptions, optionsTotal) = await ResolveSelectedOptionsAsync(id, selectionsJson, selectedOptionIds);

            decimal basePrice = PricingHelper.CalculateEffectiveBasePrice(foodItem);
            decimal priceBeforeDynamic = basePrice + optionsTotal;
            decimal finalUnitPrice = priceBeforeDynamic;

            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);
            var cart = await GetCartAsync(userId);

            int existingQuantity = cart.CartItems.Where(ci => ci.ProductID == id).Sum(ci => ci.Quantity);
            int remaining = foodItem.StockQuantity - existingQuantity;

            if (remaining <= 0)
            {
                TempData["ErrorMessage"] = "Món ăn đã hết hàng.";
                return RedirectToAction(nameof(FoodController.Detail), "Food", new { tableCode, slug = foodItem.Slug });
            }

            if (quantity > remaining)
            {
                TempData["ErrorMessage"] = remaining == 1
                    ? "Chỉ còn 1 suất cho món ăn này."
                    : $"Chỉ còn {remaining} suất cho món ăn này.";
                return RedirectToAction(nameof(FoodController.Detail), "Food", new { tableCode, slug = foodItem.Slug });
            }

            var normalizedNote = (note ?? string.Empty).Trim();
            var optionSignature = BuildOptionSignature(resolvedOptions);
            var sameItem = cart.CartItems.FirstOrDefault(i =>
                i.ProductID == id &&
                string.Equals((i.Note ?? string.Empty).Trim(), normalizedNote, StringComparison.OrdinalIgnoreCase) &&
                BuildOptionSignature(i.Options ?? new List<CartItemOption>()) == optionSignature);

            if (sameItem == null)
            {
                var newItem = new CartItem
                {
                    ProductID = foodItem.FoodItemId,
                    ProductName = foodItem.Name,
                    ProductImage = foodItem.ImageUrl ?? string.Empty,
                    Note = normalizedNote,
                    Quantity = quantity,
                    BaseUnitPrice = basePrice,
                    OptionsTotal = optionsTotal,
                    UnitPrice = finalUnitPrice,
                    TotalPrice = finalUnitPrice * quantity,
                    Options = resolvedOptions.Select(opt => new CartItemOption
                    {
                        OptionTypeName = opt.OptionTypeName,
                        OptionName = opt.OptionName,
                        PriceDelta = opt.PriceDelta,
                        Quantity = opt.Quantity,
                        ScaleValue = opt.ScaleValue
                    }).ToList()
                };

                cart.CartItems.Add(newItem);
            }
            else
            {
                sameItem.Quantity += quantity;
                sameItem.TotalPrice = sameItem.UnitPrice * sameItem.Quantity;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Food", new { tableCode });
        }

        [HttpPost("{tableCode}/cart/item/{cartItemId}/remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId, string tableCode)
        {
            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);
            var cart = await GetCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(i => i.CartItemID == cartItemId);
            if (item != null)
            {
                cart.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { tableCode });
        }

        [HttpPost("{tableCode}/cart/clear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart(string tableCode)
        {
            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);
            var cart = await GetCartAsync(userId);

            if (cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { tableCode });
        }

        [HttpPost("{tableCode}/cart/item/{cartItemId}/qty/{delta}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeQuantity(string tableCode, int cartItemId, int delta)
        {
            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);
            var cart = await GetCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(i => i.CartItemID == cartItemId);
            if (item == null) return NotFound();

            item.Quantity = Math.Clamp(item.Quantity + delta, 1, 10);
            item.TotalPrice = item.UnitPrice * item.Quantity;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { tableCode });
        }

        private async Task<(List<CartItemOption> Options, decimal OptionsTotal)> ResolveSelectedOptionsAsync(int foodItemId, string? selectionsJson, int[]? legacyOptionIds)
        {
            if (!string.IsNullOrWhiteSpace(selectionsJson))
            {
                var selections = JsonSerializer.Deserialize<List<SelectionDto>>(selectionsJson, JsonOptions) ?? new List<SelectionDto>();
                var valueIds = selections.Select(s => s.OptionValueId).Where(id => id > 0).Distinct().ToList();

                if (valueIds.Count > 0)
                {
                    var optionValues = await _context.OptionValues
                        .Include(v => v.OptionGroup)
                        .Where(v => valueIds.Contains(v.OptionValueId))
                        .ToListAsync();

                    var overrides = await _context.MenuItemOptionValues
                        .Where(v => v.FoodItemId == foodItemId && valueIds.Contains(v.OptionValueId))
                        .ToDictionaryAsync(v => v.OptionValueId);

                    var lookup = optionValues.ToDictionary(v => v.OptionValueId);
                    var results = new List<CartItemOption>();
                    decimal total = 0m;

                    foreach (var selection in selections)
                    {
                        if (!lookup.TryGetValue(selection.OptionValueId, out var value)) continue;

                        var quantity = Math.Max(1, selection.Qty);
                        var priceDelta = overrides.TryGetValue(value.OptionValueId, out var ov) && ov.PriceDeltaOverride.HasValue
                            ? ov.PriceDeltaOverride.Value
                            : value.PriceDelta;

                        results.Add(new CartItemOption
                        {
                            OptionTypeName = value.OptionGroup.Name,
                            OptionName = value.Name,
                            PriceDelta = priceDelta,
                            Quantity = quantity,
                            ScaleValue = selection.ScalePicked
                        });

                        total += priceDelta * quantity;
                    }

                    return (results, total);
                }
            }

            if (legacyOptionIds != null && legacyOptionIds.Length > 0)
            {
                var foodOptions = await _context.FoodOptions
                    .Include(o => o.OptionType)
                    .AsNoTracking()
                    .Where(o => legacyOptionIds.Contains(o.FoodOptionId))
                    .ToListAsync();

                var results = foodOptions.Select(opt => new CartItemOption
                {
                    OptionTypeName = opt.OptionType?.TypeName ?? "Tùy chọn",
                    OptionName = opt.OptionName,
                    PriceDelta = opt.ExtraPrice,
                    Quantity = 1
                }).ToList();

                var total = results.Sum(o => o.PriceDelta);
                return (results, total);
            }

            return (new List<CartItemOption>(), 0m);
        }

        private static string BuildOptionSignature(IEnumerable<CartItemOption> options)
        {
            return string.Join("|", (options ?? Array.Empty<CartItemOption>())
                .OrderBy(o => o.OptionTypeName)
                .ThenBy(o => o.OptionName)
                .ThenBy(o => o.Quantity)
                .Select(o => $"{o.OptionTypeName}:{o.OptionName}:{o.Quantity}:{o.PriceDelta}:{o.ScaleValue}"));
        }

        private async Task<DiscountValidationResult> ValidateDiscountAsync(string? discountCode, List<CartItem> items)
        {
            if (string.IsNullOrWhiteSpace(discountCode))
            {
                return new DiscountValidationResult(null, 0m, null);
            }

            var normalized = discountCode.Trim();
            var discount = await _context.Discounts
                .Include(d => d.Combos!)
                    .ThenInclude(c => c.ComboDetails!)
                .FirstOrDefaultAsync(d => d.Code == normalized);

            if (discount == null)
            {
                return new DiscountValidationResult(null, 0m, "Mã giảm giá không hợp lệ.");
            }

            var now = DateTime.Now;
            if (!discount.IsActive || discount.StartDate > now || discount.EndDate < now)
            {
                return new DiscountValidationResult(discount, 0m, "Mã giảm giá đã hết hạn hoặc chưa kích hoạt.");
            }

            if (discount.Combos != null && discount.Combos.Count > 0)
            {
                var eligibleItemIds = discount.Combos
                    .SelectMany(c => c.ComboDetails ?? new List<ComboDetail>())
                    .Select(cd => cd.FoodItemId)
                    .ToHashSet();

                bool matches = items.Any(ci => eligibleItemIds.Contains(ci.ProductID));
                if (!matches)
                {
                    return new DiscountValidationResult(discount, 0m, "Mã giảm giá chỉ áp dụng cho các combo đủ điều kiện.");
                }
            }

            var subtotal = items.Sum(ci => ci.UnitPrice * ci.Quantity);
            if (subtotal <= 0)
            {
                return new DiscountValidationResult(discount, 0m, "Đơn hàng chưa đủ điều kiện để áp dụng mã giảm giá.");
            }

            var discountValue = subtotal * discount.Percent / 100m;
            if (discount.MaxAmount.HasValue)
            {
                discountValue = Math.Min(discountValue, discount.MaxAmount.Value);
            }

            discountValue = decimal.Round(discountValue, 0, MidpointRounding.AwayFromZero);

            if (discountValue <= 0)
            {
                return new DiscountValidationResult(discount, 0m, "Giá trị mã giảm giá không phù hợp với đơn hàng hiện tại.");
            }

            return new DiscountValidationResult(discount, discountValue, null);
        }

        private record SelectionDto
        {
            public int OptionValueId { get; set; }
            public int GroupId { get; set; }
            public int Qty { get; set; } = 1;
            public decimal? ScalePicked { get; set; }
            public string? Type { get; set; }
        }

        private record DiscountValidationResult(Discount? Discount, decimal DiscountAmount, string? ErrorMessage);
    }
}