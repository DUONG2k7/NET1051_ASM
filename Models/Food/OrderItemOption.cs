using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class OrderItemOption
    {
        [Key] public int Id { get; set; }

        [Required] public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = default!;

        // Luôn lưu OptionGroupId để in/ghi log rõ ràng:
        public int? OptionGroupId { get; set; }
        public OptionGroup? OptionGroup { get; set; }   
        public int? OptionValueId { get; set; }         // SCALE có thể null
        public OptionValue? OptionValue { get; set; }   
        public int? Qty { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? ScalePicked { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal PriceDelta { get; set; } = 0m;

        // SNAPSHOT (bắt buộc)
        [MaxLength(120)] public string? OptionGroupNameSnap { get; set; }
        [MaxLength(120)] public string? OptionValueNameSnap { get; set; }
        [MaxLength(50)] public string? OptionValueCodeSnap { get; set; }
    }
}
