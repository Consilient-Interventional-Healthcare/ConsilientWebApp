using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers;

[Route("visit/{visitId:int}/servicebilling")]
[ApiController]
[Authorize]
public class VisitServiceBillingsController(IVisitServiceBillingService service) : ControllerBase
{
    /// <summary>
    /// Creates a new service billing record for the specified visit.
    /// </summary>
    /// <param name="visitId">The visit ID from the route</param>
    /// <param name="request">The service billing details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The ID of the created service billing record</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        int visitId,
        [FromBody] CreateVisitServiceBillingRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (visitId <= 0)
        {
            return BadRequest("Visit ID must be greater than zero.");
        }

        // Ensure the visitId from route matches or overrides the request
        request.VisitId = visitId;
        var id = await service.CreateAsync(request, ct);
        return Created($"/visit/{visitId}/servicebilling/{id}", id);
    }

    /// <summary>
    /// Deletes a specific service billing record.
    /// </summary>
    /// <param name="visitId">The visit ID (for route consistency)</param>
    /// <param name="id">The service billing record ID to delete</param>
    /// <param name="ct">Cancellation token</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(
        int visitId,
        int id,
        CancellationToken ct = default)
    {
        if (visitId <= 0)
        {
            return BadRequest("Visit ID must be greater than zero.");
        }

        if (id <= 0)
        {
            return BadRequest("Service Billing ID must be greater than zero.");
        }

        var request = new DeleteVisitServiceBillingRequest { VisitServiceBillingId = id };
        var deleted = await service.DeleteAsync(request, ct);
        return deleted > 0 ? NoContent() : NotFound();
    }
}
