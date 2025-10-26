using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class CartItemOption
    {
        public int CartItemOptionID { get; set; }
        public int CartItemID { get; set; }
        public string OptionTypeName { get; set; } = string.Empty; // Ví dụ: "Size", "Topping", "Đường", "Đá"
        public string OptionName { get; set; } = string.Empty;     // Ví dụ: "Lớn", "Trân châu", "Ít đường"
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceDelta { get; set; }
        public int Quantity { get; set; } = 1;
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ScaleValue { get; set; }
    }

}
