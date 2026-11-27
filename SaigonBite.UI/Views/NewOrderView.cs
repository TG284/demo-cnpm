using System.Drawing;
using System.Windows.Forms;
using SaigonBite.Models;

namespace SaigonBite.UI.Views;

/// <summary>Redesigned New Order screen with Saigon culinary hero banner, visual load bars, real dish photo cards, and live BR1–BR12 summary.</summary>
public class NewOrderView : UserControl
{
    private readonly DemoAppContext _ctx;
    private readonly Action<Hub, List<OrderLine>, OrderPriceSnapshot, Voucher?, PriceBreakdown> _onContinuePayment;

    private Hub _selectedHub;
    private string _selectedCategory = "All";
    private readonly Dictionary<int, int> _quantities = new();
    private Voucher? _appliedVoucher;
    private PriceBreakdown? _lastBreakdown;

    private readonly HeroBannerPanel _heroBanner;
    private readonly Label _lblBannerBadge;
    private readonly Label _lblBannerTitle;
    private readonly Label _lblBannerDesc;
    private readonly FlowLayoutPanel _hubFlow;
    private readonly FlowLayoutPanel _catFlow;
    private readonly Label _lblMenuTitle;
    private readonly FlowLayoutPanel _dishFlow;

    private readonly TextBox _txtDistance;
    private readonly TextBox _txtVoucher;
    private readonly CheckBox _chkRush;
    private readonly CheckBox _chkRain;
    private readonly Label _lblVoucherStatus;
    private readonly Label _valSubtotal = new();
    private readonly Label _valQuiet = new();
    private readonly Label _valVoucher = new();
    private readonly Label _valFood = new();
    private readonly Label _valDelivery = new();
    private readonly Label _valRush = new();
    private readonly Label _valRain = new();
    private readonly Label _valTotal = new();
    private readonly Label _lblDeliveryBand = new();
    private readonly Label _lblError = new();
    private readonly Button _btnContinue;

    public NewOrderView(
        DemoAppContext ctx,
        Action<Hub, List<OrderLine>, OrderPriceSnapshot, Voucher?, PriceBreakdown> onContinuePayment)
    {
        _ctx = ctx;
        _onContinuePayment = onContinuePayment;
        Dock = DockStyle.Fill;
        BackColor = SaigonBiteTheme.BgCanvas;

        _selectedHub = _ctx.Hubs.First(h => h.HubID == 3); // Default Thu Duc hub; cart starts empty
        _appliedVoucher = null;

        // LEFT COLUMN (Scrollable culinary workspace)
        var leftScroll = new Panel { Location = new Point(22, 14), Size = new Size(850, 650), AutoScroll = true, BackColor = Color.Transparent };

        // Hero Banner
        _heroBanner = new HeroBannerPanel
        {
            BackgroundPhoto = AssetLoader.GetHeroImage(),
            HorizontalScrim = true,
            CornerRadius = 14,
            Location = new Point(0, 0),
            Size = new Size(826, 112)
        };
        _lblBannerBadge = new Label
        {
            Text = "  Quiet kitchen 10% off  ",
            Font = SaigonBiteTheme.FontSmallBold,
            BackColor = SaigonBiteTheme.SuccessGreen,
            ForeColor = Color.White,
            Location = new Point(22, 24),
            AutoSize = true,
            Padding = new Padding(4, 2, 4, 2)
        };
        _lblBannerTitle = new Label
        {
            Font = new Font("Segoe UI", 14.5F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Location = new Point(20, 40),
            AutoSize = true,
            Visible = false
        };
        _lblBannerDesc = new Label
        {
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Location = new Point(22, 58),
            Size = new Size(780, 28)
        };
        _heroBanner.Controls.AddRange(new Control[] { _lblBannerBadge, _lblBannerTitle, _lblBannerDesc });
        leftScroll.Controls.Add(_heroBanner);

        // 1. Select Hub Card
        var hubCard = SaigonBiteTheme.CreateCard(12);
        hubCard.Location = new Point(0, 124);
        hubCard.Size = new Size(826, 126);

        var lblStep1 = new Label
        {
            Text = LocalizationHelper.T("1. Select cloud-kitchen hub", "1. Chọn hub giao hàng"),
            Font = SaigonBiteTheme.FontSubTitle,
            ForeColor = SaigonBiteTheme.TextPrimary,
            Location = new Point(16, 12),
            AutoSize = true
        };
        _hubFlow = new FlowLayoutPanel { Location = new Point(14, 38), Size = new Size(800, 82), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
        hubCard.Controls.AddRange(new Control[] { lblStep1, _hubFlow });
        leftScroll.Controls.Add(hubCard);

        // 2. Choose Items Card with Photo Cards
        var menuCard = SaigonBiteTheme.CreateCard(12);
        menuCard.Location = new Point(0, 260);
        menuCard.Size = new Size(826, 384);

        _lblMenuTitle = new Label { Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(16, 14), AutoSize = true };
        _catFlow = new FlowLayoutPanel { Location = new Point(360, 10), Size = new Size(454, 34), FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent };
        _dishFlow = new FlowLayoutPanel
        {
            Location = new Point(14, 48),
            Size = new Size(798, 324),
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent
        };
        menuCard.Controls.AddRange(new Control[] { _lblMenuTitle, _catFlow, _dishFlow });
        leftScroll.Controls.Add(menuCard);

        // RIGHT COLUMN: Elevated Order Summary Card
        var summaryCard = SaigonBiteTheme.CreateCard(14);
        summaryCard.Location = new Point(888, 14);
        summaryCard.Size = new Size(392, 644);

        var lblSumTitle = new Label
        {
            Text = LocalizationHelper.T("Order", "Đơn hàng"),
            Font = SaigonBiteTheme.FontTitle,
            ForeColor = SaigonBiteTheme.TextPrimary,
            Location = new Point(20, 16),
            AutoSize = true
        };

        var lblDist = new Label { Text = LocalizationHelper.T("Delivery distance (max 8 km)", "Khoảng cách giao (tối đa 8 km)"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(20, 52), AutoSize = true };
        _txtDistance = new TextBox { Text = "", PlaceholderText = LocalizationHelper.T("e.g. 3.5", "VD: 3.5"), Font = SaigonBiteTheme.FontBody, Location = new Point(20, 72), Width = 352 };
        _txtDistance.TextChanged += (_, _) => Recalculate();

        var lblVouch = new Label { Text = LocalizationHelper.T("Voucher code (max 1)", "Mã voucher (tối đa 1)"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(20, 106), AutoSize = true };
        _txtVoucher = new TextBox { Text = "", PlaceholderText = LocalizationHelper.T("Enter voucher code", "Nhập mã voucher"), Font = SaigonBiteTheme.FontBody, Location = new Point(20, 126), Width = 252 };
        var btnApplyVoucher = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Apply", "Áp dụng"), 90, 28);
        btnApplyVoucher.Location = new Point(282, 125);
        btnApplyVoucher.Click += (_, _) => ApplyVoucher();

        _lblVoucherStatus = new Label { Font = SaigonBiteTheme.FontSmall, ForeColor = SaigonBiteTheme.SuccessGreen, Location = new Point(20, 156), Size = new Size(352, 18) };

        _chkRush = new CheckBox { Text = LocalizationHelper.T("Rush hour (+20%)", "Giờ cao điểm (+20%)"), Checked = false, Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(20, 180), AutoSize = true };
        _chkRush.CheckedChanged += (_, _) => Recalculate();

        _chkRain = new CheckBox { Text = LocalizationHelper.T("Rain (+10,000 VND)", "Phụ phí mưa (+10k)"), Checked = false, Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(195, 180), AutoSize = true };
        _chkRain.CheckedChanged += (_, _) => Recalculate();

        summaryCard.Controls.AddRange(new Control[] { lblSumTitle, lblDist, _txtDistance, lblVouch, _txtVoucher, btnApplyVoucher, _lblVoucherStatus, _chkRush, _chkRain });

        int y = 216;
        void AddPriceRow(string labelText, Label valLabel, bool bold = false, Color? valColor = null, Label? customLeft = null)
        {
            var left = customLeft ?? new Label();
            left.Text = labelText;
            left.Font = bold ? SaigonBiteTheme.FontBodyBold : SaigonBiteTheme.FontBody;
            left.ForeColor = bold ? SaigonBiteTheme.TextPrimary : SaigonBiteTheme.TextMuted;
            left.Location = new Point(20, y);
            left.AutoSize = true;

            valLabel.Font = bold ? SaigonBiteTheme.FontBodyBold : SaigonBiteTheme.FontBody;
            valLabel.ForeColor = valColor ?? SaigonBiteTheme.TextPrimary;
            valLabel.Location = new Point(220, y);
            valLabel.Size = new Size(152, 22);
            valLabel.TextAlign = ContentAlignment.MiddleRight;

            summaryCard.Controls.Add(left);
            summaryCard.Controls.Add(valLabel);
            y += 31;
        }

        AddPriceRow(LocalizationHelper.T("Items total", "Giá món"), _valSubtotal);
        AddPriceRow(LocalizationHelper.T("Quiet kitchen (−10%)", "Giảm bếp vắng (−10%)"), _valQuiet, false, SaigonBiteTheme.SuccessGreen);
        AddPriceRow(LocalizationHelper.T("Voucher", "Voucher"), _valVoucher, false, SaigonBiteTheme.SuccessGreen);
        AddPriceRow(LocalizationHelper.T("Shipping fee", "Phí vận chuyển"), _valDelivery, false, null, _lblDeliveryBand);
        AddPriceRow(LocalizationHelper.T("Rush-hour fee", "Phụ phí giờ cao điểm"), _valRush);
        AddPriceRow(LocalizationHelper.T("Rain fee", "Phụ phí mưa"), _valRain);

        var totalBox = SaigonBiteTheme.CreateCard(10, SaigonBiteTheme.BgSelected, ColorTranslator.FromHtml("#F3C4AE"));
        totalBox.Location = new Point(20, y + 10);
        totalBox.Size = new Size(352, 52);
        var lblTot = new Label { Text = LocalizationHelper.T("Total", "Tổng cộng"), Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(14, 15), AutoSize = true };
        _valTotal.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _valTotal.ForeColor = SaigonBiteTheme.AccentOrange;
        _valTotal.Location = new Point(188, 12);
        _valTotal.Size = new Size(150, 28);
        _valTotal.TextAlign = ContentAlignment.MiddleRight;
        totalBox.Controls.AddRange(new Control[] { lblTot, _valTotal });
        summaryCard.Controls.Add(totalBox);

        _lblError.Font = SaigonBiteTheme.FontSmallBold;
        _lblError.ForeColor = SaigonBiteTheme.DangerRed;
        _lblError.Location = new Point(20, y + 70);
        _lblError.Size = new Size(352, 34);
        summaryCard.Controls.Add(_lblError);

        _btnContinue = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Continue to payment", "Tiếp tục thanh toán"), 352, 42);
        _btnContinue.Location = new Point(20, y + 112);
        _btnContinue.Click += (_, _) => ContinueToPayment();
        summaryCard.Controls.Add(_btnContinue);

        Controls.Add(leftScroll);
        Controls.Add(summaryCard);

        RenderHubCards();
        RenderCategories();
        RenderDishCards();
        Recalculate();
    }

    private void UpdateHeroBanner()
    {
        bool isQk = _selectedHub.CurrentLoadPct < 30m;
        _lblBannerBadge.Text = isQk
            ? LocalizationHelper.T("  Quiet kitchen 10% off  ", "  Bếp vắng giảm giá 10%  ")
            : LocalizationHelper.T("  SaigonBite Cloud Kitchen  ", "  Bếp Đám Mây SaigonBite  ");
        _lblBannerBadge.BackColor = isQk ? SaigonBiteTheme.SuccessGreen : SaigonBiteTheme.AccentOrange;
        _lblBannerTitle.Visible = false;
        _lblBannerDesc.Text = isQk
            ? LocalizationHelper.T("10% off dishes · Free shipping for orders from 300,000 VND.", "Giảm 10% giá món · Miễn phí vận chuyển cho đơn từ 300,000 VNĐ.")
            : LocalizationHelper.T("Free shipping for orders from 300,000 VND.", "Miễn phí vận chuyển cho đơn từ 300,000 VNĐ.");
    }

    private void RenderHubCards()
    {
        _hubFlow.Controls.Clear();
        UpdateHeroBanner();
        foreach (var hub in _ctx.Hubs.Where(h => h.IsActive))
        {
            bool selected = hub.HubID == _selectedHub.HubID;
            bool full = hub.CurrentLoadPct >= 90m;
            bool quiet = hub.CurrentLoadPct < 30m;

            var card = SaigonBiteTheme.CreateCard(
                10,
                selected ? SaigonBiteTheme.BgSelected : (full ? ColorTranslator.FromHtml("#FFF6F6") : Color.White),
                selected ? SaigonBiteTheme.AccentOrange : (full ? ColorTranslator.FromHtml("#FECACA") : SaigonBiteTheme.BorderSubtle));
            card.Size = new Size(150, 74);
            card.Margin = new Padding(0, 0, 8, 0);
            card.Cursor = Cursors.Hand;

            var lblName = new Label { Text = LocalizationHelper.HubName(hub.HubName), Font = SaigonBiteTheme.FontBodyBold, ForeColor = selected ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.TextPrimary, Location = new Point(10, 8), AutoSize = true };
            var lblPct = new Label
            {
                Text = $"{hub.CurrentLoadPct:0}%",
                Font = SaigonBiteTheme.FontSmallBold,
                BackColor = full ? SaigonBiteTheme.DangerSoft : (quiet ? SaigonBiteTheme.SuccessSoft : SaigonBiteTheme.AmberSoft),
                ForeColor = full ? SaigonBiteTheme.DangerRed : (quiet ? SaigonBiteTheme.SuccessGreen : SaigonBiteTheme.AmberWarn),
                Location = new Point(102, 8),
                Size = new Size(38, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var bar = new LoadBarPanel { Percent = hub.CurrentLoadPct, Location = new Point(10, 33), Size = new Size(130, 6) };
            var lblSub = new Label
            {
                Text = full
                    ? LocalizationHelper.T("Paused – not accepting", "Tạm ngừng nhận đơn")
                    : (quiet ? LocalizationHelper.T("Quiet kitchen 10% off", "Bếp vắng giảm giá 10%") : ""),
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = full ? SaigonBiteTheme.DangerRed : (quiet ? SaigonBiteTheme.SuccessGreen : SaigonBiteTheme.TextMuted),
                Location = new Point(10, 46),
                Size = new Size(134, 22)
            };

            void OnPick(object? s, EventArgs e)
            {
                if (hub.CurrentLoadPct >= 90m)
                {
                    var avail = _ctx.Hubs.Where(h => h.CurrentLoadPct < 90m).Select(h =>
                        LocalizationHelper.HubName(h.HubName) + (h.CurrentLoadPct < 30m ? LocalizationHelper.T(" (quiet kitchen 10% off)", " (bếp vắng giảm giá 10%)") : ""));
                    var hubList = string.Join(", ", avail);
                    MessageBox.Show(
                        LocalizationHelper.T(
                            $"{LocalizationHelper.HubName(hub.HubName)} hub is currently overloaded. New orders are paused to maintain food quality. Please choose another hub: {hubList}.",
                            $"Hub {LocalizationHelper.HubName(hub.HubName)} hiện đang quá tải. Tạm ngừng nhận đơn mới để đảm bảo chất lượng món ăn. Vui lòng chọn hub khác: {hubList}."),
                        LocalizationHelper.T($"{LocalizationHelper.HubName(hub.HubName)} hub overloaded", $"Hub {LocalizationHelper.HubName(hub.HubName)} đang quá tải"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
                _selectedHub = hub;
                _quantities.Clear();
                RenderHubCards();
                RenderDishCards();
                Recalculate();
            }

            card.Click += OnPick;
            lblName.Click += OnPick;
            lblPct.Click += OnPick;
            bar.Click += OnPick;
            lblSub.Click += OnPick;

            card.Controls.AddRange(new Control[] { lblName, lblPct, bar, lblSub });
            _hubFlow.Controls.Add(card);
        }
    }

    private void RenderCategories()
    {
        _catFlow.Controls.Clear();
        var cats = new[] { "Drinks", "Bread", "Rice", "Noodle Soups", "All" }; // RightToLeft
        foreach (var cat in cats)
        {
            var label = LocalizationHelper.Category(cat);
            int w = cat == "Noodle Soups" ? 104 : (cat == "All" ? 64 : (label.Length > 5 ? 76 : 58));
            var btn = SaigonBiteTheme.CreatePillButton(label, _selectedCategory == cat, w, 26);
            btn.Margin = new Padding(6, 0, 0, 0);
            btn.Click += (_, _) => { _selectedCategory = cat; RenderCategories(); RenderDishCards(); };
            _catFlow.Controls.Add(btn);
        }
    }

    private void RenderDishCards()
    {
        _lblMenuTitle.Text = LocalizationHelper.T($"2. Choose items – {LocalizationHelper.HubName(_selectedHub.HubName)} menu", $"2. Chọn món – Thực đơn {LocalizationHelper.HubName(_selectedHub.HubName)}");
        _dishFlow.Controls.Clear();

        var items = _ctx.MenuItems
            .Where(m => m.HubID == _selectedHub.HubID && m.IsActive && m.Status == "Available")
            .Where(m => _selectedCategory == "All" || string.Equals(m.Category, _selectedCategory, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var item in items)
        {
            int qty = _quantities.GetValueOrDefault(item.ItemID, 0);
            var card = SaigonBiteTheme.CreateCard(
                12,
                qty > 0 ? ColorTranslator.FromHtml("#FFFBF8") : Color.White,
                qty > 0 ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BorderSubtle);
            card.Size = new Size(382, 98);
            card.Margin = new Padding(0, 0, 10, 10);

            var pic = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(78, 78),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = SaigonBiteTheme.BgHeaderRow,
                Image = AssetLoader.GetDishImage(item.ItemName)
            };

            var lblName = new Label { Text = LocalizationHelper.DishName(item.ItemName), Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(98, 10), Size = new Size(195, 20) };
            var lblCat = new Label { Text = LocalizationHelper.Category(item.Category), Font = SaigonBiteTheme.FontSmall, BackColor = SaigonBiteTheme.BgHeaderRow, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(304, 10), Size = new Size(66, 18), TextAlign = ContentAlignment.MiddleCenter };
            
            var lblPrice = new Label { Text = $"{item.UnitPrice:N0} {LocalizationHelper.Currency()}", Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.AccentOrange, Location = new Point(98, 62), AutoSize = true };

            var btnMinus = SaigonBiteTheme.CreateOutlineButton("−", 28, 26);
            btnMinus.Location = new Point(274, 58);
            btnMinus.Click += (_, _) => { _quantities[item.ItemID] = Math.Max(0, _quantities.GetValueOrDefault(item.ItemID, 0) - 1); RenderDishCards(); Recalculate(); };

            var lblQty = new Label { Text = qty.ToString(), Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(304, 60), Size = new Size(32, 22), TextAlign = ContentAlignment.MiddleCenter };

            var btnPlus = SaigonBiteTheme.CreatePrimaryButton("+", 28, 26);
            btnPlus.Location = new Point(338, 58);
            btnPlus.Click += (_, _) => { _quantities[item.ItemID] = Math.Min(20, _quantities.GetValueOrDefault(item.ItemID, 0) + 1); RenderDishCards(); Recalculate(); };

            card.Controls.AddRange(new Control[] { pic, lblName, lblCat, lblPrice, btnMinus, lblQty, btnPlus });
            _dishFlow.Controls.Add(card);
        }
    }

    private void ApplyVoucher()
    {
        var code = _txtVoucher.Text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            _appliedVoucher = null;
            _lblVoucherStatus.Text = "";
            Recalculate();
            return;
        }
        var v = _ctx.Vouchers.FirstOrDefault(x => x.IsActive && string.Equals(x.VoucherCode, code, StringComparison.OrdinalIgnoreCase));
        if (v == null)
        {
            _appliedVoucher = null;
            _lblVoucherStatus.ForeColor = SaigonBiteTheme.DangerRed;
            _lblVoucherStatus.Text = LocalizationHelper.T("Invalid voucher. Try WELCOME15K or SAIGON10.", "Mã không hợp lệ. Thử WELCOME15K.");
        }
        else
        {
            _appliedVoucher = v;
            _lblVoucherStatus.ForeColor = SaigonBiteTheme.SuccessGreen;
            _lblVoucherStatus.Text = LocalizationHelper.T($"Applied {v.VoucherCode}", $"Đã áp dụng {v.VoucherCode}");
        }
        Recalculate();
    }

    private List<OrderLine> BuildOrderLines()
    {
        var lines = new List<OrderLine>();
        foreach (var kv in _quantities.Where(k => k.Value > 0))
        {
            var item = _ctx.MenuItems.FirstOrDefault(m => m.ItemID == kv.Key);
            if (item == null) continue;
            lines.Add(new OrderLine
            {
                ItemID = item.ItemID,
                ItemName = item.ItemName,
                BrandCategory = item.Category,
                UnitPrice = item.UnitPrice,
                Quantity = kv.Value
            });
        }
        return lines;
    }

    private OrderPriceSnapshot BuildSnapshot()
    {
        decimal.TryParse(_txtDistance.Text.Trim(), out var km);
        var dt = _chkRush.Checked ? new DateTime(2026, 11, 21, 12, 10, 0) : new DateTime(2026, 11, 21, 15, 0, 0);
        return new OrderPriceSnapshot
        {
            PlacementUtc7 = dt,
            HubLoadPct = _selectedHub.CurrentLoadPct,
            HeavyRainFlag = _chkRain.Checked,
            DistanceKm = km
        };
    }

    private void Recalculate()
    {
        _lblError.Text = "";
        var lines = BuildOrderLines();
        if (lines.Count == 0)
        {
            _lastBreakdown = null;
            _valSubtotal.Text = _valQuiet.Text = _valVoucher.Text = _valFood.Text = _valDelivery.Text = _valRush.Text = _valRain.Text = "0";
            _valTotal.Text = $"0 {LocalizationHelper.Currency()}";
            _btnContinue.Enabled = false;
            return;
        }
        try
        {
            var snap = BuildSnapshot();
            var b = _ctx.Pricing.CalculatePrice(lines, snap, _appliedVoucher);
            _lastBreakdown = b;
            _lblDeliveryBand.Text = LocalizationHelper.T("Shipping fee", "Phí vận chuyển");
            _valSubtotal.Text = $"{b.SubtotalS:N0}";
            _valQuiet.Text = b.QuietKitchenQ > 0 ? $"−{b.QuietKitchenQ:N0}" : "0";
            _valVoucher.Text = b.VoucherV > 0 ? $"−{b.VoucherV:N0}" : "0";
            _valFood.Text = $"{b.FoodTotalF:N0}";
            _valDelivery.Text = $"{b.DeliveryFeeD:N0}";
            _valRush.Text = $"{b.RushSurchargeR:N0}";
            _valRain.Text = $"{b.RainSurchargeR2:N0}";
            _valTotal.Text = $"{b.RoundedTotal:N0} {LocalizationHelper.Currency()}";
            _btnContinue.Enabled = true;
        }
        catch (Exception ex)
        {
            _lastBreakdown = null;
            _lblError.Text = LocalizationHelper.FriendlyError(ex.Message);
            _valTotal.Text = "—";
            _btnContinue.Enabled = false;
        }
    }

    private void ContinueToPayment()
    {
        if (_lastBreakdown == null) return;
        _onContinuePayment(_selectedHub, BuildOrderLines(), BuildSnapshot(), _appliedVoucher, _lastBreakdown);
    }
}
