using System.Diagnostics;
using SaigonBite.BLL;
using SaigonBite.BLL.Payments;
using SaigonBite.Models;
using Xunit;

namespace SaigonBite.Tests;

public class PricingServiceTests
{
    private readonly PricingService _pricing = new();
    private static OrderLine Line(decimal price, int qty = 1) =>
        new() { ItemID = 1, ItemName = "Pho Bo Tai", BrandCategory = "Noodles", UnitPrice = price, Quantity = qty };

    [Fact] public void T01_BR1_SubtotalBelow50k_Throws() =>
        Assert.Throws<ArgumentException>(() => _pricing.CalculatePrice(new[] { Line(49_999m) }, new() { DistanceKm = 2.0m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }));

    [Fact] public void T02_BR1_SubtotalExact50k_Accepted()
    {
        var r = _pricing.CalculatePrice(new[] { Line(50_000m) }, new() { DistanceKm = 2.0m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) });
        Assert.Equal(65_000m, r.RoundedTotal);
    }

    [Fact] public void T03_BR2_Distance8km_Accepted()
    {
        var r = _pricing.CalculatePrice(new[] { Line(100_000m) }, new() { DistanceKm = 8.0m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) });
        Assert.Equal(135_000m, r.RoundedTotal);
    }

    [Fact] public void T04_BR2_Distance8_01km_Throws() =>
        Assert.Throws<ArgumentException>(() => _pricing.CalculatePrice(new[] { Line(100_000m) }, new() { DistanceKm = 8.01m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }));

    [Theory]
    [InlineData(2.0, 15000)]  // T05
    [InlineData(2.01, 25000)] // T06
    [InlineData(5.01, 35000)] // T07
    public void T05_to_T07_BR3_DeliveryBands(decimal km, decimal expectedD)
    {
        var r = _pricing.CalculatePrice(new[] { Line(100_000m) }, new() { DistanceKm = km, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) });
        Assert.Equal(expectedD, r.DeliveryFeeD);
    }

    [Fact] public void T08_BR4_RushHourStart1100_Applies20Percent()
    {
        var r = _pricing.CalculatePrice(new[] { Line(100_000m) }, new() { DistanceKm = 3.0m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 11, 0, 0) });
        Assert.Equal(5_000m, r.RushSurchargeR);
    }

    [Fact] public void T09_BR4_RushHourEnd1300_Excluded()
    {
        var r = _pricing.CalculatePrice(new[] { Line(100_000m) }, new() { DistanceKm = 3.0m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 13, 0, 0) });
        Assert.Equal(0m, r.RushSurchargeR);
    }

    [Fact] public void T10_BR6_Subtotal299999_NoFreeDelivery()
    {
        var r = _pricing.CalculatePrice(new[] { Line(299_999m) }, new() { DistanceKm = 3.0m, HubLoadPct = 50m, HeavyRainFlag = true, PlacementUtc7 = new(2026, 11, 21, 11, 30, 0) });
        Assert.Equal(25_000m, r.DeliveryFeeD);
        Assert.Equal(340_000m, r.RoundedTotal);
    }

    [Fact] public void T11_BR6_Subtotal300000_WaivesDAndR_NotRain()
    {
        var r = _pricing.CalculatePrice(new[] { Line(300_000m) }, new() { DistanceKm = 3.0m, HubLoadPct = 50m, HeavyRainFlag = true, PlacementUtc7 = new(2026, 11, 21, 11, 30, 0) });
        Assert.Equal(0m, r.DeliveryFeeD);
        Assert.Equal(0m, r.RushSurchargeR);
        Assert.Equal(10_000m, r.RainSurchargeR2);
        Assert.Equal(310_000m, r.RoundedTotal);
    }

    [Fact] public void T12_BR7_Load29Pct_QuietKitchen10Pct()
    {
        var r = _pricing.CalculatePrice(new[] { Line(200_000m) }, new() { DistanceKm = 2.0m, HubLoadPct = 29m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) });
        Assert.Equal(20_000m, r.QuietKitchenQ);
    }

    [Fact] public void T13_BR7_Load30Pct_NoQuietKitchen()
    {
        var r = _pricing.CalculatePrice(new[] { Line(200_000m) }, new() { DistanceKm = 2.0m, HubLoadPct = 30m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) });
        Assert.Equal(0m, r.QuietKitchenQ);
    }

    [Fact] public void T14_BR8_Load89Pct_Accepted()
    {
        var r = _pricing.CalculatePrice(new[] { Line(100_000m) }, new() { DistanceKm = 2.0m, HubLoadPct = 89m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) });
        Assert.Equal(115_000m, r.RoundedTotal);
    }

    [Fact] public void T15_BR8_Load90Pct_Rejected() =>
        Assert.Throws<InvalidOperationException>(() => _pricing.CalculatePrice(new[] { Line(100_000m) }, new() { DistanceKm = 2.0m, HubLoadPct = 90m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }));

    [Fact] public void T16_BR9_PercentVoucherWithCap()
    {
        var v = new Voucher { VoucherCode = "SAVE20PCT", DiscountType = VoucherType.Percent, DiscountValue = 20m, MaxCap = 30_000m, MinSubtotal = 100_000m };
        var r = _pricing.CalculatePrice(new[] { Line(200_000m) }, new() { DistanceKm = 2.0m, HubLoadPct = 25m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }, v);
        Assert.Equal(30_000m, r.VoucherV);
        Assert.Equal(150_000m, r.FoodTotalF);
    }

    [Fact] public void T17_BR10_BR11_WorkedExample205k_AndHalfUpRounding()
    {
        var r = _pricing.CalculatePrice(new[] { Line(200_000m) }, new() { DistanceKm = 3.5m, HubLoadPct = 25m, HeavyRainFlag = false, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) });
        Assert.Equal(205_000m, r.RoundedTotal);
    }

    [Fact] public void T18_BR12_QtyAndLineCountBoundaries()
    {
        Assert.Throws<ArgumentException>(() => _pricing.CalculatePrice(new[] { Line(50_000m, 0) }, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }));
        Assert.Throws<ArgumentException>(() => _pricing.CalculatePrice(new[] { Line(50_000m, 21) }, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }));
        var lines31 = Enumerable.Range(1, 31).Select(_ => Line(10_000m, 1)).ToList();
        Assert.Throws<ArgumentException>(() => _pricing.CalculatePrice(lines31, new() { DistanceKm = 2m, HubLoadPct = 50m, PlacementUtc7 = new(2026, 11, 21, 14, 0, 0) }));
    }

    [Fact] public void T19_T20_BR14_CodLimitBoundary()
    {
        var cod = new CodPaymentStrategy();
        Assert.True(cod.ProcessPayment(1_000_000m).Success);
        Assert.Throws<InvalidOperationException>(() => cod.ProcessPayment(1_001_000m));
    }

    [Fact] public void T32_NFR1_PerformanceUnder1500msFor30Lines()
    {
        var lines30 = Enumerable.Range(1, 30).Select(_ => Line(15_000m, 2)).ToList();
        var sw = Stopwatch.StartNew();
        _pricing.CalculatePrice(lines30, new() { DistanceKm = 4.0m, HubLoadPct = 25m, HeavyRainFlag = true, PlacementUtc7 = new(2026, 11, 21, 12, 0, 0) });
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 1500);
    }
}
