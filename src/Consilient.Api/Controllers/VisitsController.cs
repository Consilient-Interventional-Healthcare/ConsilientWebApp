using Consilient.Api.Infra.ModelBinders;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{

    [Route("visits")]
    [ApiController]
    public class VisitsController(IVisitService visitService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVisitRequest request)
        {
            var created = await visitService.CreateAsync(request).ConfigureAwait(false);
            return created is null
                ? BadRequest()
                : CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await visitService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetByDate(
            [ModelBinder(BinderType = typeof(YyyyMmDdDateModelBinder))] DateOnly date)
        {
            var results = await visitService.GetByDateAsync(date).ConfigureAwait(false);
            return Ok(results);
        }

        [HttpGet("employee/{id:int}")]
        public async Task<IActionResult> GetByEmployee(int id)
        {
            var visit = await visitService.GetByEmployeeAsync(id).ConfigureAwait(false);
            return Ok(visit);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await visitService.GetByIdAsync(id).ConfigureAwait(false);
            return visit is null ? NotFound() : Ok(visit);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVisitRequest request)
        {
            var updated = await visitService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }
    }
}