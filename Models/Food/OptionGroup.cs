using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public enum OptionGroupType { SINGLE, MULTI, QUANTITY, SCALE, BOOLEAN } // enum (kiểu liệt kê)
    public class OptionGroup
    {
        [Key] public int OptionGroupId { get; set; }
        [Required, StringLength(100)] public string Name { get; set; } = default!;
        [Required] public OptionGroupType GroupType { get; set; }

        public bool Required { get; set; }
        public int MinSelect { get; set; }
        public int MaxSelect { get; set; }

        // SCALE...
        [Column(TypeName = "decimal(18,2)")] public decimal? ScaleMin { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? ScaleMax { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? ScaleStep { get; set; }
        [StringLength(10)] public string? ScaleUnit { get; set; }

        // NEW
        [StringLength(50)] public string? FamilyKey { get; set; }         // ví dụ "TOPPINGS"
        public bool IsActive { get; set; } = true;
        public bool IsArchived { get; set; } = false;
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Timestamp] public byte[]? RowVersion { get; set; }               // optimistic concurrency

        public int DisplayOrder { get; set; } = 0;
        public ICollection<OptionValue> Values { get; set; } = new List<OptionValue>();
        public ICollection<MenuItemOptionGroup> MenuItemLinks { get; set; } = new List<MenuItemOptionGroup>();
    }
}
