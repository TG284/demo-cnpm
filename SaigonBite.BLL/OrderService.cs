using SaigonBite.BLL.Payments;
using SaigonBite.DAL;
using SaigonBite.Models;

namespace SaigonBite.BLL;

public class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly PricingService _pricing;
    private readonly PaymentFactory _paymentFactory;

    public OrderService(IOrderRepository repo, PricingService pricing, PaymentFactory paymentFactory)
    {
        _repo = repo;
        _pricing = pricing;
        _paymentFactory = paymentFactory;
    }

    public Order PlaceOrder(User customer, int hubId, string address, List<OrderLine> lines,
        OrderPriceSnapshot snapshot, string paymentMethod, Voucher? voucher = null)
    {
        var breakdown = _pricing.CalculatePrice(lines, snapshot, voucher);
        var strategy = _paymentFactory.GetStrategy(paymentMethod, customer.Role);
        var payRes = strategy.ProcessPayment(breakdown.RoundedTotal);

        var order = new Order
        {
            CustomerID = customer.UserID,
            CustomerRole = customer.Role,
            HubID = hubId,
            DeliveryAddress = address,
            VoucherCode = voucher?.VoucherCode,
            PaymentMethod = strategy.MethodName,
            PaymentStatus = payRes.PaymentStatus,
            Status = OrderStatus.Placed,
            Lines = lines,
            Snapshot = snapshot,
            Pricing = breakdown
        };

        _repo.InsertOrder(order);
        _repo.AppendStatusHistory(order.OrderID, new OrderStatusHistoryEntry
        {
            FromState = null,
            ToState = OrderStatus.Placed,
            ActorID = customer.UserID,
            ActorRole = customer.Role,
            ChangedAtUtc7 = snapshot.PlacementUtc7,
            Reason = "Order placed"
        });
        return order;
    }

    /// <summary>BR17: Edit lines only while Placed, recalculating with the frozen placement snapshot.</summary>
    public Order EditPlacedOrder(int orderId, List<OrderLine> newLines, Voucher? voucher = null)
    {
        var order = _repo.GetOrderById(orderId) ?? throw new KeyNotFoundException("Order not found.");
        if (order.Status != OrderStatus.Placed)
            throw new InvalidOperationException("Orders can only be edited while in Placed status (BR17).");

        order.Lines = newLines;
        order.Pricing = _pricing.CalculatePrice(newLines, order.Snapshot, voucher);
        _repo.UpdateOrder(order);
        return order;
    }

    /// <summary>BR15 & BR16: Enforces the 8-state machine and actor permissions strictly in the BLL.</summary>
    public Order TransitionState(int orderId, OrderStatus targetState, User actor,
        string reason = "", bool codCashConfirmed = false)
    {
        var order = _repo.GetOrderById(orderId) ?? throw new KeyNotFoundException("Order not found.");
        var from = order.Status;

        if (from is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Failed)
            throw new InvalidOperationException($"Terminal state '{from}' cannot transition to '{targetState}' (BR15).");

        bool allowed = (from, targetState, actor.Role) switch
        {
            (OrderStatus.Placed, OrderStatus.Confirmed, UserRole.KitchenStaff) => true,
            (OrderStatus.Placed, OrderStatus.Cancelled, UserRole.LocalCustomer or UserRole.ForeignTourist) => true,
            (OrderStatus.Placed, OrderStatus.Cancelled, UserRole.KitchenStaff) when !string.IsNullOrWhiteSpace(reason) => true,
            (OrderStatus.Confirmed, OrderStatus.Preparing, UserRole.KitchenStaff) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled, UserRole.LocalCustomer or UserRole.ForeignTourist or UserRole.Admin) => true,
            (OrderStatus.Preparing, OrderStatus.Ready, UserRole.KitchenStaff) => true,
            (OrderStatus.Ready, OrderStatus.OutForDelivery, UserRole.Shipper) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Delivered, UserRole.Shipper) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Failed, UserRole.Shipper) => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"Transition {from} -> {targetState} is not allowed for {actor.Role} (BR15).");

        if (targetState == OrderStatus.Delivered && order.PaymentMethod == "COD" && !codCashConfirmed)
            throw new InvalidOperationException("COD cash confirmation is required before marking Delivered (BR14).");

        if (targetState == OrderStatus.OutForDelivery)
            order.ShipperID = actor.UserID;

        if (targetState == OrderStatus.Cancelled)
        {
            var strat = _paymentFactory.GetStrategy(order.PaymentMethod, order.CustomerRole);
            if (strat.IsOnlinePayment)
                order.PaymentStatus = strat.Refund(order.Pricing.RoundedTotal).PaymentStatus;
        }

        order.Status = targetState;
        _repo.UpdateOrder(order);
        _repo.AppendStatusHistory(order.OrderID, new OrderStatusHistoryEntry
        {
            FromState = from,
            ToState = targetState,
            ActorID = actor.UserID,
            ActorRole = actor.Role,
            ChangedAtUtc7 = DateTime.UtcNow.AddHours(7),
            Reason = reason
        });
        return order;
    }
}
