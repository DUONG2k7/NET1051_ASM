using System;
using System.Collections.Generic;

namespace ASM_1.Models.Food
{
    public class KitchenOrderItemViewModel
    {
        public int OrderItemId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public string FoodName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Note { get; set; }
        public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
    }

    public class KitchenDashboardViewModel
    {
        public List<KitchenOrderItemViewModel> PendingOrders { get; set; } = new();
        public List<KitchenOrderItemViewModel> InProgressOrders { get; set; } = new();
        public List<KitchenOrderItemViewModel> ReadyOrders { get; set; } = new();
    }

    public class OrderLineItemViewModel
    {
        public int OrderItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public OrderStatus Status { get; set; }
        public string? Note { get; set; }
        public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
    }

    public class InvoiceSummaryViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public decimal FinalAmount { get; set; }
        public bool IsPrepaid { get; set; }
        public List<OrderLineItemViewModel> Items { get; set; } = new();
    }

    public class CashierDashboardViewModel
    {
        public List<InvoiceSummaryViewModel> WaitingInvoices { get; set; } = new();
        public List<InvoiceSummaryViewModel> ReadyInvoices { get; set; } = new();
        public List<InvoiceSummaryViewModel> CompletedInvoices { get; set; } = new();
    }
}
