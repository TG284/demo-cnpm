using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SaigonBite.DAL;
using SaigonBite.Models;

namespace SaigonBite.BLL;

public class AuthService
{
    private static readonly Regex VnPhoneRx = new(@"^(03|05|07|08|09)\d{8}$", RegexOptions.Compiled);
    private static readonly Regex PassportRx = new(@"^[A-Za-z0-9]{6,9}$", RegexOptions.Compiled);

    public string HashPassword(string plain)
    {
        if (string.IsNullOrEmpty(plain) || plain.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters (BR18).");
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool ValidatePassword(string password) => !string.IsNullOrEmpty(password) && password.Length >= 8;
    public bool ValidateLocalPhone(string phone) => VnPhoneRx.IsMatch(phone ?? string.Empty);
    public bool ValidateVietnamesePhone(string phone) => ValidateLocalPhone(phone);
    public bool ValidatePassport(string passport) => PassportRx.IsMatch(passport ?? string.Empty);
}

public class ReportService
{
    private readonly IOrderRepository _repo;
    public ReportService(IOrderRepository repo) => _repo = repo;

    /// <summary>FR9 / Report R1: Revenue by brand category and payment method, excluding Cancelled & Failed.</summary>
    public List<RevenueReportRow> GenerateReportR1(DateTime fromUtc7, DateTime toUtc7)
    {
        var validOrders = _repo.GetAllOrders()
            .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Failed)
            .Where(o => o.Snapshot.PlacementUtc7 >= fromUtc7 && o.Snapshot.PlacementUtc7 <= toUtc7);

        return validOrders
            .SelectMany(o => o.Lines.Select(l => new { l.BrandCategory, o.PaymentMethod, Amount = l.LineTotal }))
            .GroupBy(x => (x.BrandCategory, x.PaymentMethod))
            .Select(g => new RevenueReportRow
            {
                BrandCategory = g.Key.BrandCategory,
                PaymentMethod = g.Key.PaymentMethod,
                TotalRevenue = g.Sum(x => x.Amount)
            })
            .OrderByDescending(r => r.TotalRevenue)
            .ToList();
    }

    /// <summary>FR10 / Report R2: Hub Utilization (orders handled, avg prep mins, current load %, Quiet Kitchen count).</summary>
    public List<HubUtilizationRow> GenerateReportR2(IEnumerable<Hub> hubs)
    {
        var all = _repo.GetAllOrders();
        return hubs.Where(h => h.IsActive).Select(h =>
        {
            var hubOrders = all.Where(o => o.HubID == h.HubID).ToList();
            var prepDurations = hubOrders.Select(o =>
            {
                var conf = o.History.FirstOrDefault(e => e.ToState == OrderStatus.Confirmed);
                var rdy = o.History.FirstOrDefault(e => e.ToState == OrderStatus.Ready);
                return (conf != null && rdy != null) ? (rdy.ChangedAtUtc7 - conf.ChangedAtUtc7).TotalMinutes : (double?)null;
            }).Where(m => m.HasValue).Select(m => m!.Value).ToList();

            return new HubUtilizationRow
            {
                HubName = h.HubName,
                OrdersHandled = hubOrders.Count,
                AvgPrepMinutes = prepDurations.Count > 0 ? Math.Round(prepDurations.Average(), 1) : 0.0,
                CurrentLoadPct = h.CurrentLoadPct,
                QuietKitchenCount = hubOrders.Count(o => o.Pricing.QuietKitchenQ > 0)
            };
        }).ToList();
    }
}
