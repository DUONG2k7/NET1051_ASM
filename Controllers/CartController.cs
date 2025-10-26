using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ASM_1.Controllers
{
    public class CartController : BaseController
    {
        private readonly TableCodeService _tableCodeService;
        private readonly UserSessionService _userSessionService;

        public CartController(ApplicationDbContext context, TableCodeService tableCodeService, UserSessionService userSessionService) : base(context)
        {
            _tableCodeService = tableCodeService;
            _userSessionService = userSessionService;
        }

        [HttpGet("{tableCode}/cart")]
        public async Task<IActionResult> Index(string tableCode)
        {
            //if (!User.Identity?.IsAuthenticated ?? true)
            //{
            //    TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem giỏ hàng.";
            //    return RedirectToAction("Login", "Account");
            //}

            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null) return RedirectToAction("InvalidTable");

            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);

            var cart = await GetCartAsync(userId);

            var activeInvoice = await _context.TableInvoices
                .Include(ti => ti.Invoice)
                    .ThenInclude(inv => inv.Orders)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.FoodItem)
                .Include(ti => ti.Invoice)
                    .ThenInclude(inv => inv.Orders)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.Options)
                .Where(ti => ti.TableId == tableId && ti.Invoice.Status == "Open")
                .Select(ti => ti.Invoice)
                .FirstOrDefaultAsync();

            ViewBag.ActiveInvoice = activeInvoice;
            ViewBag.AllOrders = activeInvoice?.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToList() ?? new List<Order>();

            return View(cart.CartItems);
        }

        [HttpGet("/Cart/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.FoodItem)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return PartialView("_OrderDetailPartial", order);
        }
        [HttpGet]
        public IActionResult Checkout(int id)
        {
            return View();
        }

        [HttpGet("{tableCode}/cart/count")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CartCountValue(string tableCode)
        {
            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);

            var count = await _context.CartItems
                .Where(ci => ci.Cart != null && ci.Cart.UserID == userId)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;

            return Content(count.ToString(), "text/plain");
        }

        // THÊM MỚI: Action Checkout
        [HttpPost("{tableCode}/cart/checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string tableCode)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null)
                return RedirectToAction("InvalidTable");

            // 🧾 Lấy hóa đơn mở (chưa thanh toán)
            var invoice = await _context.TableInvoices
                .Include(ti => ti.Invoice)
                .Where(ti => ti.TableId == tableId && ti.Invoice.Status == "Open")
                .Select(ti => ti.Invoice)
                .FirstOrDefaultAsync();

            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hóa đơn cần thanh toán.";
                return RedirectToAction(nameof(Index), new { tableCode });
            }

            // 🍽️ Lấy tất cả OrderItem thuộc hóa đơn này
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.InvoiceId == invoice.InvoiceId)
                .ToListAsync();

            if (!orderItems.Any())
            {
                TempData["ErrorMessage"] = "Không có món nào trong hóa đơn.";
                return RedirectToAction(nameof(Index), new { tableCode });
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0m;

                foreach (var oi in orderItems)
                {
                    var detail = new InvoiceDetail
                    {
                        InvoiceId = invoice.InvoiceId,
                        FoodItemId = oi.FoodItemId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitBasePrice,
                        SubTotal = oi.LineTotal
                    };
                    totalAmount += oi.LineTotal;
                    _context.InvoiceDetails.Add(detail);
                }

                // Cập nhật tổng tiền hóa đơn
                invoice.TotalAmount = totalAmount;
                invoice.FinalAmount = totalAmount;
                invoice.Status = "Paying";

                // Cập nhật trạng thái bàn
                var table = await _context.Tables.FindAsync(tableId);
                if (table != null)
                    table.Status = "Available";

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["SuccessMessage"] = "Thanh toán thành công!";
                return RedirectToAction(nameof(Success), new { tableCode });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine("Checkout error: " + ex.Message);
                TempData["ErrorMessage"] = "Có lỗi khi thanh toán. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index), new { tableCode });
            }
        }

        //THÊM MỚI: Thanh toán thành công
        [HttpGet("{tableCode}/cart/success")]
        public IActionResult Success(string tableCode)
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Food", new { tableCode });
            }
            return View();
        }

        //    // THÊM MỚI: Xử lý đặt hàng
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public async Task<IActionResult> PlaceOrder(string fullName, string phone, string email,
        //string address, string city, string district, string ward, string note,
        //string deliveryTime, string paymentMethod)
        //    {
        //        // THÊM DEBUG
        //        Console.WriteLine("=== PlaceOrder method called ===");
        //        Console.WriteLine($"FullName: {fullName}");
        //        Console.WriteLine($"Phone: {phone}");
        //        Console.WriteLine($"DeliveryTime: {deliveryTime}");

        //        if (!User.Identity?.IsAuthenticated ?? true)
        //        {
        //            Console.WriteLine("User not authenticated");
        //            return RedirectToAction("Login", "Account");
        //        }

        //        string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        //        Console.WriteLine($"UserId: {userId}");

        //        var cart = await GetCartAsync(userId);

        //        if (!cart.CartItems.Any())
        //        {
        //            Console.WriteLine("Cart is empty");
        //            TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
        //            return RedirectToAction("Index");
        //        }

        //        Console.WriteLine($"Cart has {cart.CartItems.Count} items");

        //        // Xóa giỏ hàng sau khi đặt thành công
        //        _context.CartItems.RemoveRange(cart.CartItems);
        //        await _context.SaveChangesAsync();

        //        Console.WriteLine("Cart cleared successfully");

        //        // Truyền thông tin qua TempData
        //        TempData["OrderSuccess"] = true;
        //        TempData["CustomerName"] = fullName;
        //        TempData["CustomerPhone"] = phone;
        //        TempData["CustomerAddress"] = address + ", " + ward + ", " + district + ", " + city;
        //        TempData["DeliveryType"] = deliveryTime == "now" ? "Tại chỗ" : "Giao hàng";
        //        TempData["PaymentMethod"] = paymentMethod;

        //        Console.WriteLine("TempData set, redirecting to Success");

        //        return RedirectToAction("Success");
        //    }

        [HttpPost("{tableCode}/cart/place-order")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string tableCode, string paymentMethod)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null)
                return RedirectToAction("InvalidTable");

            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);

            // 1️⃣ Lấy giỏ hàng của user
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index));
            }

            // 2️⃣ Tính tổng tiền
            var subtotal = cart.CartItems.Sum(x => x.UnitPrice * x.Quantity);
            var finalAmount = subtotal; // có thể cộng thêm phí giao, VAT nếu cần

            var now = DateTime.Now;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3️⃣ Kiểm tra hóa đơn hiện có cho bàn
                var existingInvoice = await _context.TableInvoices
                    .Include(ti => ti.Invoice)
                    .Where(ti => ti.TableId == tableId)
                    .OrderByDescending(ti => ti.Invoice.CreatedDate)
                    .Select(ti => ti.Invoice)
                    .FirstOrDefaultAsync(i => i.Status == "Open" || i.Status == "Pending");

                Invoice invoice;
                if (existingInvoice == null)
                {
                    // ❌ Chưa có hóa đơn mở → tạo mới
                    invoice = new Invoice
                    {
                        InvoiceCode = NewInvoiceCode(),
                        CreatedDate = now,
                        Status = "Open", // ✅ hóa đơn đang mở
                        Notes = $"Bàn {tableId} mở bill lúc {now:HH:mm dd/MM}"
                    };

                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();

                    // Tạo liên kết Table ↔ Invoice
                    _context.TableInvoices.Add(new TableInvoice
                    {
                        TableId = tableId.Value,
                        InvoiceId = invoice.InvoiceId,
                        SplitRatio = null,
                        MergeGroupId = null
                    });
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // ✅ Đã có hóa đơn mở → dùng lại
                    invoice = existingInvoice;
                }

                // 4️⃣ Tạo Order (phiếu gọi món) thuộc về Invoice
                var order = new Order
                {
                    InvoiceId = invoice.InvoiceId,
                    Status = OrderStatus.Pending, // hoặc "New" nếu bạn thích
                    CreatedByUserId = userId,
                    Note = string.Empty,
                    CreatedAt = now
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 5️⃣ Chuyển từng CartItem → OrderItem
                foreach (var ci in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        FoodItemId = ci.ProductID,
                        Quantity = ci.Quantity,
                        UnitBasePrice = ci.UnitPrice,
                        LineTotal = ci.UnitPrice * ci.Quantity,
                        Note = ci.Note,
                        CreatedAt = now
                    };
                    _context.OrderItems.Add(orderItem);
                    await _context.SaveChangesAsync();

                    // Thêm các tùy chọn (CartItemOption → OrderItemOption)
                    if (ci.Options != null && ci.Options.Count > 0)
                    {
                        foreach (var opt in ci.Options)
                        {
                            var optEntity = new OrderItemOption
                            {
                                OrderItemId = orderItem.OrderItemId,
                                PriceDelta = 0m,
                                OptionGroupNameSnap = opt.OptionTypeName,
                                OptionValueNameSnap = opt.OptionName,
                                OptionValueCodeSnap = null,
                                OptionGroupId = null,
                                OptionValueId = null
                            };
                            _context.OrderItemOptions.Add(optEntity);
                        }
                    }
                }

                // 6️⃣ Cập nhật trạng thái bàn + hóa đơn
                var table = await _context.Tables.FindAsync(tableId);
                if (table != null)
                {
                    table.Status = "Occupied";
                    _context.Tables.Update(table);
                }

                invoice.Status = "Open";
                _context.Invoices.Update(invoice);

                // 7️⃣ Xóa giỏ hàng sau khi tạo Order
                _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["OrderSuccess"] = true;
                TempData["PaymentMethod"] = paymentMethod;

                return RedirectToAction(nameof(Success), new { tableCode });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine("Error placing order: " + ex.Message);
                TempData["ErrorMessage"] = "Có lỗi khi đặt món. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index), new { tableCode });
            }
        }

        // ===== Helpers =====

        private static string NewInvoiceCode()
        {
            // Ví dụ: INV-20251021-153045-ABC
            var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var rnd = Guid.NewGuid().ToString("N")[..3].ToUpperInvariant();
            return $"INV-{ts}-{rnd}";
        }

        [HttpPost("{tableCode}/cart/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            string tableCode,
            int id,                               // FoodItemId
            [FromForm] int[]? selectedOptionIds,  // danh sách FoodOptionId mà user chọn (nhiều loại OptionType)
            int quantity,
            string? note = null)
        {
            quantity = Math.Clamp(quantity, 1, 10);

            // 2️⃣ Lấy món ăn
            var foodItem = await _context.FoodItems
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FoodItemId == id);

            if (foodItem == null)
                return NotFound();

            // 3️⃣ Lấy danh sách Option mà người dùng chọn
            var selectedOptions = (selectedOptionIds != null && selectedOptionIds.Length > 0)
                ? await _context.FoodOptions
                    .Include(o => o.OptionType)
                    .AsNoTracking()
                    .Where(o => selectedOptionIds.Contains(o.FoodOptionId))
                    .ToListAsync()
                : new List<FoodOption>();

            // 4️⃣ Tính giá tổng (base + phụ thu)
            decimal basePrice = foodItem.DiscountPrice > 0 ? foodItem.DiscountPrice : foodItem.BasePrice;
            decimal extraPrice = selectedOptions.Sum(o => o.ExtraPrice);
            decimal unitPrice = basePrice + extraPrice;

            // 5️⃣ Lấy user ID
            string userId = _userSessionService.GetOrCreateUserSessionId(tableCode);

            // 6️⃣ Lấy hoặc tạo giỏ hàng
            var cart = await GetCartAsync(userId);

            // 7️⃣ Kiểm tra xem đã có món trùng (cùng sản phẩm, cùng option, cùng ghi chú)
            var sameItem = cart.CartItems.FirstOrDefault(i =>
                i.ProductID == id &&
                i.Options.Select(o => o.OptionTypeName + ":" + o.OptionName)
                    .OrderBy(x => x)
                    .SequenceEqual(selectedOptions
                        .Select(o => o.OptionType?.TypeName + ":" + o.OptionName)
                        .OrderBy(x => x)) &&
                string.Equals((i.Note ?? "").Trim(), (note ?? "").Trim(), StringComparison.OrdinalIgnoreCase)
            );

            // 8️⃣ Nếu chưa có thì thêm mới
            if (sameItem == null)
            {
                var newItem = new CartItem
                {
                    ProductID = foodItem.FoodItemId,
                    ProductName = foodItem.Name,
                    ProductImage = foodItem.ImageUrl ?? "",
                    Note = note?.Trim() ?? "",
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    TotalPrice = unitPrice * quantity,
                    Options = selectedOptions.Select(opt => new CartItemOption
                    {
                        OptionTypeName = opt.OptionType!.TypeName,
                        OptionName = opt.OptionName
                    }).ToList()
                };

                cart.CartItems.Add(newItem);
            }
            else
            {
                // Nếu trùng thì chỉ cộng số lượng
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

    }
}

