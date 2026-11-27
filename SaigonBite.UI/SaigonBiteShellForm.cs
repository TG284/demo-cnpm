using System.Drawing;
using System.Windows.Forms;
using SaigonBite.Models;
using SaigonBite.UI.Views;

namespace SaigonBite.UI;

/// <summary>Top-level WinForms shell with SaigonBite brand emblem, EN/VI toggle, role navigation, and Quick Demo Switch bar.</summary>
public class SaigonBiteShellForm : Form
{
    private readonly DemoAppContext _ctx;
    private readonly Panel _topBar;
    private readonly Panel _demoBar;
    private readonly FlowLayoutPanel _navLinks;
    private readonly Button _btnEn;
    private readonly Button _btnVi;
    private readonly Label _lblUserBadge;
    private readonly Button _btnSignOut;
    private readonly Panel _contentHost;

    private string _currentScreen = "auth";
    private Hub? _pendingHub;
    private List<OrderLine>? _pendingLines;
    private OrderPriceSnapshot? _pendingSnapshot;
    private Voucher? _pendingVoucher;
    private PriceBreakdown? _pendingBreakdown;
    private Order? _selectedTrackOrder;

    public SaigonBiteShellForm(DemoAppContext ctx)
    {
        _ctx = ctx;
        Text = "SaigonBite";
        Size = new Size(1320, 800);
        MinimumSize = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = SaigonBiteTheme.BgCanvas;

        // Top Navigation Bar
        _topBar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White };
        _topBar.Paint += (_, e) =>
        {
            using var pen = new Pen(SaigonBiteTheme.BorderSubtle, 1);
            e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
        };

        var lblMark = new PictureBox
        {
            Image = AssetLoader.GetLogoImage(),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Location = new Point(20, 11),
            Size = new Size(44, 36)
        };

        var lblBrand = new Label
        {
            Text = "SaigonBite",
            Font = SaigonBiteTheme.FontBrand,
            ForeColor = SaigonBiteTheme.AccentOrange,
            Location = new Point(70, 15),
            AutoSize = true
        };

        _navLinks = new FlowLayoutPanel
        {
            Location = new Point(200, 15),
            Size = new Size(500, 32),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        var rightBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 580,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 12, 24, 0),
            WrapContents = false,
            BackColor = Color.Transparent
        };

        _lblUserBadge = new RoundedPillLabel
        {
            Text = "  Khách  ",
            Font = SaigonBiteTheme.FontSmallBold,
            PillBackColor = SaigonBiteTheme.BgHeaderRow,
            ForeColor = SaigonBiteTheme.TextPrimary,
            AutoSize = true,
            Padding = new Padding(8, 5, 8, 5),
            Margin = new Padding(10, 2, 0, 0)
        };

        _btnSignOut = new RoundedButton
        {
            Text = "Đăng xuất",
            Width = 84,
            Height = 28,
            BackColor = Color.White,
            ForeColor = SaigonBiteTheme.TextPrimary,
            Font = SaigonBiteTheme.FontSmallBold,
            CornerRadius = 8,
            BorderSize = 1,
            BorderColor = SaigonBiteTheme.BorderSubtle,
            Cursor = Cursors.Hand,
            Margin = new Padding(8, 2, 0, 0),
            Visible = false
        };
        _btnSignOut.Click += (_, _) => SignOut();

        _btnVi = SaigonBiteTheme.CreatePillButton("VI", true, 40, 26);
        _btnVi.Margin = new Padding(4, 3, 0, 0);
        _btnVi.Click += (_, _) => SetLang(true);

        _btnEn = SaigonBiteTheme.CreatePillButton("EN", false, 40, 26);
        _btnEn.Margin = new Padding(4, 3, 0, 0);
        _btnEn.Click += (_, _) => SetLang(false);

        rightBar.Controls.AddRange(new Control[] { _lblUserBadge, _btnSignOut, _btnVi, _btnEn });
        _topBar.Controls.AddRange(new Control[] { lblMark, lblBrand, _navLinks, rightBar });

        // Bottom Presenter Quick Demo Switch Bar
        _demoBar = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.White };
        _demoBar.Paint += (_, e) =>
        {
            using var pen = new Pen(SaigonBiteTheme.BorderSubtle, 1);
            e.Graphics.DrawLine(pen, 0, 0, _demoBar.Width, 0);
        };
        BuildDemoBar();

        _contentHost = new Panel { Dock = DockStyle.Fill, BackColor = SaigonBiteTheme.BgCanvas };
        _demoBar.Visible = false;
        Controls.Add(_contentHost);
        Controls.Add(_topBar);

        // Start on the Sign In screen so users sign in or register an account first
        _ctx.CurrentUser = null;
        ShowAuth();
    }

    private void BuildDemoBar()
    {
        _demoBar.Controls.Clear();
        var lbl = new Label
        {
            Text = LocalizationHelper.T("Quick demo switch:", "Chuyển nhanh demo:"),
            Font = SaigonBiteTheme.FontSmallBold,
            ForeColor = SaigonBiteTheme.TextMuted,
            Location = new Point(22, 13),
            AutoSize = true
        };
        _demoBar.Controls.Add(lbl);

        var flow = new FlowLayoutPanel
        {
            Location = new Point(165, 9),
            Size = new Size(1100, 32),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        void AddQuickBtn(string text, Action onClick, bool active)
        {
            var b = SaigonBiteTheme.CreatePillButton(text, active, 128, 26);
            b.Margin = new Padding(0, 0, 8, 0);
            b.Click += (_, _) => onClick();
            flow.Controls.Add(b);
        }

        AddQuickBtn(LocalizationHelper.T("Sign in", "Đăng nhập"), SignOut, _ctx.CurrentUser == null);
        AddQuickBtn(LocalizationHelper.T("Local Customer", "Khách địa phương"), () => QuickSwitch(UserRole.LocalCustomer), _ctx.CurrentUser?.Role == UserRole.LocalCustomer);
        AddQuickBtn(LocalizationHelper.T("Foreign Tourist", "Khách nước ngoài"), () => QuickSwitch(UserRole.ForeignTourist), _ctx.CurrentUser?.Role == UserRole.ForeignTourist);
        AddQuickBtn(LocalizationHelper.T("Kitchen Staff", "Nhân viên bếp"), () => QuickSwitch(UserRole.KitchenStaff), _ctx.CurrentUser?.Role == UserRole.KitchenStaff);
        AddQuickBtn(LocalizationHelper.T("Shipper", "Shipper"), () => QuickSwitch(UserRole.Shipper), _ctx.CurrentUser?.Role == UserRole.Shipper);
        AddQuickBtn(LocalizationHelper.T("System Admin", "Quản trị viên"), () => QuickSwitch(UserRole.Admin), _ctx.CurrentUser?.Role == UserRole.Admin);

        _demoBar.Controls.Add(flow);
    }

    private void QuickSwitch(UserRole role)
    {
        var user = _ctx.Users.FirstOrDefault(u => u.Role == role);
        if (user != null) OnSignedIn(user);
    }

    private void SetLang(bool vi)
    {
        LocalizationHelper.IsVietnamese = vi;
        _btnEn.BackColor = !vi ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BgPillSoft;
        _btnEn.ForeColor = !vi ? Color.White : SaigonBiteTheme.AccentOrange;
        _btnVi.BackColor = vi ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BgPillSoft;
        _btnVi.ForeColor = vi ? Color.White : SaigonBiteTheme.AccentOrange;
        Text = "SaigonBite";
        _btnSignOut.Text = LocalizationHelper.T("Sign out", "Đăng xuất");
        UpdateUserBadge();
        BuildDemoBar();
        RefreshCurrentView();
    }

    private void UpdateUserBadge()
    {
        var user = _ctx.CurrentUser;
        if (user == null)
        {
            _lblUserBadge.Text = $"  {LocalizationHelper.T("Guest", "Khách")}  ";
            return;
        }
        string hubThuDuc = LocalizationHelper.HubName("Thu Duc");
        _lblUserBadge.Text = user.Role switch
        {
            UserRole.LocalCustomer  => $"  {LocalizationHelper.T("Local Customer", "Khách địa phương")} · {user.Username}  ",
            UserRole.ForeignTourist => $"  {LocalizationHelper.T("Foreign Tourist", "Khách nước ngoài")} · {user.Username}  ",
            UserRole.KitchenStaff   => $"  {LocalizationHelper.T("Kitchen Staff", "Nhân viên bếp")} · {hubThuDuc}  ",
            UserRole.Shipper        => $"  Shipper · {hubThuDuc}  ",
            _                       => $"  {LocalizationHelper.T("System Admin", "Quản trị viên")}  "
        };
    }

    private void SignOut()
    {
        _ctx.CurrentUser = null;
        UpdateUserBadge();
        ShowAuth();
    }

    private void OnSignedIn(User user)
    {
        _ctx.CurrentUser = user;
        _btnSignOut.Visible = true;
        UpdateUserBadge();

        BuildDemoBar();

        switch (user.Role)
        {
            case UserRole.LocalCustomer:
            case UserRole.ForeignTourist:
                ShowNewOrder();
                break;
            case UserRole.KitchenStaff:
                ShowKitchen();
                break;
            case UserRole.Shipper:
                ShowShipper();
                break;
            case UserRole.Admin:
                ShowAdmin("orders");
                break;
        }
    }

    private void SetNav(params (string Label, bool Active, Action OnClick)[] items)
    {
        _navLinks.Controls.Clear();
        foreach (var (label, active, onClick) in items)
        {
            var lnk = new LinkLabel
            {
                Text = label,
                Font = active ? SaigonBiteTheme.FontBodyBold : SaigonBiteTheme.FontBody,
                LinkColor = active ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.TextMuted,
                ActiveLinkColor = SaigonBiteTheme.AccentOrange,
                LinkBehavior = LinkBehavior.NeverUnderline,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 6),
                Margin = new Padding(0, 4, 22, 0)
            };
            if (active)
            {
                lnk.Paint += (s, pe) =>
                {
                    if (s is Control c)
                    {
                        using var pen = new Pen(SaigonBiteTheme.AccentOrange, 2);
                        pe.Graphics.DrawLine(pen, 0, c.Height - 2, c.Width, c.Height - 2);
                    }
                };
            }
            lnk.Click += (_, _) => onClick();
            _navLinks.Controls.Add(lnk);
        }
    }

    private void Mount(UserControl view)
    {
        _contentHost.Controls.Clear();
        view.Dock = DockStyle.Fill;
        _contentHost.Controls.Add(view);
    }

    private void ShowAuth()
    {
        _currentScreen = "auth";
        _btnSignOut.Visible = false;
        _lblUserBadge.Text = $"  {LocalizationHelper.T("Guest", "Khách")}  ";
        _navLinks.Controls.Clear();
        BuildDemoBar();
        Mount(new AuthView(_ctx, OnSignedIn));
    }

    private void ShowNewOrder()
    {
        _currentScreen = "newOrder";
        SetNav(
            (LocalizationHelper.T("New Order", "Đặt hàng"), true, ShowNewOrder),
            (LocalizationHelper.T("My Orders", "Đơn của tôi"), false, () => ShowTracking(null))
        );
        Mount(new NewOrderView(_ctx, (hub, lines, snap, voucher, breakdown) =>
        {
            _pendingHub = hub;
            _pendingLines = lines;
            _pendingSnapshot = snap;
            _pendingVoucher = voucher;
            _pendingBreakdown = breakdown;
            ShowPayment();
        }));
    }

    private void ShowPayment()
    {
        if (_pendingHub == null || _pendingLines == null || _pendingSnapshot == null || _pendingBreakdown == null)
        {
            ShowNewOrder();
            return;
        }
        _currentScreen = "payment";
        SetNav(
            (LocalizationHelper.T("New Order", "Đặt hàng"), true, ShowNewOrder),
            (LocalizationHelper.T("My Orders", "Đơn của tôi"), false, () => ShowTracking(null))
        );
        Mount(new PaymentView(
            _ctx, _pendingHub, _pendingLines, _pendingSnapshot, _pendingVoucher, _pendingBreakdown,
            onBack: ShowNewOrder,
            onTrackOrder: ord => ShowTracking(ord),
            onPlaceAnother: ShowNewOrder));
    }

    private void ShowTracking(Order? order)
    {
        _currentScreen = "tracking";
        _selectedTrackOrder = order;
        SetNav(
            (LocalizationHelper.T("New Order", "Đặt hàng"), false, ShowNewOrder),
            (LocalizationHelper.T("My Orders", "Đơn của tôi"), true, () => ShowTracking(null))
        );
        Mount(new OrderTrackingView(_ctx, order));
    }

    private void ShowKitchen()
    {
        _currentScreen = "kitchen";
        SetNav((LocalizationHelper.T("Orders", "Đơn hàng"), true, ShowKitchen));
        Mount(new KitchenView(_ctx));
    }

    private void ShowShipper()
    {
        _currentScreen = "shipper";
        SetNav((LocalizationHelper.T("Deliveries", "Giao hàng"), true, ShowShipper));
        Mount(new ShipperView(_ctx));
    }

    private void ShowAdmin(string tab)
    {
        _currentScreen = tab == "reports" ? "adminReports" : "adminOrders";
        SetNav(
            (LocalizationHelper.T("Orders", "Đơn hàng"), tab == "orders", () => ShowAdmin("orders")),
            (LocalizationHelper.T("Reports", "Báo cáo"), tab == "reports", () => ShowAdmin("reports"))
        );
        Mount(new AdminView(_ctx, tab));
    }

    private void RefreshCurrentView()
    {
        switch (_currentScreen)
        {
            case "auth": ShowAuth(); break;
            case "newOrder": ShowNewOrder(); break;
            case "payment": ShowPayment(); break;
            case "tracking": ShowTracking(_selectedTrackOrder); break;
            case "kitchen": ShowKitchen(); break;
            case "shipper": ShowShipper(); break;
            case "adminOrders": ShowAdmin("orders"); break;
            case "adminReports": ShowAdmin("reports"); break;
        }
    }
}
