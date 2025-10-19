using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ASM_1.Models.Food
{
    public class InvoiceDetail
    {
        [Key]
        public int InvoiceDetailId { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [Required]
        public int FoodItemId { get; set; }
        public FoodItem? FoodItem { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        public ICollection<InvoiceDetailFoodOption> InvoiceDetailFoodOptions { get; set; }
    }
}
