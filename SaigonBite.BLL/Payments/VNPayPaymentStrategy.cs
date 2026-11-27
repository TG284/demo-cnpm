using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

public class VNPayPaymentStrategy : IPaymentStrategy
{
    public string MethodName => "VNPay";
    public bool IsOnlinePayment => true;
    public bool IsAllowedFor(UserRole role) => role == UserRole.LocalCustomer;
    public PaymentResult ProcessPayment(decimal roundedTotalVnd) =>
        new(true, "VNPay - Paid", $"VNP-{Guid.NewGuid():N}"[..14], $"Paid {roundedTotalVnd:N0} VND via VNPay.");
    public PaymentResult Refund(decimal roundedTotalVnd) =>
        new(true, "VNPay - Simulated Refund", $"REF-VNP-{Guid.NewGuid():N}"[..16], $"Full simulated refund of {roundedTotalVnd:N0} VND.");
}
