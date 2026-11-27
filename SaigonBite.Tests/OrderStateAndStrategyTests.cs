using SaigonBite.BLL;
using SaigonBite.BLL.Payments;
using SaigonBite.DAL;
using SaigonBite.Models;
using Xunit;

namespace SaigonBite.Tests;

public class OrderStateAndStrategyTests
{
    private static (OrderService svc, InMemoryOrderDAL repo, PaymentFactory pf) Setup()
    {
        var repo = new InMemoryOrderDAL();
        var pf = new PaymentFactory();
        return (new OrderService(repo, new PricingService(), pf), repo, pf);
    }

    private static readonly User Local = new() { UserID = 1, Role = UserRole.LocalCustomer };
    private static readonly User Tourist = new() { UserID = 2, Role = UserRole.ForeignTourist };
    private static readonly User Kitchen = new() { UserID = 3, Role = UserRole.KitchenStaff };
    private static readonly User Shipper = new() { UserID = 4, Role = UserRole.Shipper };

    [Fact] public void T21_BR13_CrossGroupPaymentRejected()
    {
        var (_, _, pf) = Setup();
        Assert.Throws<UnauthorizedAccessException>(() => pf.GetStrategy("Apple Pay", UserRole.LocalCustomer));
        Assert.Throws<UnauthorizedAccessException>(() => pf.GetStrategy("MoMo", UserRole.ForeignTourist));
    }

    [Fact] public void T23_BR15_ValidLifecycleHappyPath()
    {
        var (svc, _, _) = Setup();
        var order = svc.PlaceOrder(Local, 3, "Thu Duc", new() { new() { ItemID = 1, ItemName = "Pho", BrandCategory = "Noodles", UnitPrice = 65_000m, Quantity = 2 } },
            new() { DistanceKm = 3.0m, HubLoadPct = 22m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }, "COD");
        svc.TransitionState(order.OrderID, OrderStatus.Confirmed, Kitchen);
        svc.TransitionState(order.OrderID, OrderStatus.Preparing, Kitchen);
        svc.TransitionState(order.OrderID, OrderStatus.Ready, Kitchen);
        svc.TransitionState(order.OrderID, OrderStatus.OutForDelivery, Shipper);
        svc.TransitionState(order.OrderID, OrderStatus.Delivered, Shipper, codCashConfirmed: true);
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact] public void T24_S01_Invalid_DeliveredToPreparing()
    {
        var (svc, _, _) = Setup();
        var o = svc.PlaceOrder(Local, 3, "TD", new() { new() { ItemID = 1, UnitPrice = 60_000m, Quantity = 1 } }, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }, "MoMo");
        svc.TransitionState(o.OrderID, OrderStatus.Confirmed, Kitchen);
        svc.TransitionState(o.OrderID, OrderStatus.Preparing, Kitchen);
        svc.TransitionState(o.OrderID, OrderStatus.Ready, Kitchen);
        svc.TransitionState(o.OrderID, OrderStatus.OutForDelivery, Shipper);
        svc.TransitionState(o.OrderID, OrderStatus.Delivered, Shipper);
        Assert.Throws<InvalidOperationException>(() => svc.TransitionState(o.OrderID, OrderStatus.Preparing, Kitchen));
    }

    [Fact] public void T25_S02_Invalid_CancelledToConfirmed()
    {
        var (svc, _, _) = Setup();
        var o = svc.PlaceOrder(Local, 3, "TD", new() { new() { ItemID = 1, UnitPrice = 60_000m, Quantity = 1 } }, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }, "MoMo");
        svc.TransitionState(o.OrderID, OrderStatus.Cancelled, Local);
        Assert.Throws<InvalidOperationException>(() => svc.TransitionState(o.OrderID, OrderStatus.Confirmed, Kitchen));
    }

    [Fact] public void T26_S03_Invalid_PreparingToCancelled()
    {
        var (svc, _, _) = Setup();
        var o = svc.PlaceOrder(Local, 3, "TD", new() { new() { ItemID = 1, UnitPrice = 60_000m, Quantity = 1 } }, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }, "MoMo");
        svc.TransitionState(o.OrderID, OrderStatus.Confirmed, Kitchen);
        svc.TransitionState(o.OrderID, OrderStatus.Preparing, Kitchen);
        Assert.Throws<InvalidOperationException>(() => svc.TransitionState(o.OrderID, OrderStatus.Cancelled, Local));
    }

    [Fact] public void T27_S04_Invalid_PlacedToReady()
    {
        var (svc, _, _) = Setup();
        var o = svc.PlaceOrder(Local, 3, "TD", new() { new() { ItemID = 1, UnitPrice = 60_000m, Quantity = 1 } }, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }, "MoMo");
        Assert.Throws<InvalidOperationException>(() => svc.TransitionState(o.OrderID, OrderStatus.Ready, Kitchen));
    }

    [Fact] public void T28_S05_Invalid_ReadyToDelivered()
    {
        var (svc, _, _) = Setup();
        var o = svc.PlaceOrder(Local, 3, "TD", new() { new() { ItemID = 1, UnitPrice = 60_000m, Quantity = 1 } }, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }, "MoMo");
        svc.TransitionState(o.OrderID, OrderStatus.Confirmed, Kitchen);
        svc.TransitionState(o.OrderID, OrderStatus.Preparing, Kitchen);
        svc.TransitionState(o.OrderID, OrderStatus.Ready, Kitchen);
        Assert.Throws<InvalidOperationException>(() => svc.TransitionState(o.OrderID, OrderStatus.Delivered, Shipper));
    }
}
