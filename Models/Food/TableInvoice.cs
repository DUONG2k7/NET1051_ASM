using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class TableInvoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TableId { get; set; }
        public Table? Table { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SplitRatio { get; set; } // 0.33 if split among 3

        public int? MergeGroupId { get; set; } // For merged tables
    }
}
