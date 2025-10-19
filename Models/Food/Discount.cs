using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Azure.Core.HttpHeader;

namespace ASM_1.Models.Food
{
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Percent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Invoice>? Invoices { get; set; }
        public ICollection<Combo>? Combos { get; set; }
    }
}
