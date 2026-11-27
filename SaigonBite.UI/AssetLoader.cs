using System.Drawing;

namespace SaigonBite.UI;

/// <summary>Safe cached loader for SaigonBite logo, hero, 17 unique dish photos, and 6 payment app logos.</summary>
public static class AssetLoader
{
    private static readonly Dictionary<string, Image?> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static Image? Load(string fileName)
    {
        if (Cache.TryGetValue(fileName, out var cached))
            return cached;

        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", fileName),
            Path.Combine(Environment.CurrentDirectory, "Assets", fileName),
            Path.Combine(Environment.CurrentDirectory, "SaigonBite.UI", "Assets", fileName)
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                try
                {
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var temp = Image.FromStream(fs);
                    var copy = new Bitmap(temp);
                    Cache[fileName] = copy;
                    return copy;
                }
                catch { break; }
            }
        }
        Cache[fileName] = null;
        return null;
    }

    public static Image? GetLogoImage() => Load("losg_nobg.png");
    public static Image? GetHeroLogoImage() => Load("losg_nobg_white.png");
    public static Image? GetAuthHeroImage() => Load("hero_chobachieu.jpg");
    public static Image? GetHeroImage() => Load("hero_saigon.jpg");

    public static Image? GetPaymentLogo(string method) => (method ?? string.Empty).Replace(" ", "").ToLowerInvariant() switch
    {
        "momo"     => Load("pay_momo.png"),
        "vnpay"    => Load("pay_vnpay.png"),
        "zalopay"  => Load("pay_zalopay.png"),
        "applepay" => Load("pay_applepay.png"),
        "paypal"   => Load("pay_paypal.png"),
        _          => Load("pay_cod.png")
    };

    public static Image? GetDishImage(string itemName)
    {
        var lower = (itemName ?? string.Empty).ToLowerInvariant();
        if (lower.Contains("pho") || lower.Contains("phở")) return Load("dish_pho.jpg");
        if (lower.Contains("bun bo")) return Load("dish_bunbohue.jpg");
        if (lower.Contains("bun cha") || lower.Contains("bún chả")) return Load("dish_buncha.jpg");
        if (lower.Contains("hu tieu")) return Load("dish_hutieu.jpg");
        if (lower.Contains("mi quang")) return Load("dish_miquang.jpg");
        if (lower.Contains("com ga")) return Load("dish_comga.jpg");
        if (lower.Contains("com chien")) return Load("dish_comchien.jpg");
        if (lower.Contains("suon bi cha")) return Load("dish_comsuonbicha.jpg");
        if (lower.Contains("com tam") || lower.Contains("cơm sườn") || lower.Contains("com suon")) return Load("dish_comtam.jpg");
        if (lower.Contains("banh mi chao")) return Load("dish_banhmichao.jpg");
        if (lower.Contains("bo kho")) return Load("dish_bokho.jpg");
        if (lower.Contains("ga xe")) return Load("dish_banhmiga.jpg");
        if (lower.Contains("banh mi") || lower.Contains("bánh mì")) return Load("dish_banhmi.jpg");
        if (lower.Contains("ca phe")) return Load("dish_caphesuada.jpg");
        if (lower.Contains("tra sua")) return Load("dish_trasua.jpg");
        if (lower.Contains("nuoc mia")) return Load("dish_nuocmia.jpg");
        if (lower.Contains("tra dao") || lower.Contains("trà đào")) return Load("dish_tradao.jpg");
        return Load("dish_pho.jpg");
    }
}
