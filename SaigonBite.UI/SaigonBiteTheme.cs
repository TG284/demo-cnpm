using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SaigonBite.UI;

/// <summary>Warm Saigon culinary design tokens and custom GDI+ WinForms controls.</summary>
public static class SaigonBiteTheme
{
    public static readonly Color BgCanvas      = ColorTranslator.FromHtml("#F6F2ED");
    public static readonly Color BgCard        = Color.White;
    public static readonly Color BgSelected    = ColorTranslator.FromHtml("#FFF1EA");
    public static readonly Color BgPillSoft    = ColorTranslator.FromHtml("#FCE3D6");
    public static readonly Color BgHeaderRow   = ColorTranslator.FromHtml("#FAF7F4");
    public static readonly Color AccentOrange  = ColorTranslator.FromHtml("#D8430A");
    public static readonly Color AccentHover   = ColorTranslator.FromHtml("#B53607");
    public static readonly Color TextPrimary   = ColorTranslator.FromHtml("#1C1917");
    public static readonly Color TextMuted     = ColorTranslator.FromHtml("#57534E");
    public static readonly Color BorderSubtle  = ColorTranslator.FromHtml("#E7DFD7");
    public static readonly Color DangerRed     = ColorTranslator.FromHtml("#DC2626");
    public static readonly Color DangerSoft    = ColorTranslator.FromHtml("#FEE2E2");
    public static readonly Color SuccessGreen  = ColorTranslator.FromHtml("#15803D");
    public static readonly Color SuccessSoft   = ColorTranslator.FromHtml("#DCFCE7");
    public static readonly Color AmberWarn     = ColorTranslator.FromHtml("#B45309");
    public static readonly Color AmberSoft     = ColorTranslator.FromHtml("#FEF3C7");

    public static readonly Font FontBrand      = new("Segoe UI", 14F, FontStyle.Bold);
    public static readonly Font FontHeroTitle  = new("Segoe UI", 18F, FontStyle.Bold);
    public static readonly Font FontTitle      = new("Segoe UI", 13F, FontStyle.Bold);
    public static readonly Font FontSubTitle   = new("Segoe UI", 10.5F, FontStyle.Bold);
    public static readonly Font FontBody       = new("Segoe UI", 9.5F, FontStyle.Regular);
    public static readonly Font FontBodyBold   = new("Segoe UI", 9.5F, FontStyle.Bold);
    public static readonly Font FontSmall      = new("Segoe UI", 8.5F, FontStyle.Regular);
    public static readonly Font FontSmallBold  = new("Segoe UI", 8.5F, FontStyle.Bold);

    public static RoundedPanel CreateCard(int radius = 12, Color? fill = null, Color? border = null)
    {
        return new RoundedPanel
        {
            CornerRadius = radius,
            FillColor = fill ?? BgCard,
            BorderColor = border ?? BorderSubtle,
            BorderThickness = 1,
            BackColor = Color.Transparent
        };
    }

    public static Button CreatePrimaryButton(string text, int width = 160, int height = 38)
    {
        return new RoundedButton
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = AccentOrange,
            ForeColor = Color.White,
            Font = FontBodyBold,
            CornerRadius = 8,
            BorderSize = 0,
            Cursor = Cursors.Hand
        };
    }

    public static Button CreateOutlineButton(string text, int width = 120, int height = 36)
    {
        return new RoundedButton
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = Color.White,
            ForeColor = AccentOrange,
            Font = FontBodyBold,
            CornerRadius = 8,
            BorderSize = 1,
            BorderColor = AccentOrange,
            Cursor = Cursors.Hand
        };
    }

    public static Button CreatePillButton(string text, bool active, int width = 82, int height = 28)
    {
        return new RoundedButton
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = active ? AccentOrange : BgPillSoft,
            ForeColor = active ? Color.White : AccentOrange,
            Font = FontSmallBold,
            CornerRadius = height / 2,
            BorderSize = 0,
            Cursor = Cursors.Hand
        };
    }

    public static Label CreateStatusPill(string text, Color bg, Color fg)
    {
        return new RoundedPillLabel
        {
            Text = "  " + text + "  ",
            AutoSize = true,
            PillBackColor = bg,
            ForeColor = fg,
            Font = FontSmallBold,
            Padding = new Padding(6, 3, 6, 3)
        };
    }

    public static void StyleDataGrid(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = BorderSubtle;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.ColumnHeadersHeight = 36;
        grid.RowTemplate.Height = 38;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = BgHeaderRow,
            ForeColor = TextMuted,
            Font = FontSmallBold,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 8, 0)
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = TextPrimary,
            Font = FontBody,
            SelectionBackColor = BgSelected,
            SelectionForeColor = TextPrimary,
            Padding = new Padding(8, 0, 8, 0)
        };
    }

    public static GraphicsPath BuildRoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void DrawCoverImage(Graphics g, Image img, Rectangle dest)
    {
        float imgRatio = (float)img.Width / img.Height;
        float destRatio = (float)dest.Width / Math.Max(1, dest.Height);
        Rectangle src;
        if (imgRatio > destRatio)
        {
            int newW = (int)(img.Height * destRatio);
            int x = (img.Width - newW) / 2;
            src = new Rectangle(x, 0, newW, img.Height);
        }
        else
        {
            int newH = (int)(img.Width / destRatio);
            int y = (img.Height - newH) / 2;
            src = new Rectangle(0, y, img.Width, newH);
        }
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
    }
}

public class RoundedPanel : Panel
{
    public int CornerRadius { get; set; } = 12;
    public Color FillColor { get; set; } = Color.White;
    public Color BorderColor { get; set; } = SaigonBiteTheme.BorderSubtle;
    public int BorderThickness { get; set; } = 1;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3));
        using var path = SaigonBiteTheme.BuildRoundedRect(rect, CornerRadius);
        using var brush = new SolidBrush(FillColor);
        using var pen = new Pen(BorderColor, BorderThickness);
        e.Graphics.FillPath(brush, path);
        if (BorderThickness > 0)
            e.Graphics.DrawPath(pen, path);
    }
}

/// <summary>Draws a cover-cropped culinary hero photograph with a dark readability scrim and rounded corners.</summary>
public class HeroBannerPanel : Panel
{
    public Image? BackgroundPhoto { get; set; }
    public int CornerRadius { get; set; } = 14;
    public bool HorizontalScrim { get; set; } = true;

    public HeroBannerPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var clipPath = SaigonBiteTheme.BuildRoundedRect(rect, CornerRadius);
        e.Graphics.SetClip(clipPath);

        if (BackgroundPhoto != null)
            SaigonBiteTheme.DrawCoverImage(e.Graphics, BackgroundPhoto, rect);
        else
            e.Graphics.Clear(ColorTranslator.FromHtml("#292524"));

        using var grad = HorizontalScrim
            ? new LinearGradientBrush(rect, Color.FromArgb(145, 28, 25, 23), Color.FromArgb(30, 28, 25, 23), LinearGradientMode.Horizontal)
            : new LinearGradientBrush(rect, Color.FromArgb(45, 15, 23, 42), Color.FromArgb(75, 15, 23, 42), LinearGradientMode.Vertical);
        e.Graphics.FillRectangle(grad, rect);
        e.Graphics.ResetClip();
    }
}

/// <summary>Color-coded rounded kitchen load progress bar.</summary>
public class LoadBarPanel : Panel
{
    public decimal Percent { get; set; } = 22m;

    public LoadBarPanel()
    {
        Height = 7;
        DoubleBuffered = true;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var track = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var trackBrush = new SolidBrush(SaigonBiteTheme.BorderSubtle);
        e.Graphics.FillRectangle(trackBrush, track);

        int fillW = (int)(track.Width * Math.Clamp((double)Percent / 100.0, 0.0, 1.0));
        if (fillW > 0)
        {
            Color c = Percent >= 90m ? SaigonBiteTheme.DangerRed : (Percent < 30m ? SaigonBiteTheme.SuccessGreen : SaigonBiteTheme.AmberWarn);
            using var fillBrush = new SolidBrush(c);
            e.Graphics.FillRectangle(fillBrush, new Rectangle(0, 0, fillW, track.Height));
        }
    }
}

/// <summary>Anti-aliased rounded button matching Figma/PDF UI design.</summary>
public class RoundedButton : Button
{
    public int CornerRadius { get; set; } = 8;
    public int BorderSize { get; set; } = 0;
    public Color BorderColor { get; set; } = Color.Transparent;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        DoubleBuffered = true;
        UseMnemonic = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Color parentBg = Parent?.BackColor ?? Color.White;
        if (parentBg == Color.Transparent) parentBg = SaigonBiteTheme.BgCanvas;
        e.Graphics.Clear(parentBg);

        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        int r = Math.Min(CornerRadius, Math.Min(rect.Width, rect.Height) / 2);
        using var path = SaigonBiteTheme.BuildRoundedRect(rect, r);
        Color fill = Enabled ? BackColor : ColorTranslator.FromHtml("#E7E5E4");
        using var brush = new SolidBrush(fill);
        e.Graphics.FillPath(brush, path);

        if (BorderSize > 0)
        {
            using var pen = new Pen(Enabled ? BorderColor : SaigonBiteTheme.BorderSubtle, BorderSize);
            e.Graphics.DrawPath(pen, path);
        }

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            Enabled ? ForeColor : SaigonBiteTheme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }
}

/// <summary>Anti-aliased rounded pill label for status badges and role chips.</summary>
public class RoundedPillLabel : Label
{
    public Color PillBackColor { get; set; } = SaigonBiteTheme.BgHeaderRow;

    public RoundedPillLabel()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        UseMnemonic = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        int r = Math.Min(12, rect.Height / 2);
        using var path = SaigonBiteTheme.BuildRoundedRect(rect, r);
        using var brush = new SolidBrush(PillBackColor);
        e.Graphics.FillPath(brush, path);
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}
