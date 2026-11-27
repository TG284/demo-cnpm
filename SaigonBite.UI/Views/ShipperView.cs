using System.Drawing;
using System.Windows.Forms;
using SaigonBite.Models;

namespace SaigonBite.UI.Views;

/// <summary>Matches Figma Pages 13, 27–29, 33–35: Ready orders at hub (Claim) + My delivery card with BR14 COD cash confirmation guard.</summary>
public class ShipperView : UserControl
{
    private readonly DemoAppContext _ctx;
    private Order? _activeDelivery;

    private readonly DataGridView _readyGrid;
    private readonly Label _lblDelivTitle;
    private readonly Label _lblDelivPill;
    private readonly Label _lblCust;
    private readonly Label _lblAddr;
    private readonly Label _lblPay;
    private readonly Label _lblCollect;
    private readonly CheckBox _chkCod;
    private readonly Button _btnDelivered;
    private readonly Button _btnFailed;
    private readonly Label _lblHint;

    public ShipperView(DemoAppContext ctx)
    {
        _ctx = ctx;
        Dock = DockStyle.Fill;
        BackColor = SaigonBiteTheme.BgCanvas;
        Padding = new Padding(20, 14, 20, 16);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));

        // Left: Ready orders at hub
        var leftCard = SaigonBiteTheme.CreateCard(10);
        leftCard.Dock = DockStyle.Fill;
        leftCard.Padding = new Padding(16, 12, 16, 12);
        leftCard.Margin = new Padding(0, 0, 14, 0);

        var lblReady = new Label { Text = LocalizationHelper.T("Ready orders at hub", "Đơn sẵn sàng tại hub"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Dock = DockStyle.Top, Height = 30 };
        _readyGrid = new DataGridView { Dock = DockStyle.Fill };
        SaigonBiteTheme.StyleDataGrid(_readyGrid);
        _readyGrid.Columns.Add("Order", LocalizationHelper.T("Order", "Đơn"));
        _readyGrid.Columns.Add("Address", LocalizationHelper.T("Address", "Địa chỉ giao"));
        _readyGrid.Columns.Add("TotalPay", LocalizationHelper.T("Total · Payment", "Tổng · Thanh toán"));
        var colClaim = new DataGridViewButtonColumn { Name = "Claim", HeaderText = LocalizationHelper.T("Action", "Thao tác"), Text = LocalizationHelper.T("Claim", "Nhận đơn"), UseColumnTextForButtonValue = true, Width = 86 };
        _readyGrid.Columns.Add(colClaim);
        _readyGrid.Columns["Order"]!.Width = 72;
        _readyGrid.Columns["Address"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _readyGrid.Columns["TotalPay"]!.Width = 245;
        _readyGrid.CellContentClick += ReadyGrid_CellClick;

        leftCard.Controls.Add(_readyGrid);
        leftCard.Controls.Add(lblReady);

        // Right: My delivery card
        var rightCard = SaigonBiteTheme.CreateCard(10);
        rightCard.Dock = DockStyle.Fill;
        rightCard.Padding = new Padding(20, 16, 20, 16);

        _lblDelivTitle = new Label { Text = LocalizationHelper.T("Active delivery", "Đơn đang giao"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true };
        _lblDelivPill = SaigonBiteTheme.CreateStatusPill(LocalizationHelper.Status(OrderStatus.OutForDelivery), SaigonBiteTheme.AccentOrange, Color.White);
        _lblDelivPill.Location = new Point(20, 44);

        _lblCust = new Label { Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(20, 82), Width = 360, AutoSize = false, Height = 22 };
        _lblAddr = new Label { Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(20, 108), Width = 360, AutoSize = false, Height = 22 };
        _lblPay  = new Label { Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(20, 134), Width = 360, AutoSize = false, Height = 22 };
        _lblCollect = new Label { Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.AccentOrange, Location = new Point(20, 162), Width = 360, AutoSize = false, Height = 24 };

        _chkCod = new CheckBox { Text = LocalizationHelper.T("Paid", "Đã thanh toán"), Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(20, 196), AutoSize = true };
        _chkCod.CheckedChanged += (_, _) => UpdateDeliveryButtons();

        _btnDelivered = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Delivered", "Đã giao"), 350, 38);
        _btnDelivered.Location = new Point(20, 232);
        _btnDelivered.Click += (_, _) => CompleteDelivery(OrderStatus.Delivered);

        _btnFailed = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Delivery Failed", "Giao thất bại"), 350, 36);
        _btnFailed.Location = new Point(20, 280);
        _btnFailed.Click += (_, _) => CompleteDelivery(OrderStatus.Failed);

        _lblHint = new Label { Font = SaigonBiteTheme.FontSmall, ForeColor = SaigonBiteTheme.DangerRed, Location = new Point(20, 326), Width = 350, Height = 36 };

        rightCard.Controls.AddRange(new Control[]
        {
            _lblDelivTitle, _lblDelivPill, _lblCust, _lblAddr, _lblPay, _lblCollect,
            _chkCod, _btnDelivered, _btnFailed, _lblHint
        });

        layout.Controls.Add(leftCard, 0, 0);
        layout.Controls.Add(rightCard, 1, 0);
        Controls.Add(layout);

        RefreshView();
    }

    public void RefreshView()
    {
        var all = _ctx.OrderRepo.GetAllOrders();
        var ready = all.Where(o => o.Status == OrderStatus.Ready).OrderByDescending(o => o.OrderID).ToList();
        _readyGrid.Rows.Clear();
        foreach (var o in ready)
        {
            int idx = _readyGrid.Rows.Add($"#{o.OrderID}", LocalizationHelper.Address(o.DeliveryAddress), $"{o.Pricing.RoundedTotal:N0} · {LocalizationHelper.PayMethod(o.PaymentMethod)}");
            _readyGrid.Rows[idx].Tag = o.OrderID;
        }

        _activeDelivery = all.FirstOrDefault(o => o.Status == OrderStatus.OutForDelivery);
        RenderActiveDelivery();
    }

    private void RenderActiveDelivery()
    {
        if (_activeDelivery == null)
        {
            _lblDelivTitle.Text = LocalizationHelper.T("No active delivery", "Chưa có đơn đang giao");
            _lblDelivPill.Visible = false;
            _lblCust.Text = LocalizationHelper.T("Claim a Ready order on the left.", "Bấm Nhận đơn ở danh sách bên trái.");
            _lblAddr.Text = _lblPay.Text = _lblCollect.Text = _lblHint.Text = "";
            _chkCod.Visible = _btnDelivered.Visible = _btnFailed.Visible = false;
            return;
        }
        var o = _activeDelivery;
        _lblDelivTitle.Text = LocalizationHelper.T($"Active delivery #{o.OrderID}", $"Đơn đang giao #{o.OrderID}");
        _lblDelivPill.Visible = true;
        var custName = LocalizationHelper.ActorName(_ctx.Users.FirstOrDefault(u => u.UserID == o.CustomerID)?.FullName ?? $"#{o.CustomerID}");
        _lblCust.Text = LocalizationHelper.T($"Customer: {custName}", $"Khách hàng: {custName}");
        _lblAddr.Text = LocalizationHelper.T($"Address: {LocalizationHelper.Address(o.DeliveryAddress)}", $"Địa chỉ: {LocalizationHelper.Address(o.DeliveryAddress)}");
        _lblPay.Text = LocalizationHelper.T($"Payment: {LocalizationHelper.PayMethod(o.PaymentMethod)}", $"Hình thức thanh toán: {LocalizationHelper.PayMethod(o.PaymentMethod)}");
        _lblCollect.Text = LocalizationHelper.T($"Total: {o.Pricing.RoundedTotal:N0} VND", $"Tổng tiền: {o.Pricing.RoundedTotal:N0} VNĐ");
        _chkCod.Visible = o.PaymentMethod == "COD";
        _chkCod.Checked = false;
        _btnDelivered.Visible = _btnFailed.Visible = true;
        UpdateDeliveryButtons();
    }

    private void UpdateDeliveryButtons()
    {
        if (_activeDelivery == null) return;
        bool needCod = _activeDelivery.PaymentMethod == "COD" && !_chkCod.Checked;
        _btnDelivered.Enabled = !needCod;
        _btnDelivered.BackColor = needCod ? ColorTranslator.FromHtml("#F3B59B") : SaigonBiteTheme.AccentOrange;
        _lblHint.Text = "";
    }

    private void ReadyGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _readyGrid.Columns[e.ColumnIndex].Name != "Claim") return;
        int oid = (int)_readyGrid.Rows[e.RowIndex].Tag!;
        try
        {
            _ctx.OrderService.TransitionState(oid, OrderStatus.OutForDelivery, _ctx.CurrentUser!, LocalizationHelper.T("Claimed ready order", "Nhận đơn sẵn sàng"));
            RefreshView();
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Claim error", "Lỗi nhận đơn"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CompleteDelivery(OrderStatus target)
    {
        if (_activeDelivery == null) return;
        try
        {
            string reason = target == OrderStatus.Failed ? LocalizationHelper.T("Delivery failed", "Giao thất bại") : LocalizationHelper.T("Delivered", "Đã giao");
            _ctx.OrderService.TransitionState(_activeDelivery.OrderID, target, _ctx.CurrentUser!, reason, codCashConfirmed: _chkCod.Checked);
            MessageBox.Show(
                target == OrderStatus.Delivered
                    ? LocalizationHelper.T($"Order #{_activeDelivery.OrderID} marked Delivered.", $"Đơn #{_activeDelivery.OrderID} đã được đánh dấu Đã giao.")
                    : LocalizationHelper.T($"Order #{_activeDelivery.OrderID} marked Failed.", $"Đơn #{_activeDelivery.OrderID} đã được đánh dấu Giao thất bại."),
                LocalizationHelper.T("Delivery updated", "Đã cập nhật giao hàng"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshView();
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Cannot update delivery", "Không thể cập nhật giao hàng"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
