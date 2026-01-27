using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class FacilitiesController(IFacilityService facilityService) : ControllerBase
{
    private readonly IFacilityService _facilityService = facilityService ?? throw new ArgumentNullException(nameof(facilityService));

    [HttpPost]
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFacilityRequest request)
    {
        var created = await _facilityService.CreateAsync(request).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _facilityService.DeleteAsync(id).ConfigureAwait(false);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FacilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var facilities = await _facilityService.GetAllAsync().ConfigureAwait(false);
        return Ok(facilities);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var facility = await _facilityService.GetByIdAsync(id).ConfigureAwait(false);
        return facility == null ? NotFound() : Ok(facility);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFacilityRequest request)
    {
        var updated = await _facilityService.UpdateAsync(id, request).ConfigureAwait(false);
        return updated == null ? NotFound() : Ok(updated);
    }
}