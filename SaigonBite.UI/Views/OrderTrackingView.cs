using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SaigonBite.Models;

namespace SaigonBite.UI.Views;

/// <summary>Matches Figma Pages 9, 10, 23, 36, 37, 40: My Orders, 6-step status stepper, Price Breakdown, OrderStatusHistory, and Placed-only Edit/Cancel.</summary>
public class OrderTrackingView : UserControl
{
    private readonly DemoAppContext _ctx;
    private Order? _selectedOrder;

    private readonly DataGridView _ordersGrid;
    private readonly Label _lblOrderTitle;
    private readonly Panel _stepperCanvas;
    private readonly Label _lblBreakLines;
    private readonly Label _lblBreakDiscounts;
    private readonly Label _lblBreakDeliv;
    private readonly Label _lblBreakTotal;
    private readonly DataGridView _historyGrid;
    private readonly Button _btnEdit;
    private readonly Button _btnCancel;
    private readonly Label _lblActionHint;

    public OrderTrackingView(DemoAppContext ctx, Order? initialOrder = null)
    {
        _ctx = ctx;
        _selectedOrder = initialOrder;
        Dock = DockStyle.Fill;
        BackColor = SaigonBiteTheme.BgCanvas;
        Padding = new Padding(20, 14, 20, 16);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));

        // Left: My orders card
        var leftCard = SaigonBiteTheme.CreateCard(10);
        leftCard.Dock = DockStyle.Fill;
        leftCard.Padding = new Padding(16, 12, 16, 12);
        leftCard.Margin = new Padding(0, 0, 14, 0);

        var lblMy = new Label { Text = LocalizationHelper.T("My orders", "Đơn của tôi"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Dock = DockStyle.Top, Height = 30 };
        _ordersGrid = new DataGridView { Dock = DockStyle.Fill };
        SaigonBiteTheme.StyleDataGrid(_ordersGrid);
        _ordersGrid.Columns.Add("Order", LocalizationHelper.T("Order", "Đơn"));
        _ordersGrid.Columns.Add("Hub", "Hub");
        _ordersGrid.Columns.Add("Total", LocalizationHelper.T("Total", "Tổng"));
        _ordersGrid.Columns.Add("Status", LocalizationHelper.T("Status", "Trạng thái"));
        _ordersGrid.Columns["Order"]!.Width = 68;
        _ordersGrid.Columns["Hub"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _ordersGrid.Columns["Total"]!.Width = 82;
        _ordersGrid.Columns["Status"]!.Width = 98;
        _ordersGrid.SelectionChanged += (_, _) =>
        {
            if (_ordersGrid.SelectedRows.Count > 0 && _ordersGrid.SelectedRows[0].Tag is int oid)
            {
                _selectedOrder = _ctx.OrderRepo.GetOrderById(oid);
                RenderOrderDetail();
            }
        };
        leftCard.Controls.Add(_ordersGrid);
        leftCard.Controls.Add(lblMy);

        // Right column stack: Stepper + Price Breakdown + History + Actions
        var rightStack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 138));
        rightStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));

        // 1. Stepper Card
        var stepCard = SaigonBiteTheme.CreateCard(10);
        stepCard.Dock = DockStyle.Fill;
        stepCard.Padding = new Padding(16, 10, 16, 10);
        stepCard.Margin = new Padding(0, 0, 0, 10);
        _lblOrderTitle = new Label { Text = LocalizationHelper.T("Order — Status", "Đơn — Trạng thái"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Dock = DockStyle.Top, Height = 26 };
        _stepperCanvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        _stepperCanvas.Paint += StepperCanvas_Paint;
        stepCard.Controls.Add(_stepperCanvas);
        stepCard.Controls.Add(_lblOrderTitle);

        // 2. Price Breakdown Card
        var breakCard = SaigonBiteTheme.CreateCard(10);
        breakCard.Dock = DockStyle.Fill;
        breakCard.Padding = new Padding(16, 10, 16, 10);
        breakCard.Margin = new Padding(0, 0, 0, 10);
        var lblBreakHead = new Label { Text = LocalizationHelper.T("Price breakdown", "Chi tiết giá"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(16, 10), AutoSize = true };
        _lblBreakLines = new Label { Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(16, 38), Width = 680, AutoSize = false, Height = 20 };
        _lblBreakDiscounts = new Label { Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(16, 60), Width = 680, AutoSize = false, Height = 20 };
        _lblBreakDeliv = new Label { Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(16, 82), Width = 680, AutoSize = false, Height = 20 };
        _lblBreakTotal = new Label { Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.AccentOrange, Location = new Point(16, 104), Width = 680, AutoSize = false, Height = 22 };
        breakCard.Controls.AddRange(new Control[] { lblBreakHead, _lblBreakLines, _lblBreakDiscounts, _lblBreakDeliv, _lblBreakTotal });

        // 3. Status History Card
        var histCard = SaigonBiteTheme.CreateCard(10);
        histCard.Dock = DockStyle.Fill;
        histCard.Padding = new Padding(16, 10, 16, 10);
        histCard.Margin = new Padding(0, 0, 0, 10);
        var lblHistHead = new Label { Text = LocalizationHelper.T("Order history", "Lịch sử đơn hàng"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Dock = DockStyle.Top, Height = 28 };
        _historyGrid = new DataGridView { Dock = DockStyle.Fill };
        SaigonBiteTheme.StyleDataGrid(_historyGrid);
        _historyGrid.Columns.Add("From", LocalizationHelper.T("From", "Từ"));
        _historyGrid.Columns.Add("To", LocalizationHelper.T("To", "Đến"));
        _historyGrid.Columns.Add("Actor", LocalizationHelper.T("Actor", "Thực hiện"));
        _historyGrid.Columns.Add("Time", LocalizationHelper.T("Time", "Thời gian"));
        _historyGrid.Columns.Add("Reason", LocalizationHelper.T("Note", "Ghi chú"));
        _historyGrid.Columns["Reason"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        histCard.Controls.Add(_historyGrid);
        histCard.Controls.Add(lblHistHead);

        // 4. Action Row
        var actPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        _btnEdit = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Edit order", "Sửa đơn"), 195, 36);
        _btnEdit.Location = new Point(0, 4);
        _btnEdit.Click += (_, _) => OpenEditDialog();

        _btnCancel = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Cancel order", "Hủy đơn"), 125, 36);
        _btnCancel.Location = new Point(208, 4);
        _btnCancel.Click += (_, _) => CancelSelectedOrder();

        _lblActionHint = new Label { Font = SaigonBiteTheme.FontSmall, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(0, 44), Width = 680, AutoSize = false, Height = 20 };
        actPanel.Controls.AddRange(new Control[] { _btnEdit, _btnCancel, _lblActionHint });

        rightStack.Controls.Add(stepCard, 0, 0);
        rightStack.Controls.Add(breakCard, 0, 1);
        rightStack.Controls.Add(histCard, 0, 2);
        rightStack.Controls.Add(actPanel, 0, 3);

        layout.Controls.Add(leftCard, 0, 0);
        layout.Controls.Add(rightStack, 1, 0);
        Controls.Add(layout);

        LoadOrders();
    }

    public void LoadOrders()
    {
        var user = _ctx.CurrentUser;
        var list = user == null
            ? _ctx.OrderRepo.GetAllOrders()
            : _ctx.OrderRepo.GetOrdersByCustomer(user.UserID);
        // Show only the current customer's orders

        list = list.OrderByDescending(o => o.OrderID).ToList();
        _ordersGrid.Rows.Clear();
        int selectIdx = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var o = list[i];
            int idx = _ordersGrid.Rows.Add($"#{o.OrderID}", _ctx.GetHubName(o.HubID), $"{o.Pricing.RoundedTotal:N0}", LocalizationHelper.Status(o.Status));
            _ordersGrid.Rows[idx].Tag = o.OrderID;
            if (_selectedOrder != null && o.OrderID == _selectedOrder.OrderID)
                selectIdx = idx;
        }
        if (_ordersGrid.Rows.Count > 0)
        {
            _ordersGrid.Rows[selectIdx].Selected = true;
            _selectedOrder = _ctx.OrderRepo.GetOrderById((int)_ordersGrid.Rows[selectIdx].Tag!);
        }
        else
        {
            _selectedOrder = null;
        }
        RenderOrderDetail();
    }

    private void RenderOrderDetail()
    {
        if (_selectedOrder == null)
        {
            _lblOrderTitle.Text = LocalizationHelper.T("No orders yet", "Chưa có đơn hàng nào");
            _lblBreakLines.Text = LocalizationHelper.T("Place an order from the Order tab to track it here.", "Hãy đặt món ở mục Đặt hàng để theo dõi đơn tại đây.");
            _lblBreakDiscounts.Text = "";
            _lblBreakDeliv.Text = "";
            _lblBreakTotal.Text = "";
            _lblActionHint.Text = "";
            _historyGrid.Rows.Clear();
            _btnEdit.Enabled = false;
            _btnCancel.Enabled = false;
            _stepperCanvas.Invalidate();
            return;
        }
        var o = _selectedOrder;
        _lblOrderTitle.Text = LocalizationHelper.T($"Order #{o.OrderID} — Status ({LocalizationHelper.Status(o.Status)})", $"Đơn #{o.OrderID} — Trạng thái ({LocalizationHelper.Status(o.Status)})");
        _stepperCanvas.Invalidate();

        var itemsSummary = string.Join(" + ", o.Lines.Select(l => $"{l.Quantity}× {LocalizationHelper.DishName(l.ItemName)}"));
        _lblBreakLines.Text = $"{itemsSummary}   |   {LocalizationHelper.T("Items", "Giá món")}: {o.Pricing.SubtotalS:N0} {LocalizationHelper.Currency()}";
        _lblBreakDiscounts.Text = $"{LocalizationHelper.T("Quiet Kitchen", "Giảm bếp vắng")} −{o.Pricing.QuietKitchenQ:N0} · Voucher −{o.Pricing.VoucherV:N0}   |   {LocalizationHelper.T("Food total", "Tiền món sau giảm")}: {o.Pricing.FoodTotalF:N0} {LocalizationHelper.Currency()}";
        _lblBreakDeliv.Text = $"{LocalizationHelper.T("Shipping", "Phí vận chuyển")} {o.Pricing.DeliveryFeeD:N0} · {LocalizationHelper.T("Rush hour", "Giờ cao điểm")} {o.Pricing.RushSurchargeR:N0} · {LocalizationHelper.T("Rain", "Mưa")} {o.Pricing.RainSurchargeR2:N0}";
        _lblBreakTotal.Text = $"{LocalizationHelper.T("Total", "Tổng cộng")} ({LocalizationHelper.PayMethod(o.PaymentMethod)}):   {o.Pricing.RoundedTotal:N0} {LocalizationHelper.Currency()}";

        _historyGrid.Rows.Clear();
        foreach (var h in o.History)
        {
            _historyGrid.Rows.Add(
                h.FromState.HasValue ? LocalizationHelper.Status(h.FromState.Value) : "—",
                LocalizationHelper.Status(h.ToState),
                LocalizationHelper.RoleName(h.ActorRole),
                h.ChangedAtUtc7.ToString("dd/MM HH:mm"),
                (h.ToState == OrderStatus.Cancelled && h.ActorRole == UserRole.KitchenStaff && !string.IsNullOrWhiteSpace(h.Reason)) ? LocalizationHelper.T($"Cancelled because {h.Reason.Trim().ToLower()}", $"Bị hủy vì {h.Reason.Trim().ToLower()}") : "—");
        }

        bool canEdit = o.Status == OrderStatus.Placed;
        bool canCancel = o.Status is OrderStatus.Placed or OrderStatus.Confirmed;
        _btnEdit.Enabled = canEdit;
        _btnEdit.BackColor = canEdit ? SaigonBiteTheme.AccentOrange : ColorTranslator.FromHtml("#F3B59B");
        _btnCancel.Enabled = canCancel;

        _lblActionHint.Text = canCancel ? "" : LocalizationHelper.T("Cannot edit or cancel once the kitchen has started preparing.", "Không thể sửa hoặc hủy khi bếp đã bắt đầu chuẩn bị.");
    }

    private void StepperCanvas_Paint(object? sender, PaintEventArgs e)
    {
        if (_selectedOrder == null) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var steps = new[]
        {
            LocalizationHelper.Status(OrderStatus.Placed),
            LocalizationHelper.Status(OrderStatus.Confirmed),
            LocalizationHelper.Status(OrderStatus.Preparing),
            LocalizationHelper.Status(OrderStatus.Ready),
            LocalizationHelper.Status(OrderStatus.OutForDelivery),
            LocalizationHelper.Status(OrderStatus.Delivered)
        };
        int currentStep = _selectedOrder.Status switch
        {
            OrderStatus.Placed => 0,
            OrderStatus.Confirmed => 1,
            OrderStatus.Preparing => 2,
            OrderStatus.Ready => 3,
            OrderStatus.OutForDelivery => 4,
            OrderStatus.Delivered => 5,
            _ => 0
        };

        int w = Math.Max(400, _stepperCanvas.Width - 60);
        int stepW = w / (steps.Length - 1);
        int cy = 22;

        using var linePen = new Pen(SaigonBiteTheme.BorderSubtle, 2);
        g.DrawLine(linePen, 30, cy, 30 + stepW * (steps.Length - 1), cy);

        for (int i = 0; i < steps.Length; i++)
        {
            int cx = 30 + i * stepW;
            bool active = i <= currentStep;
            var circleRect = new Rectangle(cx - 13, cy - 13, 26, 26);
            using var fill = new SolidBrush(active ? SaigonBiteTheme.AccentOrange : Color.White);
            using var pen = new Pen(SaigonBiteTheme.AccentOrange, 1.5F);
            g.FillEllipse(fill, circleRect);
            g.DrawEllipse(pen, circleRect);

            var numStr = (i + 1).ToString();
            var sz = g.MeasureString(numStr, SaigonBiteTheme.FontSmallBold);
            using var numBrush = new SolidBrush(active ? Color.White : SaigonBiteTheme.AccentOrange);
            g.DrawString(numStr, SaigonBiteTheme.FontSmallBold, numBrush, cx - sz.Width / 2, cy - sz.Height / 2);

            var lblSz = g.MeasureString(steps[i], SaigonBiteTheme.FontSmall);
            using var lblBrush = new SolidBrush(active ? SaigonBiteTheme.TextPrimary : SaigonBiteTheme.TextMuted);
            g.DrawString(steps[i], active ? SaigonBiteTheme.FontSmallBold : SaigonBiteTheme.FontSmall, lblBrush, cx - lblSz.Width / 2, cy + 18);
        }
    }

    private void OpenEditDialog()
    {
        if (_selectedOrder == null || _selectedOrder.Status != OrderStatus.Placed) return;
        // Full BR17 edit modal matching finalized UI: adjust qty, remove lines, add items from same hub, live snapshot price preview
        var o = _selectedOrder;
        var voucher = string.IsNullOrEmpty(o.VoucherCode) ? null : _ctx.Vouchers.FirstOrDefault(v => v.VoucherCode == o.VoucherCode);
        var cloneLines = o.Lines.Select(l => new OrderLine
        {
            ItemID = l.ItemID, ItemName = l.ItemName, BrandCategory = l.BrandCategory, UnitPrice = l.UnitPrice, Quantity = l.Quantity
        }).ToList();

        var hubMenu = _ctx.MenuItems.Where(m => m.HubID == o.HubID && m.IsActive && m.Status == "Available").ToList();

        using var dlg = new Form
        {
            Text = LocalizationHelper.T($"Edit order #{o.OrderID} — Status: Placed", $"Sửa đơn #{o.OrderID} — Trạng thái: Đã đặt"),
            Size = new Size(800, 490),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = SaigonBiteTheme.BgCanvas,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, MinimizeBox = false
        };

        var leftCard = SaigonBiteTheme.CreateCard(12);
        leftCard.Location = new Point(18, 16);
        leftCard.Size = new Size(468, 418);

        var lblHead = new Label
        {
            Text = LocalizationHelper.T($"Edit order #{o.OrderID} — Status: Placed", $"Sửa đơn #{o.OrderID} — Trạng thái: Đã đặt"),
            Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(18, 14), AutoSize = true
        };
        var linesFlow = new FlowLayoutPanel
        {
            Location = new Point(14, 48), Size = new Size(440, 294), AutoScroll = true,
            FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.Transparent
        };

        var cboAdd = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = SaigonBiteTheme.FontBody, Location = new Point(18, 362), Width = 284 };
        foreach (var m in hubMenu)
            cboAdd.Items.Add($"{LocalizationHelper.DishName(m.ItemName)} ({m.UnitPrice:N0} {LocalizationHelper.Currency()})");
        if (cboAdd.Items.Count > 0) cboAdd.SelectedIndex = 0;

        var btnAdd = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Add item", "Đặt thêm món"), 136, 32);
        btnAdd.Location = new Point(312, 360);

        leftCard.Controls.AddRange(new Control[] { lblHead, linesFlow, cboAdd, btnAdd });

        var rightCard = SaigonBiteTheme.CreateCard(12);
        rightCard.Location = new Point(500, 16);
        rightCard.Size = new Size(266, 418);

        var lblTotHead = new Label { Text = LocalizationHelper.T("Total", "Tổng tiền"), Font = SaigonBiteTheme.FontSubTitle, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(18, 14), AutoSize = true };
        var lblValSub = new Label { Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(130, 56), Size = new Size(118, 22), TextAlign = ContentAlignment.MiddleRight };
        var lblValDisc = new Label { Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.SuccessGreen, Location = new Point(130, 88), Size = new Size(118, 22), TextAlign = ContentAlignment.MiddleRight };
        var lblValShip = new Label { Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(130, 120), Size = new Size(118, 22), TextAlign = ContentAlignment.MiddleRight };

        rightCard.Controls.Add(new Label { Text = LocalizationHelper.T("Items total", "Giá món"), Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(18, 56), AutoSize = true });
        rightCard.Controls.Add(new Label { Text = LocalizationHelper.T("Discounts", "Giảm giá"), Font = SaigonBiteTheme.FontBody, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(18, 88), AutoSize = true });
        rightCard.Controls.Add(new Label { Text = LocalizationHelper.T("Shipping + fees", "Phí vận chuyển + phụ phí"), Font = SaigonBiteTheme.FontSmall, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(18, 122), AutoSize = true });
        rightCard.Controls.AddRange(new Control[] { lblTotHead, lblValSub, lblValDisc, lblValShip });

        var totBox = SaigonBiteTheme.CreateCard(10, SaigonBiteTheme.BgSelected, ColorTranslator.FromHtml("#F3C4AE"));
        totBox.Location = new Point(18, 160);
        totBox.Size = new Size(230, 50);
        totBox.Controls.Add(new Label { Text = LocalizationHelper.T("Total", "Tổng cộng"), Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(12, 14), AutoSize = true });
        var lblValTot = new Label { Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = SaigonBiteTheme.AccentOrange, Location = new Point(98, 12), Size = new Size(122, 26), TextAlign = ContentAlignment.MiddleRight };
        totBox.Controls.Add(lblValTot);
        rightCard.Controls.Add(totBox);

        var lblErr = new Label { Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.DangerRed, Location = new Point(18, 222), Size = new Size(230, 56) };
        var btnSave = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Save changes", "Lưu thay đổi"), 230, 38);
        btnSave.Location = new Point(18, 294);
        var btnDiscard = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Discard changes", "Hủy thay đổi"), 230, 36);
        btnDiscard.Location = new Point(18, 344);
        btnDiscard.Click += (_, _) => dlg.DialogResult = DialogResult.Cancel;
        rightCard.Controls.AddRange(new Control[] { lblErr, btnSave, btnDiscard });

        void RecalcPreview()
        {
            lblErr.Text = "";
            if (cloneLines.Count == 0)
            {
                lblValSub.Text = lblValDisc.Text = lblValShip.Text = "0";
                lblValTot.Text = $"0 {LocalizationHelper.Currency()}";
                lblErr.Text = LocalizationHelper.T("Order must contain at least 1 item.", "Đơn hàng phải có ít nhất 1 món.");
                btnSave.Enabled = false;
                return;
            }
            try
            {
                var b = _ctx.Pricing.CalculatePrice(cloneLines, o.Snapshot, voucher);
                lblValSub.Text = $"{b.SubtotalS:N0}";
                decimal disc = b.QuietKitchenQ + b.VoucherV;
                lblValDisc.Text = disc > 0 ? $"−{disc:N0}" : "0";
                lblValShip.Text = $"{b.DeliveryFeeD + b.RushSurchargeR + b.RainSurchargeR2:N0}";
                lblValTot.Text = $"{b.RoundedTotal:N0} {LocalizationHelper.Currency()}";
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                lblErr.Text = LocalizationHelper.FriendlyError(ex.Message);
                lblValTot.Text = "—";
                btnSave.Enabled = false;
            }
        }

        void RenderLines()
        {
            linesFlow.Controls.Clear();
            foreach (var line in cloneLines.ToList())
            {
                var card = SaigonBiteTheme.CreateCard(10, Color.White, SaigonBiteTheme.AccentOrange);
                card.Size = new Size(414, 72);
                card.Margin = new Padding(0, 0, 0, 8);

                var lName = new Label { Text = LocalizationHelper.DishName(line.ItemName), Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(12, 10), Size = new Size(220, 22) };
                var lPrice = new Label { Text = $"{line.UnitPrice:N0} {LocalizationHelper.Currency()}", Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.AccentOrange, Location = new Point(12, 38), AutoSize = true };

                var btnDel = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Remove", "Xóa"), 64, 24);
                btnDel.Location = new Point(338, 8);
                btnDel.Click += (_, _) => { cloneLines.Remove(line); RenderLines(); RecalcPreview(); };

                var btnM = SaigonBiteTheme.CreateOutlineButton("−", 28, 26);
                btnM.Location = new Point(308, 38);
                btnM.Click += (_, _) => { if (line.Quantity > 1) { line.Quantity--; RenderLines(); RecalcPreview(); } };

                var lQty = new Label { Text = line.Quantity.ToString(), Font = SaigonBiteTheme.FontBodyBold, ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(340, 41), Size = new Size(28, 22), TextAlign = ContentAlignment.MiddleCenter };

                var btnP = SaigonBiteTheme.CreatePrimaryButton("+", 28, 26);
                btnP.Location = new Point(374, 38);
                btnP.Click += (_, _) => { if (line.Quantity < 20) { line.Quantity++; RenderLines(); RecalcPreview(); } };

                card.Controls.AddRange(new Control[] { lName, lPrice, btnDel, btnM, lQty, btnP });
                linesFlow.Controls.Add(card);
            }
        }

        btnAdd.Click += (_, _) =>
        {
            if (cboAdd.SelectedIndex < 0 || cboAdd.SelectedIndex >= hubMenu.Count) return;
            var m = hubMenu[cboAdd.SelectedIndex];
            var existing = cloneLines.FirstOrDefault(l => l.ItemID == m.ItemID);
            if (existing != null)
            {
                if (existing.Quantity < 20) existing.Quantity++;
            }
            else if (cloneLines.Count < 30)
            {
                cloneLines.Add(new OrderLine { ItemID = m.ItemID, ItemName = m.ItemName, BrandCategory = m.Category == "Noodle Soups" ? "Noodles" : m.Category, UnitPrice = m.UnitPrice, Quantity = 1 });
            }
            RenderLines();
            RecalcPreview();
        };

        btnSave.Click += (_, _) =>
        {
            try
            {
                _ctx.OrderService.EditPlacedOrder(o.OrderID, cloneLines, voucher);
                dlg.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Edit error", "Lỗi chỉnh sửa"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        dlg.Controls.AddRange(new Control[] { leftCard, rightCard });
        RenderLines();
        RecalcPreview();
        if (dlg.ShowDialog(this) == DialogResult.OK) LoadOrders();
    }

    private void CancelSelectedOrder()
    {
        if (_selectedOrder == null) return;
        var o = _selectedOrder;
        var res = MessageBox.Show(
            LocalizationHelper.T($"Cancel order #{o.OrderID}?\nPayment: {LocalizationHelper.PayMethod(o.PaymentMethod)} ({o.Pricing.RoundedTotal:N0} VND).", $"Hủy đơn #{o.OrderID}?\nThanh toán: {LocalizationHelper.PayMethod(o.PaymentMethod)} ({o.Pricing.RoundedTotal:N0} VNĐ)."),
            LocalizationHelper.T("Cancel order", "Hủy đơn"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (res != DialogResult.Yes) return;

        try
        {
            _ctx.OrderService.TransitionState(o.OrderID, OrderStatus.Cancelled, _ctx.CurrentUser!, LocalizationHelper.T("Cancelled by customer", "Khách hàng hủy đơn"));
            LoadOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationHelper.FriendlyError(ex.Message), LocalizationHelper.T("Invalid state transition", "Chuyển trạng thái không hợp lệ"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
