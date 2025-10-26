using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Food
{
    public enum OrderStatus { Pending, Confirmed, In_Kitchen, Ready, Served, Requested_Bill, Paid, Canceled }
    public class Order
    {
        [Key] public int OrderId { get; set; }
        [Required] public int TableSessionId { get; set; }
        //public TableSession TableSession { get; set; } = default!;
        [Required] public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [MaxLength(64)] public string? CreatedByUserId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Timestamp] public byte[]? RowVersion { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
