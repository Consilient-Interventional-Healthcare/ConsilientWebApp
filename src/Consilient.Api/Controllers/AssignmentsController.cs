using Consilient.Api.Configuration;
using Consilient.Api.Helpers;
using Consilient.Background.Workers;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AssignmentsController(
        IBackgroundJobClient backgroundJobClient,
        ApplicationSettings applicationSettings) : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
        private readonly FileUploadSettings _fileUploadSettings = applicationSettings.FileUpload;



        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(
            IFormFile file,
            [FromForm] DateOnly serviceDate,
            [FromForm] int facilityId,
            CancellationToken cancellationToken = default)
        {
            // Validate file
            var fileUploaderHelper = new FileUploadHelper(_fileUploadSettings);
            var validationResult = fileUploaderHelper.ValidateFile(file);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            // Save the file
            var filePath = await fileUploaderHelper.SaveFileAsync(file, cancellationToken).ConfigureAwait(false);

            //Queue the background job
            var jobId = _backgroundJobClient.Enqueue<ImportClinicalDataSheetWorker>(
                worker => worker.Import(filePath, serviceDate, facilityId, null!));

            var result = new
            {
                JobId = jobId,
                file.FileName,
                ServiceDate = serviceDate,
                FacilityId = facilityId,
                Message = "File uploaded successfully and queued for processing."
            };

            return Accepted(result);
        }
    }
}
