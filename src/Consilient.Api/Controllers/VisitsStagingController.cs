using Consilient.Api.Infra.ModelBinders;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{

    [Route("visits/staging")]
    [ApiController]
    public class VisitsStagingController(IVisitStagingService visitsStagingService) : ControllerBase
    {

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetByDate(
            [ModelBinder(BinderType = typeof(YyyyMmDdDateModelBinder))] DateOnly date)
        {
            var results = await visitsStagingService.GetByDateAsync(date).ConfigureAwait(false);
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var visit = await visitsStagingService.GetByIdAsync(id).ConfigureAwait(false);
            return visit is null ? NotFound() : Ok(visit);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVisitStagingRequest request)
        {
            var created = await visitsStagingService.CreateAsync(request).ConfigureAwait(false);
            if (created is null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVisitStagingRequest request)
        {
            var updated = await visitsStagingService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await visitsStagingService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("push-approved")]
        public async Task<IActionResult> PushApprovedVisits()
        {
            var pushedCount = await visitsStagingService.PushApprovedVisitsAsync().ConfigureAwait(false);
            return Ok(pushedCount);
        }

        [HttpPost("upload-assignment")]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAssignment(IFormFile file)
        {
            if (file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using var uploadStream = new MemoryStream();
            await file.CopyToAsync(uploadStream).ConfigureAwait(false);
            uploadStream.Position = 0;
            var result = await visitsStagingService.UploadAssignmentAsync(uploadStream).ConfigureAwait(false);
            return Ok(result);
        }
    }
}