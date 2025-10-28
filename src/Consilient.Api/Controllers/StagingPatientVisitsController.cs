using Consilient.Api.Infra;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("patients/visits/staging")]
    [ApiController]
    public class StagingPatientVisitsController(IStagingPatientVisitService stagingPatientVisitService) : ControllerBase
    {
        private readonly IStagingPatientVisitService _stagingPatientVisitService = stagingPatientVisitService;

        [HttpGet("by-date/{date}")]
        public async Task<IActionResult> GetByDate([ModelBinder(BinderType = typeof(YyyyMmDdDateModelBinder))] DateTime date)
        {
            var results = await _stagingPatientVisitService.GetByDateAsync(date).ConfigureAwait(false);
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await _stagingPatientVisitService.GetByIdAsync(id).ConfigureAwait(false);
            return visit is null ? NotFound() : Ok(visit);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStagingPatientVisitRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var created = await _stagingPatientVisitService.CreateAsync(request).ConfigureAwait(false);
            if (created is null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(GetById), new { id = created.PatientVisitStagingId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStagingPatientVisitRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var updated = await _stagingPatientVisitService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _stagingPatientVisitService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }
    }
}
