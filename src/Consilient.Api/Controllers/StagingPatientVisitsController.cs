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

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetByDate(
            [ModelBinder(BinderType = typeof(YyyyMmDdDateModelBinder))] DateOnly date)
        {
            var results = await stagingPatientVisitService.GetByDateAsync(date).ConfigureAwait(false);
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await stagingPatientVisitService.GetByIdAsync(id).ConfigureAwait(false);
            return visit is null ? NotFound() : Ok(visit);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStagingPatientVisitRequest request)
        {
            var created = await stagingPatientVisitService.CreateAsync(request).ConfigureAwait(false);
            if (created is null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(GetById), new { id = created.PatientVisitStagingId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStagingPatientVisitRequest request)
        {
            var updated = await stagingPatientVisitService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await stagingPatientVisitService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("push-approved")]
        public async Task<IActionResult> PushApprovedPatientVisits()
        {
            var pushedCount = await stagingPatientVisitService.PushApprovedPatientVisitsAsync().ConfigureAwait(false);
            return Ok(pushedCount);
        }

        [HttpPost("upload-spreadsheet")]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSpreadsheet(IFormFile file)
        {
            if (file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using var uploadStream = new MemoryStream();
            await file.CopyToAsync(uploadStream).ConfigureAwait(false);
            uploadStream.Position = 0;
            var result = await stagingPatientVisitService.UploadSpreadsheetAsync(uploadStream).ConfigureAwait(false);
            return Ok(result);
        }
    }
}