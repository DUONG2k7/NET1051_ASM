namespace ASM_1.Models.Food
{
    public class Cart
    {
        public int CartID { get; set; }
        public string UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
