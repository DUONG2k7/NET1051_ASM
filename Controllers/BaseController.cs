using ASM_1.Data;
using ASM_1.Models.Food;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected const string SessionCartIdKey = "CartId";

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected int? GetCartIdFromSession()
        {
            return HttpContext.Session.GetInt32(SessionCartIdKey);
        }

        protected void SetCartIdToSession(int cartId)
        {
            HttpContext.Session.SetInt32(SessionCartIdKey, cartId);
        }

        protected async Task<Cart> GetOrCreateActiveCartAsync(string UserSessionId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == UserSessionId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = UserSessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        protected async Task<Cart> GetCartAsync(string UserSessionId)
        {
            var cartId = GetCartIdFromSession();
            if (cartId.HasValue)
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CartID == cartId && c.UserID == UserSessionId);

                if (cart != null) return cart;
            }

            var newCart = await GetOrCreateActiveCartAsync(UserSessionId);
            SetCartIdToSession(newCart.CartID);
            return newCart;
        }
    }
}
