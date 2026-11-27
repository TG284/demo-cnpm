using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

public class ApplePayPaymentStrategy : IPaymentStrategy
{
    public string MethodName => "Apple Pay";
    public bool IsOnlinePayment => true;
    public bool IsAllowedFor(UserRole role) => role == UserRole.ForeignTourist;
    public PaymentResult ProcessPayment(decimal roundedTotalVnd) =>
        new(true, "Apple Pay - Paid", $"APPL-{Guid.NewGuid():N}"[..14], $"Paid {roundedTotalVnd:N0} VND via Apple Pay.");
    public PaymentResult Refund(decimal roundedTotalVnd) =>
        new(true, "Apple Pay - Simulated Refund", $"REF-APPL-{Guid.NewGuid():N}"[..16], $"Full simulated refund of {roundedTotalVnd:N0} VND.");
}
