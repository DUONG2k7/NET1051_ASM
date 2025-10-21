namespace ASM_1.Models.Food
{
    public class ProductDetailViewModel
    {
        public FoodItem Item { get; set; } = default!;
        public decimal BasePriceEffective { get; set; }  // đã áp DiscountPrice/Percent
        public List<GroupVM> Groups { get; set; } = new(); // đã merge override & loại ẩn

        public class GroupVM
        {
            public int GroupId { get; set; }
            public string Name { get; set; } = default!;
            public OptionGroupType GroupType { get; set; }
            public bool Required { get; set; }
            public int Min { get; set; }
            public int Max { get; set; }
            public decimal? ScaleMin { get; set; }
            public decimal? ScaleMax { get; set; }
            public decimal? ScaleStep { get; set; }
            public string? ScaleUnit { get; set; }
            public List<ValueVM> Values { get; set; } = new();
        }

        public class ValueVM
        {
            public int ValueId { get; set; }
            public string Name { get; set; } = default!;
            public string? Code { get; set; }
            public decimal PriceDelta { get; set; }
            public bool IsDefault { get; set; }
            public bool IsHidden { get; set; }
            public int SortOrder { get; set; }
            public decimal? ScaleValue { get; set; } // cho SCALE: mốc rời rạc 0/30/...
        }
    }
}
