using System.Drawing;
using System.Windows.Forms;
using SaigonBite.Models;

namespace SaigonBite.UI.Views;

/// <summary>Admin All Orders (Cancel Confirmed) + Report R1 (table & horizontal bar chart) + Report R2 (4 KPI cards + Hub Utilization table).</summary>
public class AdminView : UserControl
{
    private readonly DemoAppContext _ctx;
    private readonly Panel _ordersPanel;
    private readonly Panel _reportsPanel;
    private readonly DataGridView _ordersGrid;

    private readonly Button _btnTabR1;
    private readonly Button _btnTabR2;
    private readonly Panel _r1Container;
    private readonly Panel _r2Container;
    private readonly DataGridView _r1Grid;
    private readonly Panel _r1ChartCanvas;
    private readonly DataGridView _r2Grid;
    private readonly Label _kpiOrdersVal;
    private readonly Label _kpiPrepVal;
    private readonly Label _kpiOverVal;
    private readonly Label _kpiQuietVal;
    private readonly DateTimePicker _dtFrom;
    private readonly DateTimePicker _dtTo;
    private readonly ComboBox _cboCategory;
    private readonly Label _lblR1;
    private readonly Label _lblR2;
    private Dictionary<string, decimal> _r1CategoryTotals = new();

    public AdminView(DemoAppContext ctx, string initialTab = "orders")
    {
        _ctx = ctx;
        Dock = DockStyle.Fill;
        BackColor = SaigonBiteTheme.BgCanvas;
        Padding = new Padding(20, 14, 20, 16);

        // --- ALL ORDERS PANEL ---
        _ordersPanel = SaigonBiteTheme.CreateCard(10);
        _ordersPanel.Dock = DockStyle.Fill;
        _ordersPanel.Padding = new Padding(18, 14, 18, 14);
        var lblAll = new Label { Text = LocalizationHelper.T("Order list", "Danh sách đơn"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Dock = DockStyle.Top, Height = 32, UseMnemonic = false };
        _ordersGrid = new DataGridView { Dock = DockStyle.Fill };
        SaigonBiteTheme.StyleDataGrid(_ordersGrid);
        _ordersGrid.Columns.Add("Order", LocalizationHelper.T("Order", "Đơn"));
        _ordersGrid.Columns.Add("Hub", "Hub");
        _ordersGrid.Columns.Add("Status", LocalizationHelper.T("Status", "Trạng thái"));
        _ordersGrid.Columns.Add("Total", LocalizationHelper.T("Total (VND)", "Tổng (VNĐ)"));
        var colAct = new DataGridViewButtonColumn { Name = "Action", HeaderText = LocalizationHelper.T("Action", "Thao tác"), Width = 155 };
        _ordersGrid.Columns.Add(colAct);
        _ordersGrid.Columns["Hub"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _ordersGrid.Columns["Status"]!.Width = 140;
        _ordersGrid.Columns["Total"]!.Width = 120;
        _ordersGrid.CellContentClick += OrdersGrid_CellClick;
        _ordersPanel.Controls.Add(_ordersGrid);
        _ordersPanel.Controls.Add(lblAll);

        // --- REPORTS PANEL ---
        _reportsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

        var topBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent };
        _btnTabR1 = SaigonBiteTheme.CreatePillButton(LocalizationHelper.T("Revenue", "Doanh thu"), true, 120, 30);
        _btnTabR1.Location = new Point(0, 4);
        _btnTabR1.Click += (_, _) => SwitchReport(showR1: true);

        _btnTabR2 = SaigonBiteTheme.CreatePillButton(LocalizationHelper.T("Hub Utilization", "Hiệu suất Hub"), false, 155, 30);
        _btnTabR2.Location = new Point(128, 4);
        _btnTabR2.Click += (_, _) => SwitchReport(showR1: false);

        var lblFrom = new Label { Text = LocalizationHelper.T("From:", "Từ:"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(300, 10), AutoSize = true };
        _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = new DateTime(2026, 1, 1), Font = SaigonBiteTheme.FontSmall, Location = new Point(338, 6), Width = 104 };
        var lblTo = new Label { Text = LocalizationHelper.T("To:", "Đến:"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(452, 10), AutoSize = true };
        _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = new DateTime(2026, 12, 31), Font = SaigonBiteTheme.FontSmall, Location = new Point(490, 6), Width = 104 };
        var lblCat = new Label { Text = LocalizationHelper.T("Category:", "Danh mục:"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(604, 10), AutoSize = true };
        _cboCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = SaigonBiteTheme.FontSmall, Location = new Point(676, 6), Width = 120 };
        _cboCategory.Items.AddRange(new object[]
        {
            LocalizationHelper.Category("All"),
            LocalizationHelper.Category("Noodles"),
            LocalizationHelper.Category("Rice"),
            LocalizationHelper.Category("Bread"),
            LocalizationHelper.Category("Drinks")
        });
        _cboCategory.SelectedIndex = 0;
        _cboCategory.SelectedIndexChanged += (_, _) => RefreshReports();

        var btnGen = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Generate", "Tạo báo cáo"), 115, 30);
        btnGen.Location = new Point(808, 4);
        btnGen.Click += (_, _) => RefreshReports();
        topBar.Controls.AddRange(new Control[] { _btnTabR1, _btnTabR2, lblFrom, _dtFrom, lblTo, _dtTo, lblCat, _cboCategory, btnGen });

        // R1 container (Table + horizontal bar chart)
        _r1Container = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
        ((TableLayoutPanel)_r1Container).RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        ((TableLayoutPanel)_r1Container).RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

        var r1Card = SaigonBiteTheme.CreateCard(10);
        r1Card.Dock = DockStyle.Fill;
        r1Card.Padding = new Padding(16, 12, 16, 12);
        r1Card.Margin = new Padding(0, 0, 0, 12);
        _lblR1 = new Label
        {
            Text = LocalizationHelper.T("Revenue by category & payment method (01/01/2026 – 01/02/2026)", "Doanh thu theo danh mục & phương thức thanh toán (01/01/2026 – 01/02/2026)"),
            Font = SaigonBiteTheme.FontSubTitle,
            ForeColor = SaigonBiteTheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 28,
            UseMnemonic = false
        };
        _r1Grid = new DataGridView { Dock = DockStyle.Fill };
        SaigonBiteTheme.StyleDataGrid(_r1Grid);
        r1Card.Controls.Add(_r1Grid);
        r1Card.Controls.Add(_lblR1);

        var r1ChartCard = SaigonBiteTheme.CreateCard(10);
        r1ChartCard.Dock = DockStyle.Fill;
        r1ChartCard.Padding = new Padding(16, 10, 16, 10);
        var lblChart = new Label { Text = LocalizationHelper.T("Revenue by category (VND)", "Doanh thu theo danh mục (VNĐ)"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Dock = DockStyle.Top, Height = 26, UseMnemonic = false };
        _r1ChartCanvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        _r1ChartCanvas.Paint += R1ChartCanvas_Paint;
        r1ChartCard.Controls.Add(_r1ChartCanvas);
        r1ChartCard.Controls.Add(lblChart);

        ((TableLayoutPanel)_r1Container).Controls.Add(r1Card, 0, 0);
        ((TableLayoutPanel)_r1Container).Controls.Add(r1ChartCard, 0, 1);

        // R2 container (4 KPI cards + Hub utilization table matching finalized UI)
        _r2Container = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

        var kpiRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 88, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 12) };
        for (int i = 0; i < 4; i++) kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        (Panel card, Label val) MakeKpi(string title, Color valColor)
        {
            var card = SaigonBiteTheme.CreateCard(10);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 12, 0);
            card.Padding = new Padding(16, 10, 16, 10);
            var lblT = new Label { Text = title, Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(16, 12), AutoSize = true, UseMnemonic = false };
            var lblV = new Label { Text = "0", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = valColor, Location = new Point(16, 34), AutoSize = true, UseMnemonic = false };
            card.Controls.AddRange(new Control[] { lblT, lblV });
            return (card, lblV);
        }

        var (c1, v1) = MakeKpi(LocalizationHelper.T("Total orders handled", "Tổng đơn đã xử lý"), SaigonBiteTheme.AccentOrange);
        var (c2, v2) = MakeKpi(LocalizationHelper.T("Fastest avg prep", "Chuẩn bị nhanh nhất"), SaigonBiteTheme.TextPrimary);
        var (c3, v3) = MakeKpi(LocalizationHelper.T("Overloaded hubs", "Hub đang quá tải"), SaigonBiteTheme.DangerRed);
        var (c4, v4) = MakeKpi(LocalizationHelper.T("Quiet kitchen orders", "Số đơn giảm giá bếp vắng"), SaigonBiteTheme.SuccessGreen);
        c4.Margin = new Padding(0);
        _kpiOrdersVal = v1; _kpiPrepVal = v2; _kpiOverVal = v3; _kpiQuietVal = v4;
        kpiRow.Controls.Add(c1, 0, 0);
        kpiRow.Controls.Add(c2, 1, 0);
        kpiRow.Controls.Add(c3, 2, 0);
        kpiRow.Controls.Add(c4, 3, 0);

        var r2Card = SaigonBiteTheme.CreateCard(10);
        r2Card.Dock = DockStyle.Fill;
        r2Card.Padding = new Padding(16, 12, 16, 12);
        _lblR2 = new Label
        {
            Text = LocalizationHelper.T("Hub Utilization & Kitchen Operations (01/01/2026 – 01/02/2026)", "Hiệu suất Hub & Hoạt động bếp (01/01/2026 – 01/02/2026)"),
            Font = SaigonBiteTheme.FontSubTitle,
            ForeColor = SaigonBiteTheme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 32,
            UseMnemonic = false
        };
        _r2Grid = new DataGridView { Dock = DockStyle.Fill };
        SaigonBiteTheme.StyleDataGrid(_r2Grid);
        _r2Grid.Columns.Add("Hub", "Hub");
        _r2Grid.Columns.Add("Orders", LocalizationHelper.T("Orders handled", "Số đơn xử lý"));
        _r2Grid.Columns.Add("Prep", LocalizationHelper.T("Avg prep time", "Thời gian chuẩn bị TB"));
        _r2Grid.Columns.Add("Load", LocalizationHelper.T("Current load", "Tải hiện tại"));
        _r2Grid.Columns.Add("Quiet", LocalizationHelper.T("Quiet kitchen discounts", "Số đơn giảm giá bếp vắng"));
        _r2Grid.Columns["Hub"]!.Width = 170;
        _r2Grid.Columns["Orders"]!.Width = 140;
        _r2Grid.Columns["Prep"]!.Width = 200;
        _r2Grid.Columns["Load"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _r2Grid.Columns["Quiet"]!.Width = 230;
        r2Card.Controls.Add(_r2Grid);
        r2Card.Controls.Add(_lblR2);

        _r2Container.Controls.Add(r2Card);
        _r2Container.Controls.Add(kpiRow);

        _reportsPanel.Controls.Add(_r1Container);
        _reportsPanel.Controls.Add(_r2Container);
        _reportsPanel.Controls.Add(topBar);

        Controls.Add(_ordersPanel);
        Controls.Add(_reportsPanel);
        ShowSection(string.Equals(initialTab, "reports", StringComparison.OrdinalIgnoreCase));

        RefreshOrders();
        RefreshReports();
    }

    public void ShowSection(bool showReports)
    {
        _ordersPanel.Visible = !showReports;
        _reportsPanel.Visible = showReports;
        if (showReports) RefreshReports(); else RefreshOrders();
    }

    private void SwitchReport(bool showR1)
    {
        _r1Container.Visible = showR1;
        _r2Container.Visible = !showR1;
        _btnTabR1.BackColor = showR1 ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BgPillSoft;
        _btnTabR1.ForeColor = showR1 ? Color.White : SaigonBiteTheme.AccentOrange;
        _btnTabR2.BackColor = showR1 ? SaigonBiteTheme.BgPillSoft : SaigonBiteTheme.AccentOrange;
        _btnTabR2.ForeColor = showR1 ? SaigonBiteTheme.AccentOrange : Color.White;
    }

    public void RefreshOrders()
    {
        _ordersGrid.Rows.Clear();
        foreach (var o in _ctx.OrderRepo.GetAllOrders().OrderByDescending(x => x.OrderID))
        {
            string act = o.Status == OrderStatus.Confirmed ? LocalizationHelper.T("Cancel & refund", "Hủy & hoàn tiền") : (o.Status == OrderStatus.Cancelled ? LocalizationHelper.T("Refunded", "Đã hoàn tiền") : "—");
            int idx = _ordersGrid.Rows.Add($"#{o.OrderID}", _ctx.GetHubName(o.HubID), LocalizationHelper.Status(o.Status), $"{o.Pricing.RoundedTotal:N0}", act);
            _ordersGrid.Rows[idx].Tag = o.OrderID;
        }
    }

    private void OrdersGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _ordersGrid.Columns[e.ColumnIndex].Name != "Action") return;
        int oid = (int)_ordersGrid.Rows[e.RowIndex].Tag!;
        var o = _ctx.OrderRepo.GetOrderById(oid);
        if (o == null || o.Status != OrderStatus.Confirmed) return;

        try
        {
            _ctx.OrderService.TransitionState(oid, OrderStatus.Cancelled, _ctx.CurrentUser!, LocalizationHelper.T("Cancelled by System Admin (full refund)", "Quản trị viên hủy đơn (hoàn tiền 100%)"));
            RefreshOrders();
            RefreshReports();
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Admin action", "Thao tác quản trị"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public void RefreshReports()
    {
        // Report R1 via BLL ReportService with live Date Range & Category filter
        var fromDate = _dtFrom.Value.Date;
        var toDate = _dtTo.Value.Date.AddDays(1).AddSeconds(-1);
        string rangeLabel = $"{fromDate:dd/MM/yyyy} – {_dtTo.Value.Date:dd/MM/yyyy}";
        _lblR1.Text = LocalizationHelper.T($"Revenue by category & payment method ({rangeLabel})", $"Doanh thu theo danh mục & phương thức thanh toán ({rangeLabel})");
        _lblR2.Text = LocalizationHelper.T($"Hub Utilization & Kitchen Operations ({rangeLabel})", $"Hiệu suất Hub & Hoạt động bếp ({rangeLabel})");

        var rows = _ctx.ReportService.GenerateReportR1(fromDate, toDate);
        var methods = new[] { "MoMo", "VNPay", "Apple Pay", "PayPal", "COD" };
        var allCats = new[] { "Noodles", "Rice", "Bread", "Drinks" };
        var categories = _cboCategory.SelectedIndex > 0 ? new[] { allCats[_cboCategory.SelectedIndex - 1] } : allCats;

        _r1Grid.Columns.Clear();
        _r1Grid.Rows.Clear();
        _r1Grid.Columns.Add("Category", LocalizationHelper.T("Category", "Danh mục"));
        foreach (var m in methods) _r1Grid.Columns.Add(m, LocalizationHelper.PayMethod(m));
        _r1Grid.Columns.Add("Total", LocalizationHelper.T("Total (VND)", "Tổng (VNĐ)"));
        _r1Grid.Columns["Category"]!.Width = 150;
        _r1Grid.Columns["COD"]!.Width = 210;
        _r1Grid.Columns["Total"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        _r1CategoryTotals = new Dictionary<string, decimal>();
        decimal grandTotal = 0;
        var colTotals = methods.ToDictionary(m => m, _ => 0m);

        foreach (var cat in categories)
        {
            var vals = new List<object> { LocalizationHelper.Category(cat) };
            decimal rowSum = 0;
            foreach (var m in methods)
            {
                decimal amt = rows.Where(r => r.BrandCategory == cat && r.PaymentMethod.Equals(m, StringComparison.OrdinalIgnoreCase)).Sum(r => r.TotalRevenue);
                rowSum += amt;
                colTotals[m] += amt;
                vals.Add($"{amt:N0}");
            }
            vals.Add($"{rowSum:N0}");
            grandTotal += rowSum;
            _r1CategoryTotals[cat] = rowSum;
            _r1Grid.Rows.Add(vals.ToArray());
        }
        var totalRow = new List<object> { LocalizationHelper.T("TOTAL", "TỔNG CỘNG") };
        foreach (var m in methods) totalRow.Add($"{colTotals[m]:N0}");
        totalRow.Add($"{grandTotal:N0}");
        int tIdx = _r1Grid.Rows.Add(totalRow.ToArray());
        _r1Grid.Rows[tIdx].DefaultCellStyle.Font = SaigonBiteTheme.FontBodyBold;
        _r1Grid.Rows[tIdx].DefaultCellStyle.BackColor = SaigonBiteTheme.BgSelected;
        _r1Grid.Rows[tIdx].Cells["Total"].Style.ForeColor = SaigonBiteTheme.AccentOrange;
        _r1ChartCanvas.Invalidate();

        // Report R2 via BLL ReportService
        var r2 = _ctx.ReportService.GenerateReportR2(_ctx.Hubs);
        _r2Grid.Rows.Clear();
        int totalOrders = 0, overloaded = 0, quietTotal = 0;
        double minPrep = double.MaxValue;
        foreach (var h in r2)
        {
            totalOrders += h.OrdersHandled;
            if (h.CurrentLoadPct >= 90m) overloaded++;
            quietTotal += h.QuietKitchenCount;
            if (h.AvgPrepMinutes > 0 && h.AvgPrepMinutes < minPrep) minPrep = h.AvgPrepMinutes;

            var loadTxt = h.CurrentLoadPct >= 90
                ? $"{h.CurrentLoadPct:0}% - " + LocalizationHelper.T("overloaded", "bếp quá tải")
                : (h.CurrentLoadPct < 30 ? $"{h.CurrentLoadPct:0}% - " + LocalizationHelper.T("quiet kitchen", "bếp vắng") : $"{h.CurrentLoadPct:0}%");
            int rIdx = _r2Grid.Rows.Add(
                LocalizationHelper.HubName(h.HubName),
                h.OrdersHandled,
                $"{h.AvgPrepMinutes:0.0} " + LocalizationHelper.T("min", "phút"),
                loadTxt,
                h.QuietKitchenCount);
            if (h.CurrentLoadPct >= 90m)
            {
                _r2Grid.Rows[rIdx].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FEF2F2");
                _r2Grid.Rows[rIdx].Cells["Load"].Style.ForeColor = SaigonBiteTheme.DangerRed;
                _r2Grid.Rows[rIdx].Cells["Load"].Style.Font = SaigonBiteTheme.FontBodyBold;
            }
            else if (h.CurrentLoadPct < 30m)
            {
                _r2Grid.Rows[rIdx].Cells["Load"].Style.ForeColor = SaigonBiteTheme.SuccessGreen;
                _r2Grid.Rows[rIdx].Cells["Load"].Style.Font = SaigonBiteTheme.FontBodyBold;
            }
        }
        _kpiOrdersVal.Text = totalOrders.ToString();
        _kpiPrepVal.Text = (minPrep == double.MaxValue ? 0.0 : minPrep).ToString("0.0") + " " + LocalizationHelper.T("min", "phút");
        _kpiOverVal.Text = overloaded.ToString();
        _kpiQuietVal.Text = $"{quietTotal} " + LocalizationHelper.T("orders", "đơn");
    }

    private void R1ChartCanvas_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        if (_r1CategoryTotals.Count == 0) return;

        decimal maxVal = Math.Max(1m, _r1CategoryTotals.Values.Max());
        int y = 10;
        int maxBarW = Math.Max(160, _r1ChartCanvas.Width - 260);

        foreach (var kv in _r1CategoryTotals)
        {
            using var txtBrush = new SolidBrush(SaigonBiteTheme.TextPrimary);
            g.DrawString(LocalizationHelper.Category(kv.Key), SaigonBiteTheme.FontBody, txtBrush, 6, y + 2);

            var trackRect = new Rectangle(116, y + 3, maxBarW, 18);
            using var trackPath = SaigonBiteTheme.BuildRoundedRect(trackRect, 7);
            using var trackBrush = new SolidBrush(SaigonBiteTheme.BgHeaderRow);
            g.FillPath(trackBrush, trackPath);

            int barW = Math.Max(14, (int)(maxBarW * (double)(kv.Value / maxVal)));
            var rect = new Rectangle(116, y + 3, barW, 18);
            using var path = SaigonBiteTheme.BuildRoundedRect(rect, 7);
            using var barBrush = new SolidBrush(SaigonBiteTheme.AccentOrange);
            g.FillPath(barBrush, path);

            using var valBrush = new SolidBrush(SaigonBiteTheme.AccentOrange);
            g.DrawString(LocalizationHelper.IsVietnamese ? $"{kv.Value / 1_000_000m:0.##} triệu" : $"{kv.Value / 1_000_000m:0.##}M", SaigonBiteTheme.FontSmallBold, valBrush, 116 + maxBarW + 12, y + 3);
            y += 30;
        }
    }
}
