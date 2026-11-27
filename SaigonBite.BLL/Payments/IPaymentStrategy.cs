using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

public record PaymentResult(bool Success, string PaymentStatus, string ReferenceCode, string Message);

public interface IPaymentStrategy
{
    string MethodName { get; }
    bool IsOnlinePayment { get; }
    bool IsAllowedFor(UserRole role);
    PaymentResult ProcessPayment(decimal roundedTotalVnd);
    PaymentResult Refund(decimal roundedTotalVnd);
}
