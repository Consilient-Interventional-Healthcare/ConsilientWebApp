namespace Consilient.Billing.Contracts;

public interface IBillingCodeService
{
    Task<IEnumerable<BillingCodeDto>> GetAllAsync(CancellationToken ct = default);
}

