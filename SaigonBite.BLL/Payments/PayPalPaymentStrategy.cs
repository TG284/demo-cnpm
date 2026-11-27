using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

public class PayPalPaymentStrategy : IPaymentStrategy
{
    public string MethodName => "PayPal";
    public bool IsOnlinePayment => true;
    public bool IsAllowedFor(UserRole role) => role == UserRole.ForeignTourist;
    public PaymentResult ProcessPayment(decimal roundedTotalVnd) =>
        new(true, "PayPal - Paid", $"PP-{Guid.NewGuid():N}"[..14], $"Paid {roundedTotalVnd:N0} VND via PayPal.");
    public PaymentResult Refund(decimal roundedTotalVnd) =>
        new(true, "PayPal - Simulated Refund", $"REF-PP-{Guid.NewGuid():N}"[..16], $"Full simulated refund of {roundedTotalVnd:N0} VND.");
}
