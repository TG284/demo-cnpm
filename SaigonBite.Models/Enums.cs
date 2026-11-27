namespace SaigonBite.Models;

public enum UserRole { LocalCustomer, ForeignTourist, KitchenStaff, Shipper, Admin }

public enum OrderStatus
{
    Placed, Confirmed, Preparing, Ready, OutForDelivery, Delivered, Cancelled, Failed
}

public enum VoucherType { Percent, Fixed }
