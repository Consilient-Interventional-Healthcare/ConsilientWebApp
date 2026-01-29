using Consilient.Api.Configuration;
using Consilient.Api.Helpers;
using Consilient.Common.Contracts;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Consilient.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AssignmentsController(
    IProviderAssignmentsService providerAssignmentService,
    IOptions<ProviderAssignmentsUploadsOptions> fileUploadOptions,
    ICurrentUserService currentUserService) : ControllerBase
{
    private readonly ProviderAssignmentsUploadsOptions _fileUploadOptions = fileUploadOptions.Value;

    [HttpPost("upload")]
    [ProducesResponseType<ImportProviderAssignmentResult>(StatusCodes.Status202Accepted)]
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

        // Create request and let service handle batch creation, file saving, and job enqueuing
        var request = new ImportProviderAssignmentRequest
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            FacilityId = facilityId,
            ServiceDate = serviceDate,
            EnqueuedByUserId = currentUserService.UserId
        };

        var result = await providerAssignmentService.ImportAsync(request, cancellationToken);

        // Return appropriate status based on success flag
        if (result.Success)
        {
            return Accepted(result);
        }

        return BadRequest(result.Message);
    }

    [HttpGet("batch-statuses")]
    [ProducesResponseType<IEnumerable<ProviderAssignmentBatchStatusDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatchStatuses(CancellationToken cancellationToken = default)
    {
        var statuses = await providerAssignmentService.GetBatchStatusesAsync(cancellationToken);
        return Ok(statuses);
    }

    [HttpPost("process/{batchId:guid}")]
    [ProducesResponseType<string>(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ProcessBatch(Guid batchId, CancellationToken cancellationToken = default)
    {
        var jobId = await providerAssignmentService.ProcessAsync(batchId, cancellationToken);
        return Accepted(jobId);
    }
}
