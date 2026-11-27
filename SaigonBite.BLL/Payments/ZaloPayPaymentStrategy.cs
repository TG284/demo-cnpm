using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

/// <summary>Week 5 Change Request: added without modifying any existing IPaymentStrategy class (OCP/NFR2).</summary>
public class ZaloPayPaymentStrategy : IPaymentStrategy
{
    public string MethodName => "ZaloPay";
    public bool IsOnlinePayment => true;
    public bool IsAllowedFor(UserRole role) => role == UserRole.LocalCustomer;
    public PaymentResult ProcessPayment(decimal roundedTotalVnd) =>
        new(true, "ZaloPay - Paid", $"ZLP-{Guid.NewGuid():N}"[..14], $"Paid {roundedTotalVnd:N0} VND via ZaloPay.");
    public PaymentResult Refund(decimal roundedTotalVnd) =>
        new(true, "ZaloPay - Simulated Refund", $"REF-ZLP-{Guid.NewGuid():N}"[..16], $"Full simulated refund of {roundedTotalVnd:N0} VND.");
}
