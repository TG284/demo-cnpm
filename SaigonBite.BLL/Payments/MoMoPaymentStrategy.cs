using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

public class MoMoPaymentStrategy : IPaymentStrategy
{
    public string MethodName => "MoMo";
    public bool IsOnlinePayment => true;
    public bool IsAllowedFor(UserRole role) => role == UserRole.LocalCustomer;
    public PaymentResult ProcessPayment(decimal roundedTotalVnd) =>
        new(true, "MoMo - Paid", $"MOMO-{Guid.NewGuid():N}"[..14], $"Paid {roundedTotalVnd:N0} VND via MoMo.");
    public PaymentResult Refund(decimal roundedTotalVnd) =>
        new(true, "MoMo - Simulated Refund", $"REF-MOMO-{Guid.NewGuid():N}"[..16], $"Full simulated refund of {roundedTotalVnd:N0} VND.");
}
