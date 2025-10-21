using ASM_1.Data;
using ASM_1.Models.Food;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ASM_1.Controllers
{
    public class CartController : BaseController
    {

        public CartController(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            var cart = await GetCartAsync(userId);
            return View(cart.CartItems);
        }

        //THÊM MỚI: Method GetCartAsync
        private new async Task<Cart> GetCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CartCountValue()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
                return Content("0", "text/plain");

            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return Content("0", "text/plain"); // hoặc xử lý khác tùy bạn
            }
            string userId = claim.Value;


            var count = await _context.CartItems
                .Where(ci => ci.Cart != null && ci.Cart.UserID == userId)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;

            return Content(count.ToString(), "text/plain");
        }

        // THÊM MỚI: Action Checkout
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var cart = await GetCartAsync(userId);

            if (!cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            return View(cart.CartItems);
        }

        //THÊM MỚI: Thanh toán thành công
        public IActionResult Success()
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // THÊM MỚI: Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string fullName, string phone, string email,
    string address, string city, string district, string ward, string note,
    string deliveryTime, string paymentMethod)
        {
            // THÊM DEBUG
            Console.WriteLine("=== PlaceOrder method called ===");
            Console.WriteLine($"FullName: {fullName}");
            Console.WriteLine($"Phone: {phone}");
            Console.WriteLine($"DeliveryTime: {deliveryTime}");

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                Console.WriteLine("User not authenticated");
                return RedirectToAction("Login", "Account");
            }

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            Console.WriteLine($"UserId: {userId}");

            var cart = await GetCartAsync(userId);

            if (!cart.CartItems.Any())
            {
                Console.WriteLine("Cart is empty");
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            Console.WriteLine($"Cart has {cart.CartItems.Count} items");

            // Xóa giỏ hàng sau khi đặt thành công
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            Console.WriteLine("Cart cleared successfully");

            // Truyền thông tin qua TempData
            TempData["OrderSuccess"] = true;
            TempData["CustomerName"] = fullName;
            TempData["CustomerPhone"] = phone;
            TempData["CustomerAddress"] = address + ", " + ward + ", " + district + ", " + city;
            TempData["DeliveryType"] = deliveryTime == "now" ? "Tại chỗ" : "Giao hàng";
            TempData["PaymentMethod"] = paymentMethod;

            Console.WriteLine("TempData set, redirecting to Success");

            return RedirectToAction("Success");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            int id,                               // FoodItemId
            [FromForm] int[]? selectedOptionIds,  // danh sách FoodOptionId mà user chọn (nhiều loại OptionType)
            int quantity,
            string? note = null)
        {
            // 1️⃣ Kiểm tra đăng nhập
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

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
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            // 6️⃣ Lấy hoặc tạo giỏ hàng
            var cart = await GetCartAsync(userId);

            // 7️⃣ Kiểm tra xem đã có món trùng (cùng sản phẩm, cùng option, cùng ghi chú)
            var sameItem = cart.CartItems.FirstOrDefault(i =>
                i.ProductID == id &&
                i.Options.Select(o => o.OptionTypeName + ":" + o.OptionName)
                    .OrderBy(x => x)
                    .SequenceEqual(selectedOptions
                        .Select(o => o.OptionType.TypeName + ":" + o.OptionName)
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
                        OptionTypeName = opt.OptionType.TypeName,
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
            return RedirectToAction("Index", "Food");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var cart = await GetCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(i => i.CartItemID == cartItemId);
            if (item != null)
            {
                cart.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var cart = await GetCartAsync(userId);

            if (cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeQuantity(int cartItemId, int delta)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var cart = await GetCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(i => i.CartItemID == cartItemId);
            if (item == null) return NotFound();

            item.Quantity = Math.Clamp(item.Quantity + delta, 1, 10);
            item.TotalPrice = item.UnitPrice * item.Quantity;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}

