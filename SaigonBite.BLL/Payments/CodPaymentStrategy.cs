using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

public class CodPaymentStrategy : IPaymentStrategy
{
    public const decimal MaxCodLimitVnd = 1_000_000m;
    public string MethodName => "COD";
    public bool IsOnlinePayment => false;
    public bool IsAllowedFor(UserRole role) => role is UserRole.LocalCustomer or UserRole.ForeignTourist;

    public PaymentResult ProcessPayment(decimal roundedTotalVnd)
    {
        if (roundedTotalVnd > MaxCodLimitVnd)
            throw new InvalidOperationException("COD is only allowed when rounded total <= 1,000,000 VND (BR14).");
        return new(true, "COD - Pending Cash", $"COD-{Guid.NewGuid():N}"[..14], $"COD registered for {roundedTotalVnd:N0} VND.");
    }

    public PaymentResult Refund(decimal roundedTotalVnd) =>
        new(true, "COD - Cancelled (No Charge)", "COD-VOID", "No online refund required for COD.");
}
