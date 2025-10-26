namespace ASM_1.Models.Food
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string TableCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
        public List<OrderDetailsItemViewModel> Items { get; set; } = new();
    }

    public class OrderDetailsItemViewModel
    {
        public int OrderItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public OrderStatus Status { get; set; }
        public decimal LineTotal { get; set; }
        public string? Note { get; set; }
        public List<string> Options { get; set; } = new();
    }
}
