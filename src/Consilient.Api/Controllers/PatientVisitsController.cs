using Consilient.Api.Infra;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("patients/visits")]
    [ApiController]
    public class PatientVisitsController(IPatientVisitService patientVisitService) : ControllerBase
    {
        private readonly IPatientVisitService _patientVisitService = patientVisitService;

        // GET patients/visits/by-date/{yyyyMMdd}
        [HttpGet("by-date/{date}")]
        public async Task<IActionResult> GetByDate([ModelBinder(BinderType = typeof(YyyyMmDdDateModelBinder))] DateTime date)
        {
            var results = await _patientVisitService.GetByDateAsync(date).ConfigureAwait(false);
            return Ok(results);
        }

        // GET patients/visits/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await _patientVisitService.GetByIdAsync(id).ConfigureAwait(false);
            return visit is null ? NotFound() : Ok(visit);
        }

        // POST patients/visits
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePatientVisitRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var created = await _patientVisitService.CreateAsync(request).ConfigureAwait(false);
            if (created is null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(GetById), new { id = created.PatientVisitId }, created);
        }

        // PUT patients/visits/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientVisitRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var updated = await _patientVisitService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }

        // DELETE patients/visits/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _patientVisitService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }
    }
}
