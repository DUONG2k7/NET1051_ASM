using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class OptionValue
    {
        [Key] public int OptionValueId { get; set; }

        [Required] public int OptionGroupId { get; set; }
        public OptionGroup OptionGroup { get; set; } = default!;

        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [StringLength(50)] public string? Code { get; set; } // mã in bếp (KDS – Kitchen Display System)

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceDelta { get; set; } = 0m;

        public bool IsDefault { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        // SCALE-only: mốc rời rạc (ví dụ 0,30,50,70,100)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ScaleValue { get; set; }
    }
}
