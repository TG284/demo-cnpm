using SaigonBite.BLL;
using SaigonBite.DAL;
using SaigonBite.Models;

namespace SaigonBite.UI;

/// <summary>Seeds exact hubs, menu items, vouchers, users, and sample orders matching Team 9's Figma prototype.</summary>
public class DemoAppContext
{
    public IOrderRepository OrderRepo { get; }
    public PricingService Pricing { get; }
    public SaigonBite.BLL.Payments.PaymentFactory PaymentFactory { get; }
    public OrderService OrderService { get; }
    public AuthService AuthService { get; }
    public ReportService ReportService { get; }

    public List<User> Users { get; } = new();
    public List<Hub> Hubs { get; } = new();
    public List<MenuItem> MenuItems { get; } = new();
    public List<Voucher> Vouchers { get; } = new();
    public User? CurrentUser { get; set; }

    public DemoAppContext()
    {
        OrderRepo = new InMemoryOrderDAL();
        Pricing = new PricingService();
        PaymentFactory = new SaigonBite.BLL.Payments.PaymentFactory();
        OrderService = new OrderService(OrderRepo, Pricing, PaymentFactory);
        AuthService = new AuthService();
        ReportService = new ReportService(OrderRepo);

        SeedData();
    }

    private void SeedData()
    {
        var pwHash = AuthService.HashPassword("SaigonBite2026");

        Users.AddRange(new[]
        {
            new User { UserID = 1, Username = "0901234567", FullName = "Nguyen Van A", PasswordHash = pwHash, Role = UserRole.LocalCustomer },
            new User { UserID = 2, Username = "C1234567", FullName = "John Smith", PasswordHash = pwHash, Role = UserRole.ForeignTourist },
            new User { UserID = 3, Username = "kitchen.thuduc", FullName = "Kitchen Staff · Thu Duc", PasswordHash = pwHash, Role = UserRole.KitchenStaff, HubID = 3 },
            new User { UserID = 4, Username = "shipper.thuduc", FullName = "Shipper · Thu Duc", PasswordHash = pwHash, Role = UserRole.Shipper, HubID = 3 },
            new User { UserID = 5, Username = "admin", FullName = "System Admin", PasswordHash = pwHash, Role = UserRole.Admin }
        });

        Hubs.AddRange(new[]
        {
            new Hub { HubID = 1, HubName = "District 1", District = "District 1", CurrentLoadPct = 45m, HeavyRainFlag = true },
            new Hub { HubID = 2, HubName = "Binh Thanh", District = "Binh Thanh", CurrentLoadPct = 92m, HeavyRainFlag = false },
            new Hub { HubID = 3, HubName = "Thu Duc", District = "Thu Duc", CurrentLoadPct = 22m, HeavyRainFlag = true },
            new Hub { HubID = 4, HubName = "District 7", District = "District 7", CurrentLoadPct = 38m, HeavyRainFlag = false },
            new Hub { HubID = 5, HubName = "Go Vap", District = "Go Vap", CurrentLoadPct = 26m, HeavyRainFlag = false }
        });

        // Exact menu items from Figma (Thu Duc has all 5; District 1 has 3; Binh Thanh has 3)
        MenuItems.AddRange(new[]
        {
            // Hub 3: Thu Duc (5 unique dishes)
            new MenuItem { ItemID = 1,  HubID = 3, ItemName = "Phở tái",           BrandName = "", Category = "Noodle Soups", UnitPrice = 65_000m },
            new MenuItem { ItemID = 2,  HubID = 3, ItemName = "Bún Chả Hà Nội",       BrandName = "", Category = "Noodle Soups", UnitPrice = 55_000m },
            new MenuItem { ItemID = 3,  HubID = 3, ItemName = "Cơm sườn",         BrandName = "", Category = "Rice",         UnitPrice = 60_000m },
            new MenuItem { ItemID = 4,  HubID = 3, ItemName = "Bánh Mì Thịt",         BrandName = "", Category = "Bread",        UnitPrice = 30_000m },
            new MenuItem { ItemID = 5,  HubID = 3, ItemName = "Trà Đào Cam Sả",       BrandName = "", Category = "Drinks",       UnitPrice = 35_000m },
            // Hub 1: District 1 (4 unique dishes)
            new MenuItem { ItemID = 6,  HubID = 1, ItemName = "Bun Bo Hue",           BrandName = "", Category = "Noodle Soups", UnitPrice = 68_000m },
            new MenuItem { ItemID = 7,  HubID = 1, ItemName = "Com Ga Xoi Mo",        BrandName = "", Category = "Rice",         UnitPrice = 62_000m },
            new MenuItem { ItemID = 8,  HubID = 1, ItemName = "Banh Mi Chao",         BrandName = "", Category = "Bread",        UnitPrice = 55_000m },
            new MenuItem { ItemID = 9,  HubID = 1, ItemName = "Ca Phe Sua Da",        BrandName = "", Category = "Drinks",       UnitPrice = 32_000m },
            // Hub 4: District 7 (4 unique dishes)
            new MenuItem { ItemID = 10, HubID = 4, ItemName = "Hu Tieu Nam Vang",     BrandName = "", Category = "Noodle Soups", UnitPrice = 65_000m },
            new MenuItem { ItemID = 11, HubID = 4, ItemName = "Com Chien Duong Chau", BrandName = "", Category = "Rice",         UnitPrice = 58_000m },
            new MenuItem { ItemID = 12, HubID = 4, ItemName = "Banh Mi Bo Kho",       BrandName = "", Category = "Bread",        UnitPrice = 52_000m },
            new MenuItem { ItemID = 13, HubID = 4, ItemName = "Tra Sua Tran Chau",    BrandName = "", Category = "Drinks",       UnitPrice = 40_000m },
            // Hub 5: Go Vap (4 unique dishes)
            new MenuItem { ItemID = 14, HubID = 5, ItemName = "Mi Quang Tom Thit",    BrandName = "", Category = "Noodle Soups", UnitPrice = 60_000m },
            new MenuItem { ItemID = 15, HubID = 5, ItemName = "Com Suon Bi Cha",      BrandName = "", Category = "Rice",         UnitPrice = 65_000m },
            new MenuItem { ItemID = 16, HubID = 5, ItemName = "Banh Mi Ga Xe",        BrandName = "", Category = "Bread",        UnitPrice = 35_000m },
            new MenuItem { ItemID = 17, HubID = 5, ItemName = "Nuoc Mia Tac",         BrandName = "", Category = "Drinks",       UnitPrice = 25_000m }
        });

        Vouchers.AddRange(new[]
        {
            new Voucher { VoucherCode = "WELCOME15K", DiscountType = VoucherType.Fixed, DiscountValue = 15_000m, MinSubtotal = 100_000m },
            new Voucher { VoucherCode = "SAIGON20", DiscountType = VoucherType.Percent, DiscountValue = 20m, MaxCap = 30_000m, MinSubtotal = 100_000m }
        });

        // Start clean with zero pre-seeded orders (orders are created live by users)
    }

    public string GetHubName(int hubId) => LocalizationHelper.HubName(Hubs.FirstOrDefault(h => h.HubID == hubId)?.HubName ?? $"Hub #{hubId}");
}
