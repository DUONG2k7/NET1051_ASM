using ASM_1.Models.Food;
using System.Collections.Generic;
using System.Linq;

namespace ASM_1.Services
{
    public static class OrderStatusCalculator
    {
        public static OrderStatus Calculate(IEnumerable<OrderItem> items)
        {
            return CalculateFromStatuses((items ?? Enumerable.Empty<OrderItem>()).Select(i => i.Status));
        }

        public static OrderStatus CalculateFromStatuses(IEnumerable<OrderStatus> statusesSource)
        {
            var statuses = statusesSource?.ToList() ?? new List<OrderStatus>();
            if (statuses.Count == 0)
            {
                return OrderStatus.Pending;
            }

            if (statuses.All(s => s == OrderStatus.Canceled))
            {
                return OrderStatus.Canceled;
            }

            if (statuses.All(s => s == OrderStatus.Paid))
            {
                return OrderStatus.Paid;
            }

            if (statuses.All(s => s is OrderStatus.Paid or OrderStatus.Served))
            {
                return OrderStatus.Served;
            }

            if (statuses.Any(s => s == OrderStatus.Requested_Bill))
            {
                return OrderStatus.Requested_Bill;
            }

            if (statuses.All(s => s is OrderStatus.Ready or OrderStatus.Served or OrderStatus.Paid or OrderStatus.Requested_Bill))
            {
                return OrderStatus.Ready;
            }

            if (statuses.Any(s => s == OrderStatus.In_Kitchen))
            {
                return OrderStatus.In_Kitchen;
            }

            if (statuses.Any(s => s == OrderStatus.Confirmed))
            {
                return OrderStatus.Confirmed;
            }

            return OrderStatus.Pending;
        }
    }
}
