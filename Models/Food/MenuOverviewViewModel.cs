namespace ASM_1.Models.Food
{
    public class MenuOverviewViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Combo> Combos { get; set; } = new();
        public List<FoodItem> FoodItems { get; set; } = new();
    }
}
