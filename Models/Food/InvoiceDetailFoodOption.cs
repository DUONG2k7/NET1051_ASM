namespace ASM_1.Models.Food
{
    public class InvoiceDetailFoodOption
    {
        public int InvoiceDetailFoodOptionId { get; set; }
        public int InvoiceDetailId { get; set; }
        public InvoiceDetail InvoiceDetail { get; set; }

        public int FoodOptionId { get; set; }
        public FoodOption FoodOption { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
