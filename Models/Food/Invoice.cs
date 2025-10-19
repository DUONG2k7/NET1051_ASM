using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required, StringLength(50)]
        public string InvoiceCode { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(200)]
        public string? Notes { get; set; }

        // Navigation
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public ICollection<TableInvoice> TableInvoices { get; set; }
    }
}
