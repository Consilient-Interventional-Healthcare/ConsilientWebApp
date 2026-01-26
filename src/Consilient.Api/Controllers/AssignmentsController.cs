using Consilient.Api.Configuration;
using Consilient.Api.Helpers;
using Consilient.Background.Workers.ProviderAssignments;
using Consilient.Common.Contracts;
using Consilient.Infrastructure.Storage;
using Consilient.Infrastructure.Storage.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AssignmentsController(
        ProviderAssignmentsImportWorkerEnqueuer importWorkerEnqueuer,
        IOptions<ProviderAssignmentsUploadsOptions> fileUploadOptions,
        ICurrentUserService currentUserService,
        IFileStorage fileStorage) : ControllerBase
    {
        private readonly ProviderAssignmentsImportWorkerEnqueuer _importWorkerEnqueuer = importWorkerEnqueuer;
        private readonly ProviderAssignmentsUploadsOptions _fileUploadOptions = fileUploadOptions.Value;
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
            var fileValidator = new FileValidator(_fileUploadOptions);
            var validationResult = fileValidator.ValidateFile(file);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            var batchId = Guid.NewGuid();
            // Save the file and get a reference
            var filename = PathHelper.GenerateFileReference(batchId.ToString(), file.FileName);
            await using var stream = file.OpenReadStream();
            var fileReference = await _fileStorage.SaveAsync(filename, stream, cancellationToken).ConfigureAwait(false);

            // Queue the import job (which will automatically chain the resolution job)
            var result = _importWorkerEnqueuer.Import(batchId, fileReference, facilityId, serviceDate, _currentUserService.UserId);

            return Accepted(result);
        }
    }
}
