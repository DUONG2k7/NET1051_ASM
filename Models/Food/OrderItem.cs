using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class OrderItem
    {
        [Key] public int OrderItemId { get; set; }
        public int? InvoiceId { get; set; }               // null khi chưa xuất hóa đơn
        public Invoice? Invoice { get; set; }
        [Required] public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; } = default!;    // món gì
        public int Quantity { get; set; } = 1;
        [Column(TypeName = "decimal(18,2)")] public decimal UnitBasePrice { get; set; }      // giá gốc hiệu lực
        //[Column(TypeName = "decimal(18,2)")] public decimal OptionsDeltaTotal { get; set; }  // tổng chênh do option
        //[Column(TypeName = "decimal(18,2)")] public decimal UnitFinalPrice { get; set; }     // base + delta
        [Column(TypeName = "decimal(18,2)")] public decimal LineTotal { get; set; }          // UnitFinalPrice * Qty
        [MaxLength(200)] public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<OrderItemOption> Options { get; set; } = new List<OrderItemOption>();
    }
}
