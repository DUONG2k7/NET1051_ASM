using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Food
{
    public class Table
    {
        [Key]
        public int TableId { get; set; }

        [Required, StringLength(50)]
        public string TableName { get; set; }

        public int SeatCount { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Available";

        // Navigation
        public ICollection<TableInvoice>? TableInvoices { get; set; }
    }
}
