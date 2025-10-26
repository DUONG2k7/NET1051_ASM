using Microsoft.AspNetCore.SignalR;

namespace ASM_1.Hubs
{
    public class OrderStatusHub : Hub
    {
        private const string KitchenGroupName = "staff:kitchen";
        private const string CashierGroupName = "staff:cashier";
        private static string TableGroup(string tableCode) => $"table:{tableCode}";
        private static string OrderGroup(int orderId) => $"order:{orderId}";

        public Task JoinTable(string? tableCode)
        {
            if (string.IsNullOrWhiteSpace(tableCode))
            {
                return Task.CompletedTask;
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, TableGroup(tableCode));
        }

        public Task LeaveTable(string? tableCode)
        {
            if (string.IsNullOrWhiteSpace(tableCode))
            {
                return Task.CompletedTask;
            }

            return Groups.RemoveFromGroupAsync(Context.ConnectionId, TableGroup(tableCode));
        }

        public Task JoinOrder(int orderId)
        {
            if (orderId <= 0)
            {
                return Task.CompletedTask;
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, OrderGroup(orderId));
        }

        public Task LeaveOrder(int orderId)
        {
            if (orderId <= 0)
            {
                return Task.CompletedTask;
            }

            return Groups.RemoveFromGroupAsync(Context.ConnectionId, OrderGroup(orderId));
        }

        public Task JoinKitchen()
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, KitchenGroupName);
        }

        public Task LeaveKitchen()
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, KitchenGroupName);
        }

        public Task JoinCashier()
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, CashierGroupName);
        }

        public Task LeaveCashier()
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, CashierGroupName);
        }
    }
}
