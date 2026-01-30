using Consilient.Billing.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class BillingCodesController(IBillingCodeService billingCodeService) : ControllerBase
{
    /// <summary>
    /// Gets all billing codes for lookup/dropdown purposes.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of billing codes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BillingCodeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var items = await billingCodeService.GetAllAsync(ct);
        return Ok(items);
    }
}


