using System.Drawing;
using System.Windows.Forms;
using SaigonBite.Models;

namespace SaigonBite.UI.Views;

/// <summary>Redesigned Payment Selection view with branded gateway badges and dish photo thumbnails (BR13–BR14 + Week 5 CR).</summary>
public class PaymentView : UserControl
{
    private readonly DemoAppContext _ctx;
    private readonly Hub _hub;
    private readonly List<OrderLine> _lines;
    private readonly OrderPriceSnapshot _snapshot;
    private readonly Voucher? _voucher;
    private readonly PriceBreakdown _breakdown;
    private readonly Action _onBack;
    private readonly Action<Order> _onTrackOrder;
    private readonly Action _onPlaceAnother;

    private string _selectedMethod = "";
    private readonly Panel _checkoutPanel;
    private readonly FlowLayoutPanel _methodsFlow;
    private readonly RoundedPanel _confirmCard;

    public PaymentView(
        DemoAppContext ctx, Hub hub, List<OrderLine> lines, OrderPriceSnapshot snapshot,
        Voucher? voucher, PriceBreakdown breakdown,
        Action onBack, Action<Order> onTrackOrder, Action onPlaceAnother)
    {
        _ctx = ctx;
        _hub = hub;
        _lines = lines;
        _snapshot = snapshot;
        _voucher = voucher;
        _breakdown = breakdown;
        _onBack = onBack;
        _onTrackOrder = onTrackOrder;
        _onPlaceAnother = onPlaceAnother;

        Dock = DockStyle.Fill;
        BackColor = SaigonBiteTheme.BgCanvas;

        _checkoutPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

        var leftCard = SaigonBiteTheme.CreateCard(14);
        leftCard.Location = new Point(28, 22);
        leftCard.Size = new Size(790, 560);

        var lblTitle = new Label
        {
            Text = LocalizationHelper.T("Choose payment method", "Chọn phương thức thanh toán"),
            Font = SaigonBiteTheme.FontTitle,
            ForeColor = SaigonBiteTheme.TextPrimary,
            Location = new Point(24, 20),
            AutoSize = true
        };
        var lblSub = new Label
        {
            Text = _ctx.CurrentUser?.Role == UserRole.ForeignTourist
                ? LocalizationHelper.T("Available payment methods for Foreign Tourists", "Phương thức thanh toán cho Khách nước ngoài")
                : LocalizationHelper.T("Available payment methods for Local Customers", "Phương thức thanh toán cho Khách địa phương"),
            Font = SaigonBiteTheme.FontSmall,
            ForeColor = SaigonBiteTheme.TextMuted,
            Location = new Point(26, 48),
            AutoSize = true
        };

        _methodsFlow = new FlowLayoutPanel
        {
            Location = new Point(24, 82),
            Size = new Size(742, 360),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        var noteBox = SaigonBiteTheme.CreateCard(10, SaigonBiteTheme.AmberSoft, ColorTranslator.FromHtml("#FDE68A"));
        noteBox.Location = new Point(24, 466);
        noteBox.Size = new Size(742, 58);
        var lblNote = new Label
        {
            Text = LocalizationHelper.T(
                "Note: COD is only available for orders up to 1,000,000 VND.",
                "Lưu ý: Thanh toán khi nhận hàng chỉ áp dụng cho đơn từ 1,000,000 VNĐ trở xuống."),
            Font = SaigonBiteTheme.FontSmallBold,
            ForeColor = SaigonBiteTheme.AmberWarn,
            Location = new Point(14, 12),
            Size = new Size(712, 36)
        };
        noteBox.Controls.Add(lblNote);
        leftCard.Controls.AddRange(new Control[] { lblTitle, lblSub, _methodsFlow, noteBox });

        // Right Order Total Card with mini dish photos
        var rightCard = SaigonBiteTheme.CreateCard(14);
        rightCard.Location = new Point(842, 22);
        rightCard.Size = new Size(430, 560);

        var lblSum = new Label { Text = LocalizationHelper.T("Order summary", "Tổng đơn hàng"), Font = SaigonBiteTheme.FontTitle, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(22, 20), AutoSize = true };
        rightCard.Controls.Add(lblSum);

        int y = 58;
        foreach (var line in _lines.Take(3))
        {
            var mini = SaigonBiteTheme.CreateCard(10, SaigonBiteTheme.BgHeaderRow, SaigonBiteTheme.BorderSubtle);
            mini.Location = new Point(22, y);
            mini.Size = new Size(386, 54);
            var pic = new PictureBox { Location = new Point(7, 7), Size = new Size(40, 40), SizeMode = PictureBoxSizeMode.Zoom, Image = AssetLoader.GetDishImage(line.ItemName) };
            var lName = new Label { Text = $"{line.Quantity}× {LocalizationHelper.DishName(line.ItemName)}", Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(56, 8), AutoSize = true };
            var lSub = new Label { Text = $"{line.LineTotal:N0} {LocalizationHelper.Currency()}", Font = SaigonBiteTheme.FontSmall, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(56, 28), AutoSize = true };
            mini.Controls.AddRange(new Control[] { pic, lName, lSub });
            rightCard.Controls.Add(mini);
            y += 62;
        }

        void AddRow(string k, string v)
        {
            rightCard.Controls.Add(new Label { Text = k, Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(22, y), AutoSize = true });
            rightCard.Controls.Add(new Label { Text = v, Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(210, y), Size = new Size(198, 22), TextAlign = ContentAlignment.MiddleRight });
            y += 30;
        }

        y += 8;
        int totalQty = _lines.Sum(l => l.Quantity);
        decimal delivAll = _breakdown.DeliveryFeeD + _breakdown.RushSurchargeR + _breakdown.RainSurchargeR2;
        AddRow("Hub", LocalizationHelper.HubName(_hub.HubName));
        AddRow(LocalizationHelper.T("Items", "Số lượng"), $"{totalQty} " + LocalizationHelper.T("items", "món"));
        AddRow(LocalizationHelper.T("Items total", "Giá món"), $"{_breakdown.FoodTotalF:N0} {LocalizationHelper.Currency()}");
        AddRow(LocalizationHelper.T("Shipping + surcharges", "Phí vận chuyển + phụ phí"), $"{delivAll:N0} {LocalizationHelper.Currency()}");

        var totBox = SaigonBiteTheme.CreateCard(10, SaigonBiteTheme.BgSelected, ColorTranslator.FromHtml("#F3C4AE"));
        totBox.Location = new Point(22, y + 8);
        totBox.Size = new Size(386, 50);
        totBox.Controls.Add(new Label { Text = LocalizationHelper.T("Total", "Tổng cộng"), Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(14, 15), AutoSize = true });
        totBox.Controls.Add(new Label { Text = $"{_breakdown.RoundedTotal:N0} {LocalizationHelper.Currency()}", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = SaigonBiteTheme.AccentOrange, Location = new Point(180, 12), Size = new Size(192, 26), TextAlign = ContentAlignment.MiddleRight });
        rightCard.Controls.Add(totBox);

        var btnPay = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Pay & place order", "Thanh toán & đặt hàng"), 386, 42);
        btnPay.Location = new Point(22, y + 72);
        btnPay.Click += (_, _) => PlaceOrderNow();

        var btnBack = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Back to order", "Quay lại đặt hàng"), 386, 36);
        btnBack.Location = new Point(22, y + 122);
        btnBack.Click += (_, _) => _onBack();

        rightCard.Controls.AddRange(new Control[] { btnPay, btnBack });
        _checkoutPanel.Controls.AddRange(new Control[] { leftCard, rightCard });

        _confirmCard = SaigonBiteTheme.CreateCard(14);
        _confirmCard.Size = new Size(560, 320);
        _confirmCard.Location = new Point(360, 140);
        _confirmCard.Visible = false;

        Controls.Add(_checkoutPanel);
        Controls.Add(_confirmCard);
        BuildMethodCards();
    }

    private void BuildMethodCards()
    {
        _methodsFlow.Controls.Clear();
        var role = _ctx.CurrentUser?.Role ?? UserRole.LocalCustomer;
        var allowed = _ctx.PaymentFactory.GetAllowedMethodsFor(role);
        if (string.IsNullOrEmpty(_selectedMethod) || !allowed.Contains(_selectedMethod))
            _selectedMethod = allowed.First();

        foreach (var method in allowed)
        {
            bool sel = method == _selectedMethod;
            bool codBlocked = method == "COD" && _breakdown.RoundedTotal > 1_000_000m;

            var card = SaigonBiteTheme.CreateCard(12, sel ? SaigonBiteTheme.BgSelected : Color.White, sel ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BorderSubtle);
            card.Size = new Size(734, 72);
            card.Margin = new Padding(0, 0, 0, 12);
            card.Cursor = codBlocked ? Cursors.No : Cursors.Hand;

            (string code, Color bg, string sub) = method switch
            {
                "MoMo"     => ("Mo", ColorTranslator.FromHtml("#A50064"), LocalizationHelper.T("Vietnamese e-wallet · instant confirmation", "Ví điện tử phổ biến tại Việt Nam")),
                "VNPay"    => ("VN", ColorTranslator.FromHtml("#005BAA"), LocalizationHelper.T("VNPay-QR / Domestic ATM card", "Quét mã VNPay-QR / Thẻ ATM nội địa")),
                "ZaloPay"  => ("ZP", ColorTranslator.FromHtml("#0068FF"), LocalizationHelper.T("Vietnamese e-wallet", "Ví điện tử ZaloPay")),
                "ApplePay" => ("AP", ColorTranslator.FromHtml("#111827"), LocalizationHelper.T("Express card payment via Apple Pay", "Thanh toán nhanh qua Apple Pay")),
                "PayPal"   => ("PP", ColorTranslator.FromHtml("#003087"), LocalizationHelper.T("International PayPal account", "Tài khoản PayPal quốc tế")),
                _          => ("COD", ColorTranslator.FromHtml("#B45309"), codBlocked ? "Unavailable for orders over 1,000,000 VNĐ" : LocalizationHelper.T("Pay cash upon delivery (≤ 1,000,000 VNĐ)", "Thanh toán tiền mặt khi nhận hàng"))
            };

            var radioDot = new Panel { Location = new Point(18, 25), Size = new Size(20, 20), BackColor = Color.Transparent };
            radioDot.Paint += (_, pe) =>
            {
                pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var pen = new Pen(sel ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BorderSubtle, 2);
                pe.Graphics.DrawEllipse(pen, 1, 1, 17, 17);
                if (sel)
                {
                    using var br = new SolidBrush(SaigonBiteTheme.AccentOrange);
                    pe.Graphics.FillEllipse(br, 5, 5, 9, 9);
                }
            };

            var badge = new PictureBox
            {
                Image = AssetLoader.GetPaymentLogo(method),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Location = new Point(48, 16),
                Size = new Size(52, 38)
            };
            var lblName = new Label
            {
                Text = LocalizationHelper.PayMethod(method),
                Font = SaigonBiteTheme.FontSubTitle,
                ForeColor = codBlocked ? SaigonBiteTheme.TextMuted : SaigonBiteTheme.TextPrimary,
                Location = new Point(112, 23),
                AutoSize = true
            };

            void Pick(object? s, EventArgs e)
            {
                if (codBlocked) return;
                _selectedMethod = method;
                BuildMethodCards();
            }

            card.Click += Pick;
            radioDot.Click += Pick;
            badge.Click += Pick;
            lblName.Click += Pick;

            card.Controls.AddRange(new Control[] { radioDot, badge, lblName });
            if (sel)
            {
                var pillSel = SaigonBiteTheme.CreateStatusPill(LocalizationHelper.T("Selected", "Đang chọn"), SaigonBiteTheme.AccentOrange, Color.White);
                pillSel.Location = new Point(620, 23);
                pillSel.Click += Pick;
                card.Controls.Add(pillSel);
            }
            _methodsFlow.Controls.Add(card);
        }
    }

    private void PlaceOrderNow()
    {
        try
        {
            var user = _ctx.CurrentUser!;
            var order = _ctx.OrderService.PlaceOrder(user, _hub.HubID, "12 Vo Van Ngan, HCMC", _lines, _snapshot, _selectedMethod, _voucher);
            _checkoutPanel.Visible = false;
            _confirmCard.Controls.Clear();
            _confirmCard.Visible = true;

            _confirmCard.Controls.Add(new Label { Text = LocalizationHelper.T($"Order #{order.OrderID} Confirmed!", $"Đặt đơn #{order.OrderID} thành công!"), Font = SaigonBiteTheme.FontHeroTitle, ForeColor = SaigonBiteTheme.SuccessGreen, Location = new Point(32, 32), AutoSize = true });
            string paySt = order.PaymentStatus == "Paid" ? LocalizationHelper.T("Paid", "Đã thanh toán") : LocalizationHelper.T("Pending", "Chờ thanh toán");
            _confirmCard.Controls.Add(new Label { Text = LocalizationHelper.T($"Hub: {LocalizationHelper.HubName(_hub.HubName)}  ·  Payment: {LocalizationHelper.PayMethod(order.PaymentMethod)} ({paySt})", $"Hub: {LocalizationHelper.HubName(_hub.HubName)}  ·  Thanh toán: {LocalizationHelper.PayMethod(order.PaymentMethod)} ({paySt})"), Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(34, 80), AutoSize = true });
            _confirmCard.Controls.Add(new Label { Text = $"{LocalizationHelper.T("Total", "Tổng cộng")}: {order.Pricing.RoundedTotal:N0} {LocalizationHelper.Currency()}", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = SaigonBiteTheme.AccentOrange, Location = new Point(34, 118), AutoSize = true });

            var btnTrack = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Track my order →", "Theo dõi đơn hàng →"), 240, 42);
            btnTrack.Location = new Point(34, 210);
            btnTrack.Click += (_, _) => _onTrackOrder(order);

            var btnNew = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Place another order", "Đặt đơn mới"), 210, 42);
            btnNew.Location = new Point(290, 210);
            btnNew.Click += (_, _) => _onPlaceAnother();

            _confirmCard.Controls.AddRange(new Control[] { btnTrack, btnNew });
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Payment error", "Lỗi thanh toán"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
