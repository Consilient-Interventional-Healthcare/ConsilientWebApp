using Consilient.Api.Infra.ModelBinders;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Models;
using Consilient.Visits.Contracts.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{

    [Route("visits")]
    [ApiController]
    public class VisitsController(IVisitService visitService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(VisitDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateVisitRequest request)
        {
            var created = await visitService.CreateAsync(request).ConfigureAwait(false);
            return created is null
                ? BadRequest()
                : CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await visitService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("date/{date}/facility/{facilityId}")]
        [ProducesResponseType(typeof(IEnumerable<VisitDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDate(
            [ModelBinder(BinderType = typeof(YyyyMmDdDateModelBinder))] DateOnly date,
            int facilityId)
        {
            var results = await visitService.GetByDateAndFacilityIdAsync(date, facilityId).ConfigureAwait(false);
            return Ok(results);
        }

        [HttpGet("provider/{id:int}")]
        [ProducesResponseType(typeof(IEnumerable<VisitDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByProvider(int id)
        {
            var visit = await visitService.GetByProviderAsync(id).ConfigureAwait(false);
            return Ok(visit);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VisitDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await visitService.GetByIdAsync(id).ConfigureAwait(false);
            return visit is null ? NotFound() : Ok(visit);
        }
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(VisitDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVisitRequest request)
        {
            var updated = await visitService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }
    }
}