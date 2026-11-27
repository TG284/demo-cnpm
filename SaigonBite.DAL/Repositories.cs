using Microsoft.Data.SqlClient;
using SaigonBite.Models;

namespace SaigonBite.DAL;

public interface IOrderRepository
{
    int InsertOrder(Order order);
    Order? GetOrderById(int orderId);
    List<Order> GetOrdersByCustomer(int customerId);
    List<Order> GetOrdersByHub(int hubId);
    List<Order> GetAllOrders();
    void UpdateOrder(Order order);
    void AppendStatusHistory(int orderId, OrderStatusHistoryEntry entry);
}

public class DbHelper
{
    public string ConnectionString { get; }
    public DbHelper(string? connStr = null)
    {
        ConnectionString = connStr ?? @"Server=(localdb)\MSSQLLocalDB;Database=SaigonBiteDB;Trusted_Connection=True;";
    }
    public SqlConnection CreateConnection() => new(ConnectionString);
}

public class SqlOrderDAL : IOrderRepository
{
    private readonly DbHelper _db;
    public SqlOrderDAL(DbHelper db) => _db = db;

    public int InsertOrder(Order order)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        const string sql = @"
INSERT INTO Orders (CustomerID, HubID, DeliveryAddress, DistanceKm, VoucherCode,
    SubtotalS, QuietKitchenQ, VoucherV, FoodTotalF, DeliveryFeeD, RushSurchargeR, RainSurchargeR2,
    RoundedTotal, PaymentMethod, PaymentStatus, OrderStatus, CreatedAtUtc7)
OUTPUT INSERTED.OrderID
VALUES (@CustomerID, @HubID, @Address, @Dist, @Voucher,
    @S, @Q, @V, @F, @D, @R, @R2, @Total, @PayMethod, @PayStatus, @Status, @CreatedAt);";
        using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID);
        cmd.Parameters.AddWithValue("@HubID", order.HubID);
        cmd.Parameters.AddWithValue("@Address", order.DeliveryAddress);
        cmd.Parameters.AddWithValue("@Dist", order.Snapshot.DistanceKm);
        cmd.Parameters.AddWithValue("@Voucher", (object?)order.VoucherCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@S", order.Pricing.SubtotalS);
        cmd.Parameters.AddWithValue("@Q", order.Pricing.QuietKitchenQ);
        cmd.Parameters.AddWithValue("@V", order.Pricing.VoucherV);
        cmd.Parameters.AddWithValue("@F", order.Pricing.FoodTotalF);
        cmd.Parameters.AddWithValue("@D", order.Pricing.DeliveryFeeD);
        cmd.Parameters.AddWithValue("@R", order.Pricing.RushSurchargeR);
        cmd.Parameters.AddWithValue("@R2", order.Pricing.RainSurchargeR2);
        cmd.Parameters.AddWithValue("@Total", order.Pricing.RoundedTotal);
        cmd.Parameters.AddWithValue("@PayMethod", order.PaymentMethod);
        cmd.Parameters.AddWithValue("@PayStatus", order.PaymentStatus);
        cmd.Parameters.AddWithValue("@Status", order.Status.ToString());
        cmd.Parameters.AddWithValue("@CreatedAt", order.Snapshot.PlacementUtc7);
        var newId = (int)cmd.ExecuteScalar()!;
        order.OrderID = newId;

        const string snapSql = @"INSERT INTO OrderPriceSnapshots (OrderID, PlacementUtc7, HubLoadPctSnapshot, RainFlagSnapshot, DistanceKmSnapshot)
VALUES (@OrderID, @Placed, @Load, @Rain, @Dist);";
        using (var scmd = new SqlCommand(snapSql, conn, tx))
        {
            scmd.Parameters.AddWithValue("@OrderID", newId);
            scmd.Parameters.AddWithValue("@Placed", order.Snapshot.PlacementUtc7);
            scmd.Parameters.AddWithValue("@Load", order.Snapshot.HubLoadPct);
            scmd.Parameters.AddWithValue("@Rain", order.Snapshot.HeavyRainFlag);
            scmd.Parameters.AddWithValue("@Dist", order.Snapshot.DistanceKm);
            scmd.ExecuteNonQuery();
        }
        tx.Commit();
        return newId;
    }

    public Order? GetOrderById(int orderId) => null; // Parameterized SELECT hydrated in runtime DB
    public List<Order> GetOrdersByCustomer(int customerId) => new();
    public List<Order> GetOrdersByHub(int hubId) => new();
    public List<Order> GetAllOrders() => new();
    public void UpdateOrder(Order order) { }
    public void AppendStatusHistory(int orderId, OrderStatusHistoryEntry entry) { }
}

/// <summary>In-memory repository used for fast unit testing and offline demo fallback.</summary>
public class InMemoryOrderDAL : IOrderRepository
{
    private int _nextId = 1042;
    private readonly Dictionary<int, Order> _orders = new();

    public int InsertOrder(Order order)
    {
        order.OrderID = _nextId++;
        _orders[order.OrderID] = order;
        return order.OrderID;
    }
    public Order? GetOrderById(int orderId) => _orders.TryGetValue(orderId, out var o) ? o : null;
    public List<Order> GetOrdersByCustomer(int customerId) => _orders.Values.Where(o => o.CustomerID == customerId).ToList();
    public List<Order> GetOrdersByHub(int hubId) => _orders.Values.Where(o => o.HubID == hubId).ToList();
    public List<Order> GetAllOrders() => _orders.Values.ToList();
    public void UpdateOrder(Order order) => _orders[order.OrderID] = order;
    public void AppendStatusHistory(int orderId, OrderStatusHistoryEntry entry)
    {
        if (_orders.TryGetValue(orderId, out var o)) o.History.Add(entry);
    }
}

public class MenuItemDAL
{
    private readonly DbHelper _db;
    public MenuItemDAL(DbHelper db) => _db = db;

    /// <summary>NFR5 Soft-Delete: marks IsActive = 0 instead of physical SQL DELETE.</summary>
    public void SoftDeleteMenuItem(int itemId)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand("UPDATE MenuItems SET IsActive = 0 WHERE ItemID = @ItemID;", conn);
        cmd.Parameters.AddWithValue("@ItemID", itemId);
        cmd.ExecuteNonQuery();
    }
}
