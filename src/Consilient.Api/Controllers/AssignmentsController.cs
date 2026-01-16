using Consilient.Api.Configuration;
using Consilient.Api.Helpers;
using Consilient.Background.Workers.ProviderAssignments;
using Consilient.Common.Services;
using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AssignmentsController(
        ProviderAssignmentsImportWorkerEnqueuer importWorkerEnqueuer,
        ApplicationSettings applicationSettings,
        ICurrentUserService currentUserService,
        IFileStorage fileStorage) : ControllerBase
    {
        private readonly ProviderAssignmentsImportWorkerEnqueuer _importWorkerEnqueuer = importWorkerEnqueuer;
        private readonly FileUploadSettings _fileUploadSettings = applicationSettings.ProviderAssignmentsUploads;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IFileStorage _fileStorage = fileStorage;



        [HttpPost("upload")]
        [ProducesResponseType<FileUploadResult>(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(
            IFormFile file,
            [FromForm] DateOnly serviceDate,
            [FromForm] int facilityId,
            CancellationToken cancellationToken = default)
        {
            // Validate file
            var fileValidator = new FileValidator(_fileUploadSettings);
            var validationResult = fileValidator.ValidateFile(file);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            // Save the file and get a reference
            await using var stream = file.OpenReadStream();
            var fileReference = await _fileStorage.SaveAsync(file.FileName, stream, cancellationToken).ConfigureAwait(false);

            // Queue the import job (which will automatically chain the resolution job)
            var result = _importWorkerEnqueuer.Import(fileReference, facilityId, serviceDate, _currentUserService.UserId);

            return Accepted(result);
        }
    }
}
