using SaigonBite.Models;

namespace SaigonBite.BLL.Payments;

public class PaymentFactory
{
    private readonly Dictionary<string, IPaymentStrategy> _strategies;

    public PaymentFactory()
    {
        var list = new IPaymentStrategy[]
        {
            new MoMoPaymentStrategy(),
            new VNPayPaymentStrategy(),
            new ApplePayPaymentStrategy(),
            new PayPalPaymentStrategy(),
            new CodPaymentStrategy(),
            new ZaloPayPaymentStrategy()
        };
        _strategies = list.ToDictionary(s => s.MethodName, StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentStrategy GetStrategy(string methodName, UserRole role)
    {
        if (!_strategies.TryGetValue(methodName, out var strategy))
            throw new ArgumentException($"Unsupported payment method: {methodName} (BR13).");
        if (!strategy.IsAllowedFor(role))
            throw new UnauthorizedAccessException($"Payment method '{methodName}' is not allowed for {role} (BR13).");
        return strategy;
    }

    public IReadOnlyList<string> GetAllowedMethodsFor(UserRole role) =>
        _strategies.Values.Where(s => s.IsAllowedFor(role)).Select(s => s.MethodName).ToList();
}
