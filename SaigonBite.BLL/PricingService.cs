using SaigonBite.Models;

namespace SaigonBite.BLL;

/// <summary>
/// Isolated Business Logic Layer module owning all SaigonBite pricing rules (BR1-BR12).
/// Zero UI or SQL dependencies so calculations execute in < 1.5s (NFR1) and are unit-testable.
/// </summary>
public class PricingService
{
    public const decimal MinSubtotalVnd = 50_000m;
    public const decimal MaxDistanceKm = 8.0m;
    public const decimal FreeDeliveryThresholdVnd = 300_000m;
    public const decimal QuietKitchenLoadThreshold = 30.0m;
    public const decimal HubOverloadThreshold = 90.0m;
    public const decimal RainSurchargeVnd = 10_000m;

    public PriceBreakdown CalculatePrice(
        IReadOnlyList<OrderLine> lines,
        OrderPriceSnapshot snapshot,
        Voucher? voucher = null)
    {
        // BR12: 1-30 lines, quantity 1-20 per line
        if (lines == null || lines.Count < 1 || lines.Count > 30)
            throw new ArgumentException("Order must contain between 1 and 30 line items (BR12).");
        if (lines.Any(l => l.Quantity < 1 || l.Quantity > 20))
            throw new ArgumentException("Each order line quantity must be between 1 and 20 (BR12).");

        // BR8: Hub load protection >= 90%
        if (snapshot.HubLoadPct >= HubOverloadThreshold)
            throw new InvalidOperationException("Selected hub is at or above 90% load. Please choose another hub (BR8).");

        // BR2: Coverage distance <= 8.0 km
        if (snapshot.DistanceKm <= 0 || snapshot.DistanceKm > MaxDistanceKm)
            throw new ArgumentException("Delivery distance must be > 0 and <= 8.0 km (BR2).");

        // BR10 Step 1: Subtotal S
        decimal subtotalS = lines.Sum(l => l.UnitPrice * l.Quantity);

        // BR1: Minimum order subtotal >= 50,000 VND
        if (subtotalS < MinSubtotalVnd)
            throw new ArgumentException("Order subtotal must be at least 50,000 VND (BR1).");

        // BR7: Quiet Kitchen discount Q (10% of S when hub load < 30%)
        decimal quietKitchenQ = snapshot.HubLoadPct < QuietKitchenLoadThreshold
            ? subtotalS * 0.10m
            : 0m;

        // BR9: Single voucher V (min subtotal checked on S; PERCENT applied to S - Q with cap; FIXED <= S - Q)
        decimal voucherV = 0m;
        if (voucher != null && voucher.IsActive && subtotalS >= voucher.MinSubtotal)
        {
            decimal afterQuiet = subtotalS - quietKitchenQ;
            if (voucher.DiscountType == VoucherType.Percent)
            {
                decimal rawV = afterQuiet * (voucher.DiscountValue / 100m);
                voucherV = voucher.MaxCap.HasValue ? Math.Min(rawV, voucher.MaxCap.Value) : rawV;
            }
            else
            {
                voucherV = Math.Min(voucher.DiscountValue, afterQuiet);
            }
        }

        // BR10: Food total F = S - Q - V
        decimal foodTotalF = subtotalS - quietKitchenQ - voucherV;

        // BR3: Delivery fee D by distance band
        decimal deliveryD = snapshot.DistanceKm <= 2.0m ? 15_000m
                          : snapshot.DistanceKm <= 5.0m ? 25_000m
                          : 35_000m;

        // BR4: Rush-hour surcharge R = 20% of D in [11:00, 13:00) or [17:30, 19:30) UTC+7
        TimeSpan t = snapshot.PlacementUtc7.TimeOfDay;
        bool isRush = (t >= new TimeSpan(11, 0, 0) && t < new TimeSpan(13, 0, 0))
                   || (t >= new TimeSpan(17, 30, 0) && t < new TimeSpan(19, 30, 0));
        decimal rushR = isRush ? deliveryD * 0.20m : 0m;

        // BR6: Free delivery waiver if S >= 300,000 VND (waives D and R, NOT R2)
        if (subtotalS >= FreeDeliveryThresholdVnd)
        {
            deliveryD = 0m;
            rushR = 0m;
        }

        // BR5: Heavy rain surcharge R2 = 10,000 VND (never waived)
        decimal rainR2 = snapshot.HeavyRainFlag ? RainSurchargeVnd : 0m;

        // BR10 & BR11: Raw total rounded ONCE at the end to nearest 1,000 VND (half up)
        decimal rawTotal = foodTotalF + deliveryD + rushR + rainR2;
        decimal roundedTotal = Math.Floor((rawTotal + 500m) / 1_000m) * 1_000m;

        return new PriceBreakdown
        {
            SubtotalS = subtotalS,
            QuietKitchenQ = quietKitchenQ,
            VoucherV = voucherV,
            FoodTotalF = foodTotalF,
            DeliveryFeeD = deliveryD,
            RushSurchargeR = rushR,
            RainSurchargeR2 = rainR2,
            RawTotal = rawTotal,
            RoundedTotal = roundedTotal
        };
    }
}
