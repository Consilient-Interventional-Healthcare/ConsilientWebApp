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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePatientVisitRequest request)
        {
            var created = await patientVisitService.CreateAsync(request).ConfigureAwait(false);
            return created is null
                ? BadRequest()
                : CreatedAtAction(nameof(GetById), new { id = created.PatientVisitId }, created);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await patientVisitService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetByDate(
            [ModelBinder(BinderType = typeof(YyyyMmDdDateModelBinder))] DateOnly date)
        {
            var results = await patientVisitService.GetByDateAsync(date).ConfigureAwait(false);
            return Ok(results);
        }

        [HttpGet("employee/{id:int}")]
        public async Task<IActionResult> GetByEmployee(int id)
        {
            var visit = await patientVisitService.GetByEmployeeAsync(id).ConfigureAwait(false);
            return Ok(visit);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await patientVisitService.GetByIdAsync(id).ConfigureAwait(false);
            return visit is null ? NotFound() : Ok(visit);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientVisitRequest request)
        {
            var updated = await patientVisitService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }
    }
}