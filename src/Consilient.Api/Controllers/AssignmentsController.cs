using Consilient.Api.Configuration;
using Consilient.Api.Helpers;
using Consilient.Background.Workers.DoctorAssignments;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AssignmentsController(
        DoctorAssignmentsImportWorkerEnqueuer importWorkerEnqueuer,
        ApplicationSettings applicationSettings) : ControllerBase
    {
        private readonly DoctorAssignmentsImportWorkerEnqueuer _importWorkerEnqueuer = importWorkerEnqueuer;
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

            // Queue the import job (which will automatically chain the resolution job)
            _importWorkerEnqueuer.Import(filePath, facilityId, serviceDate);

            var result = new
            {
                file.FileName,
                ServiceDate = serviceDate,
                FacilityId = facilityId,
                Message = "File uploaded successfully and queued for processing."
            };

            return Accepted(result);
        }
    }
}
