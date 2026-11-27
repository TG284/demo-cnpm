using System.Text.RegularExpressions;
using SaigonBite.Models;

namespace SaigonBite.UI;

/// <summary>Runtime English / Vietnamese bilingual switcher for WinForms UI (defaults to Vietnamese).</summary>
public static class LocalizationHelper
{
    public static bool IsVietnamese { get; set; } = true;

    public static event Action? LanguageChanged;

    public static void SetLanguage(bool vietnamese)
    {
        if (IsVietnamese == vietnamese) return;
        IsVietnamese = vietnamese;
        LanguageChanged?.Invoke();
    }

    public static string T(string en, string vi) => IsVietnamese ? vi : en;

    public static string Currency() => IsVietnamese ? "VNĐ" : "VND";

    public static string PayMethod(string method) => method switch
    {
        "COD"      => T("COD", "Thanh toán khi nhận hàng"),
        "ApplePay" => "Apple Pay",
        _          => method
    };

    public static string RoleName(UserRole role) => role switch
    {
        UserRole.LocalCustomer  => T("Local Customer", "Khách địa phương"),
        UserRole.ForeignTourist => T("Foreign Tourist", "Khách nước ngoài"),
        UserRole.KitchenStaff   => T("Kitchen Staff", "Nhân viên bếp"),
        UserRole.Shipper        => "Shipper",
        UserRole.Admin          => T("System Admin", "Quản trị viên"),
        _                       => role.ToString()
    };

    public static string Status(OrderStatus status) => status switch
    {
        OrderStatus.Placed         => T("Placed", "Đã đặt"),
        OrderStatus.Confirmed      => T("Confirmed", "Đã xác nhận"),
        OrderStatus.Preparing      => T("Preparing", "Đang chuẩn bị"),
        OrderStatus.Ready          => T("Ready", "Sẵn sàng"),
        OrderStatus.OutForDelivery => T("Out for Delivery", "Đang giao"),
        OrderStatus.Delivered      => T("Delivered", "Đã giao"),
        OrderStatus.Cancelled      => T("Cancelled", "Đã hủy"),
        OrderStatus.Failed         => T("Failed", "Thất bại"),
        _                          => status.ToString()
    };

    public static string HubName(string raw) => raw switch
    {
        "District 1" or "Quận 1"     => T("District 1", "Quận 1"),
        "Binh Thanh" or "Bình Thạnh" => T("Binh Thanh", "Bình Thạnh"),
        "Thu Duc"    or "Thủ Đức"    => T("Thu Duc", "Thủ Đức"),
        "District 7" or "Quận 7"     => T("District 7", "Quận 7"),
        "Go Vap"     or "Gò Vấp"     => T("Go Vap", "Gò Vấp"),
        _                            => raw
    };

    public static string Category(string raw) => raw switch
    {
        "All" or "Tất cả"                         => T("All", "Tất cả"),
        "Noodles" or "Noodle Soups" or "Món nước" => T("Noodle Soups", "Món nước"),
        "Rice" or "Cơm"                           => T("Rice", "Cơm"),
        "Bread" or "Bánh mì"                      => T("Bread", "Bánh mì"),
        "Drinks" or "Đồ uống"                     => T("Drinks", "Đồ uống"),
        _                                         => raw
    };

    public static string DishName(string raw) => raw switch
    {
        "Phở tái" or "Pho Bo Tai"                  => T("Rare Beef Pho", "Phở tái"),
        "Bánh Mì Thịt" or "Banh Mi Thit"           => T("Saigon Banh Mi", "Bánh Mì Thịt"),
        "Bún Chả Hà Nội" or "Bun Cha Ha Noi"       => T("Hanoi Bun Cha", "Bún Chả Hà Nội"),
        "Cơm sườn" or "Com Tam Suon"               => T("Grilled Pork Chop Rice", "Cơm sườn"),
        "Trà Đào Cam Sả" or "Tra Dao Cam Sa"       => T("Peach Lemongrass Tea", "Trà Đào Cam Sả"),
        "Bun Bo Hue" or "Bún Bò Huế"               => T("Hue Spicy Beef Noodle", "Bún Bò Huế"),
        "Com Ga Xoi Mo" or "Cơm Gà Xối Mỡ"         => T("Crispy Chicken Rice", "Cơm Gà Xối Mỡ"),
        "Banh Mi Chao" or "Bánh Mì Chảo"           => T("Sizzling Pan Banh Mi", "Bánh Mì Chảo"),
        "Ca Phe Sua Da" or "Cà Phê Sữa Đá"         => T("Iced Milk Coffee", "Cà Phê Sữa Đá"),
        "Hu Tieu Nam Vang" or "Hủ Tiếu Nam Vang"   => T("Nam Vang Noodle Soup", "Hủ Tiếu Nam Vang"),
        "Com Chien Duong Chau" or "Cơm Chiên Dương Châu" => T("Yangzhou Fried Rice", "Cơm Chiên Dương Châu"),
        "Banh Mi Bo Kho" or "Bánh Mì Bò Kho"       => T("Beef Stew Banh Mi", "Bánh Mì Bò Kho"),
        "Tra Sua Tran Chau" or "Trà Sữa Trân Châu" => T("Pearl Milk Tea", "Trà Sữa Trân Châu"),
        "Mi Quang Tom Thit" or "Mì Quảng Tôm Thịt" => T("Quang Noodle Shrimp & Pork", "Mì Quảng Tôm Thịt"),
        "Com Suon Bi Cha" or "Cơm Sườn Bì Chả"     => T("Broken Rice Combo", "Cơm Sườn Bì Chả"),
        "Banh Mi Ga Xe" or "Bánh Mì Gà Xé"         => T("Shredded Chicken Banh Mi", "Bánh Mì Gà Xé"),
        "Nuoc Mia Tac" or "Nước Mía Tắc"           => T("Kumquat Sugarcane Juice", "Nước Mía Tắc"),
        _                                          => raw
    };

    public static string Reason(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "—";
        var r = raw.Trim();
        return r switch
        {
            "By customer" or "Cancelled by customer" or "Khách hàng hủy đơn"
                => T("Cancelled by customer", "Khách hàng hủy đơn"),
            "Confirmed by Thu Duc kitchen" or "Bếp Thủ Đức đã xác nhận"
                => T("Confirmed by Thu Duc kitchen", "Bếp Thủ Đức đã xác nhận"),
            "Self-claimed Ready order" or "Claimed ready order" or "Nhận đơn sẵn sàng" or "Shipper đã nhận đơn sẵn sàng"
                => T("Shipper claimed ready order", "Shipper đã nhận đơn sẵn sàng"),
            "Cancelled by System Admin (full refund)" or "Quản trị viên hủy đơn (hoàn tiền 100%)"
                => T("Cancelled by System Admin (full refund)", "Quản trị viên hủy đơn (hoàn tiền 100%)"),
            "Ingredient out of stock" or "Hết nguyên liệu"
                => T("Ingredient out of stock", "Hết nguyên liệu"),
            "Delivery failed" or "Giao thất bại"
                => T("Delivery failed", "Giao thất bại"),
            "Delivered" or "Đã giao"
                => T("Delivered", "Đã giao"),
            "Order placed" or "Khách hàng đặt đơn"
                => T("Order placed", "Khách hàng đặt đơn"),
            _ => r
        };
    }

    public static string ActorName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "—";
        return raw.Trim() switch
        {
            "Nguyen Van A" or "Nguyễn Văn A" => T("Nguyen Van A", "Nguyễn Văn A"),
            "Kitchen Staff · Thu Duc"        => T("Kitchen Staff · Thu Duc", "Nhân viên bếp · Thủ Đức"),
            "Shipper · Thu Duc"              => T("Shipper · Thu Duc", "Shipper · Thủ Đức"),
            "System Admin"                   => T("System Admin", "Quản trị viên"),
            _                                => raw
        };
    }

    public static string Address(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "—";
        return raw.Trim() switch
        {
            "12 Vo Van Ngan, Thu Duc"  or "12 Võ Văn Ngân, Thủ Đức"  => T("12 Vo Van Ngan, Thu Duc", "12 Võ Văn Ngân, Thủ Đức"),
            "45 Le Van Viet, Thu Duc"  or "45 Lê Văn Việt, Thủ Đức"  => T("45 Le Van Viet, Thu Duc", "45 Lê Văn Việt, Thủ Đức"),
            "8 Pham Van Dong, Thu Duc" or "8 Phạm Văn Đồng, Thủ Đức" => T("8 Pham Van Dong, Thu Duc", "8 Phạm Văn Đồng, Thủ Đức"),
            "25 Le Loi, District 1"    or "25 Lê Lợi, Quận 1"        => T("25 Le Loi, District 1", "25 Lê Lợi, Quận 1"),
            "12 Vo Van Ngan, HCMC"     or "12 Võ Văn Ngân, TP.HCM"   => T("12 Vo Van Ngan, HCMC", "12 Võ Văn Ngân, TP.HCM"),
            _                                                        => raw
        };
    }

    public static string FriendlyError(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return T("An unexpected error occurred.", "Đã xảy ra lỗi không xác định.");
        string msg = raw;
        if (msg.Contains("at least 50,000", StringComparison.OrdinalIgnoreCase))
            return T("Order subtotal must be at least 50,000 VND.", "Giá món tối thiểu phải từ 50,000 VNĐ.");
        if (msg.Contains("Delivery distance", StringComparison.OrdinalIgnoreCase))
            return T("Delivery distance must be greater than 0 and at most 8.0 km.", "Khoảng cách giao hàng phải lớn hơn 0 và tối đa 8.0 km.");
        if (msg.Contains("90% load", StringComparison.OrdinalIgnoreCase))
            return T("Selected hub is currently overloaded. Please choose another hub.", "Hub đang quá tải. Vui lòng chọn hub khác.");
        if (msg.Contains("1 and 30", StringComparison.OrdinalIgnoreCase))
            return T("Order must contain between 1 and 30 line items.", "Đơn hàng phải có từ 1 đến 30 dòng món.");
        if (msg.Contains("1 and 20", StringComparison.OrdinalIgnoreCase))
            return T("Each item quantity must be between 1 and 20.", "Số lượng mỗi món phải từ 1 đến 20.");
        if (msg.Contains("1,000,000", StringComparison.OrdinalIgnoreCase))
            return T("Cash on Delivery is only allowed for orders of 1,000,000 VND or less.", "Thanh toán khi nhận hàng chỉ áp dụng cho đơn từ 1,000,000 VNĐ trở xuống.");
        if (msg.Contains("cash confirmation", StringComparison.OrdinalIgnoreCase))
            return T("Cash collection confirmation is required before marking Delivered.", "Cần xác nhận đã thu tiền trước khi đánh dấu Đã giao.");
        if (msg.Contains("only be edited", StringComparison.OrdinalIgnoreCase))
            return T("Orders can only be edited while in Placed status.", "Chỉ có thể chỉnh sửa đơn hàng khi đang ở trạng thái Đã đặt.");
        if (msg.Contains("Terminal state", StringComparison.OrdinalIgnoreCase))
            return T("This order is already in a final state and cannot be updated.", "Đơn hàng đã ở trạng thái kết thúc, không thể thay đổi.");
        if (msg.Contains("Transition", StringComparison.OrdinalIgnoreCase) && msg.Contains("not allowed", StringComparison.OrdinalIgnoreCase))
            return T("This status change is not allowed for your current role.", "Thao tác chuyển trạng thái này không được phép với vai trò hiện tại.");
        if (msg.Contains("Unsupported payment method", StringComparison.OrdinalIgnoreCase))
            return T("Unsupported payment method.", "Phương thức thanh toán không được hỗ trợ.");
        if (msg.Contains("Payment method", StringComparison.OrdinalIgnoreCase) && msg.Contains("not allowed", StringComparison.OrdinalIgnoreCase))
            return T("This payment method is not available for your account type.", "Phương thức thanh toán này không áp dụng cho loại tài khoản của bạn.");
        if (msg.Contains("Password must be at least 8", StringComparison.OrdinalIgnoreCase))
            return T("Password must be at least 8 characters.", "Mật khẩu phải từ 8 ký tự trở lên.");
        if (msg.Contains("Order not found", StringComparison.OrdinalIgnoreCase))
            return T("Order not found.", "Không tìm thấy đơn hàng.");
        if (msg.Contains("reason", StringComparison.OrdinalIgnoreCase))
            return T("Please enter a rejection reason.", "Vui lòng nhập lý do từ chối đơn hàng.");

        return Regex.Replace(msg, @"\s*\((?:BR|FR|NFR)\d+\)", "", RegexOptions.IgnoreCase).Trim();
    }
}
