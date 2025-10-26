namespace ASM_1.Models.Food
{
    public class OrderViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
    }
}
