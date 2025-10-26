using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public enum OrderStatus { Pending, Confirmed, In_Kitchen, Ready, Served, Requested_Bill, Paid, Canceled }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required, MaxLength(32)]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        public int TableId { get; set; }
        public Table? Table { get; set; }

        [Required, MaxLength(120)]
        public string TableNameSnapshot { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string UserSessionId { get; set; } = string.Empty;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string? Note { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(40)]
        public string? PaymentMethod { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = default!;

        //public PaymentSession? PaymentSession { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
