namespace ASM_1.Models.Food
{
    public class MenuOverviewViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Combo> Combos { get; set; } = new();
        public List<FoodItem> FoodItems { get; set; } = new();
        public Dictionary<int, decimal> FoodPriceOverrides { get; set; } = new();
        public Dictionary<int, decimal> ComboPriceOverrides { get; set; } = new();
        public string? DynamicPricingLabel { get; set; }
        public decimal? DynamicPriceFactor { get; set; }
        public string? TableName { get; set; }
    }
}
