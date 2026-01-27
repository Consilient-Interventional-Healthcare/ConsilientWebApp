using Consilient.Hospitalizations.Contracts;
using Consilient.Hospitalizations.Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class HospitalizationsController(IHospitalizationService hospitalizationsService) : ControllerBase
{
    [HttpGet("statuses")]
    [ProducesResponseType(typeof(IEnumerable<HospitalizationStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHospitalizationStatuses()
    {
        var statuses = await hospitalizationsService.GetHospitalizationStatuses().ConfigureAwait(false);
        return Ok(statuses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HospitalizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var hospitalization = await hospitalizationsService.GetHospitalizationById(id).ConfigureAwait(false);
        return hospitalization is null ? NotFound() : Ok(hospitalization);
    }

}
