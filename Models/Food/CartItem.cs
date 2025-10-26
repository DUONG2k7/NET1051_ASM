using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int CartID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;

        public List<CartItemOption> Options { get; set; } = new(); // ← Danh sách tùy chọn động

        public string Note { get; set; } = string.Empty;
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseUnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal OptionsTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AppliedDynamicFactor { get; set; }

        public Cart? Cart { get; set; }
    }
}
