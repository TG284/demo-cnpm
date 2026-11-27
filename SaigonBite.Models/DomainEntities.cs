namespace SaigonBite.Models;

public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? HubID { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Hub
{
    public int HubID { get; set; }
    public string HubName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public decimal CurrentLoadPct { get; set; }
    public bool HeavyRainFlag { get; set; }
    public bool IsActive { get; set; } = true;
}

public class MenuItem
{
    public int ItemID { get; set; }
    public int HubID { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;
}

public class Voucher
{
    public string VoucherCode { get; set; } = string.Empty;
    public VoucherType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxCap { get; set; }
    public decimal MinSubtotal { get; set; }
    public bool IsActive { get; set; } = true;
}

public class OrderLine
{
    public int ItemID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BrandCategory { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class OrderPriceSnapshot
{
    public DateTime PlacementUtc7 { get; set; }
    public decimal HubLoadPct { get; set; }
    public bool HeavyRainFlag { get; set; }
    public decimal DistanceKm { get; set; }
}

public class PriceBreakdown
{
    public decimal SubtotalS { get; set; }
    public decimal QuietKitchenQ { get; set; }
    public decimal VoucherV { get; set; }
    public decimal FoodTotalF { get; set; }
    public decimal DeliveryFeeD { get; set; }
    public decimal RushSurchargeR { get; set; }
    public decimal RainSurchargeR2 { get; set; }
    public decimal RawTotal { get; set; }
    public decimal RoundedTotal { get; set; }
}

public class OrderStatusHistoryEntry
{
    public OrderStatus? FromState { get; set; }
    public OrderStatus ToState { get; set; }
    public int ActorID { get; set; }
    public UserRole ActorRole { get; set; }
    public DateTime ChangedAtUtc7 { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class Order
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public UserRole CustomerRole { get; set; }
    public int HubID { get; set; }
    public int? ShipperID { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? VoucherCode { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = "Pending";
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public List<OrderLine> Lines { get; set; } = new();
    public OrderPriceSnapshot Snapshot { get; set; } = new();
    public PriceBreakdown Pricing { get; set; } = new();
    public List<OrderStatusHistoryEntry> History { get; set; } = new();
}

public class RevenueReportRow
{
    public string BrandCategory { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
}

public class HubUtilizationRow
{
    public string HubName { get; set; } = string.Empty;
    public int OrdersHandled { get; set; }
    public double AvgPrepMinutes { get; set; }
    public decimal CurrentLoadPct { get; set; }
    public int QuietKitchenCount { get; set; }
}
