using System.Drawing;
using System.Windows.Forms;
using SaigonBite.Models;

namespace SaigonBite.UI.Views;

/// <summary>Matches Figma Pages 12, 25, 26, 31, 32: Kitchen Staff incoming queue + status filter pills + mandatory Reject panel.</summary>
public class KitchenView : UserControl
{
    private readonly DemoAppContext _ctx;
    private OrderStatus? _filterStatus;
    private Order? _selectedOrder;

    private readonly Label _lblHeader;
    private readonly FlowLayoutPanel _filterFlow;
    private readonly Label _kpiPlaced;
    private readonly Label _kpiConf;
    private readonly Label _kpiPrep;
    private readonly Label _kpiReady;
    private readonly DataGridView _grid;
    private readonly Label _lblRejectTitle;
    private readonly TextBox _txtReason;

    public KitchenView(DemoAppContext ctx)
    {
        _ctx = ctx;
        Dock = DockStyle.Fill;
        BackColor = SaigonBiteTheme.BgCanvas;
        Padding = new Padding(20, 14, 20, 16);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31F));

        // Left: Incoming orders card
        var leftCard = SaigonBiteTheme.CreateCard(10);
        leftCard.Dock = DockStyle.Fill;
        leftCard.Padding = new Padding(16, 12, 16, 12);
        leftCard.Margin = new Padding(0, 0, 14, 0);

        _lblHeader = new Label { Text = LocalizationHelper.T("Orders – Thu Duc Hub", "Danh sách đơn - Hub Thủ Đức"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Dock = DockStyle.Top, Height = 28 };
        _filterFlow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent, Padding = new Padding(0, 4, 0, 4) };

        _grid = new DataGridView { Dock = DockStyle.Fill };
        SaigonBiteTheme.StyleDataGrid(_grid);
        _grid.Columns.Add("Order", LocalizationHelper.T("Order", "Đơn"));
        _grid.Columns.Add("Items", LocalizationHelper.T("Items", "Món"));
        _grid.Columns.Add("Total", LocalizationHelper.T("Total", "Tổng"));
        _grid.Columns.Add("Status", LocalizationHelper.T("Status", "Trạng thái"));
        var colAct = new DataGridViewButtonColumn { Name = "Action", HeaderText = LocalizationHelper.T("Action", "Thao tác"), Width = 152 };
        _grid.Columns.Add(colAct);
        _grid.Columns["Order"]!.Width = 68;
        _grid.Columns["Items"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _grid.Columns["Total"]!.Width = 84;
        _grid.Columns["Status"]!.Width = 118;
        _grid.SelectionChanged += (_, _) =>
        {
            if (_grid.SelectedRows.Count > 0 && _grid.SelectedRows[0].Tag is int oid)
            {
                _selectedOrder = _ctx.OrderRepo.GetOrderById(oid);
                _lblRejectTitle.Text = _selectedOrder != null ? LocalizationHelper.T($"Reject order #{_selectedOrder.OrderID}", $"Từ chối đơn #{_selectedOrder.OrderID}") : LocalizationHelper.T("Reject order", "Từ chối đơn");
            }
        };
        _grid.CellContentClick += Grid_CellClick;

        var kpiRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 72, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 10) };
        for (int i = 0; i < 4; i++) kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        (Panel card, Label val) MakeKitchenKpi(string label, Color vc, int rightMargin)
        {
            var c = SaigonBiteTheme.CreateCard(10, SaigonBiteTheme.BgCanvas, SaigonBiteTheme.BorderSubtle);
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(0, 0, rightMargin, 0);
            var lt = new Label { Text = label, Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(12, 8), AutoSize = true };
            var lv = new Label { Text = "0", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = vc, Location = new Point(12, 28), AutoSize = true };
            c.Controls.AddRange(new Control[] { lt, lv });
            return (c, lv);
        }
        var (kc1, kv1) = MakeKitchenKpi(LocalizationHelper.Status(OrderStatus.Placed), SaigonBiteTheme.AccentOrange, 8);
        var (kc2, kv2) = MakeKitchenKpi(LocalizationHelper.Status(OrderStatus.Confirmed), SaigonBiteTheme.TextPrimary, 8);
        var (kc3, kv3) = MakeKitchenKpi(LocalizationHelper.Status(OrderStatus.Preparing), SaigonBiteTheme.TextPrimary, 8);
        var (kc4, kv4) = MakeKitchenKpi(LocalizationHelper.T("Ready for pickup", "Sẵn sàng giao"), SaigonBiteTheme.TextPrimary, 0);
        _kpiPlaced = kv1; _kpiConf = kv2; _kpiPrep = kv3; _kpiReady = kv4;
        kpiRow.Controls.Add(kc1, 0, 0); kpiRow.Controls.Add(kc2, 1, 0); kpiRow.Controls.Add(kc3, 2, 0); kpiRow.Controls.Add(kc4, 3, 0);

        leftCard.Controls.Add(_grid);
        leftCard.Controls.Add(_filterFlow);
        leftCard.Controls.Add(_lblHeader);
        leftCard.Controls.Add(kpiRow);

        // Right: Reject order panel (mandatory reason per BR15)
        var rightCard = SaigonBiteTheme.CreateCard(10);
        rightCard.Dock = DockStyle.Fill;
        rightCard.Padding = new Padding(18, 14, 18, 14);

        _lblRejectTitle = new Label { Text = LocalizationHelper.T("Reject order", "Từ chối đơn"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(18, 16), AutoSize = true };
        var lblReason = new Label { Text = LocalizationHelper.T("Reason (mandatory)", "Lý do (bắt buộc)"), Font = SaigonBiteTheme.FontSmall, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(18, 46), AutoSize = true };
        _txtReason = new TextBox { Text = "", PlaceholderText = LocalizationHelper.T("e.g. Out of ingredients, kitchen overloaded...", "VD: Hết nguyên liệu, bếp quá tải..."), Multiline = true, Font = SaigonBiteTheme.FontBody, Location = new Point(18, 66), Width = 310, Height = 90 };

        var btnReject = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Reject order", "Từ chối đơn"), 310, 38);
        btnReject.Location = new Point(18, 170);
        btnReject.Click += (_, _) => RejectSelected();

        rightCard.Controls.AddRange(new Control[] { _lblRejectTitle, lblReason, _txtReason, btnReject });

        layout.Controls.Add(leftCard, 0, 0);
        layout.Controls.Add(rightCard, 1, 0);
        Controls.Add(layout);

        RefreshView();
    }

    public void RefreshView()
    {
        var all = _ctx.OrderRepo.GetAllOrders().OrderByDescending(o => o.OrderID).ToList();
        _kpiPlaced.Text = all.Count(o => o.Status == OrderStatus.Placed).ToString();
        _kpiConf.Text = all.Count(o => o.Status == OrderStatus.Confirmed).ToString();
        _kpiPrep.Text = all.Count(o => o.Status == OrderStatus.Preparing).ToString();
        _kpiReady.Text = all.Count(o => o.Status == OrderStatus.Ready).ToString();
        RenderFilters(all);

        var filtered = _filterStatus.HasValue ? all.Where(o => o.Status == _filterStatus.Value).ToList() : all;
        _grid.Rows.Clear();
        foreach (var o in filtered)
        {
            string items = string.Join(", ", o.Lines.Select(l => $"{l.Quantity}× {LocalizationHelper.DishName(l.ItemName)}"));
            string actText = o.Status switch
            {
                OrderStatus.Placed    => LocalizationHelper.T("Confirm", "Xác nhận"),
                OrderStatus.Confirmed => LocalizationHelper.T("Start preparing", "Bắt đầu chuẩn bị"),
                OrderStatus.Preparing => LocalizationHelper.T("Mark Ready", "Đánh dấu sẵn sàng"),
                OrderStatus.Ready     => LocalizationHelper.T("Waiting for shipper", "Chờ shipper"),
                _                     => LocalizationHelper.Status(o.Status)
            };
            int idx = _grid.Rows.Add($"#{o.OrderID}", items, $"{o.Pricing.RoundedTotal:N0}", LocalizationHelper.Status(o.Status), actText);
            _grid.Rows[idx].Tag = o.OrderID;
        }
    }

    private void RenderFilters(List<Order> all)
    {
        _filterFlow.Controls.Clear();
        var defs = new (string label, OrderStatus? st)[]
        {
            ($"{LocalizationHelper.T("All", "Tất cả")} ({all.Count})", null),
            ($"{LocalizationHelper.Status(OrderStatus.Placed)} ({all.Count(o => o.Status == OrderStatus.Placed)})", OrderStatus.Placed),
            ($"{LocalizationHelper.Status(OrderStatus.Confirmed)} ({all.Count(o => o.Status == OrderStatus.Confirmed)})", OrderStatus.Confirmed),
            ($"{LocalizationHelper.Status(OrderStatus.Preparing)} ({all.Count(o => o.Status == OrderStatus.Preparing)})", OrderStatus.Preparing),
            ($"{LocalizationHelper.Status(OrderStatus.Ready)} ({all.Count(o => o.Status == OrderStatus.Ready)})", OrderStatus.Ready)
        };
        foreach (var (lbl, st) in defs)
        {
            var btn = SaigonBiteTheme.CreatePillButton(lbl, _filterStatus == st, lbl.Length > 13 ? 132 : 106, 26);
            btn.Margin = new Padding(0, 0, 8, 0);
            btn.Click += (_, _) => { _filterStatus = st; RefreshView(); };
            _filterFlow.Controls.Add(btn);
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Action") return;
        int oid = (int)_grid.Rows[e.RowIndex].Tag!;
        var o = _ctx.OrderRepo.GetOrderById(oid);
        if (o == null) return;

        OrderStatus? next = o.Status switch
        {
            OrderStatus.Placed => OrderStatus.Confirmed,
            OrderStatus.Confirmed => OrderStatus.Preparing,
            OrderStatus.Preparing => OrderStatus.Ready,
            _ => null
        };
        if (next == null) return;

        try
        {
            _ctx.OrderService.TransitionState(oid, next.Value, _ctx.CurrentUser!, LocalizationHelper.T($"Kitchen moved to {next}", $"Bếp chuyển sang {LocalizationHelper.Status(next.Value)}"));
            RefreshView();
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Transition error", "Lỗi chuyển trạng thái"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RejectSelected()
    {
        if (_selectedOrder == null) return;
        try
        {
            string reasonText = _txtReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reasonText))
            {
                MessageBox.Show(LocalizationHelper.T("Please enter a rejection reason.", "Vui lòng nhập lý do từ chối đơn hàng."), LocalizationHelper.T("Cannot reject", "Không thể từ chối"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _ctx.OrderService.TransitionState(_selectedOrder.OrderID, OrderStatus.Cancelled, _ctx.CurrentUser!, reasonText);
            _txtReason.Text = "";
            MessageBox.Show(LocalizationHelper.T($"Order #{_selectedOrder.OrderID} rejected and refunded.", $"Đã từ chối và hoàn tiền đơn #{_selectedOrder.OrderID}."), LocalizationHelper.T("Order rejected", "Đã từ chối đơn"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshView();
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Cannot reject", "Không thể từ chối"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
