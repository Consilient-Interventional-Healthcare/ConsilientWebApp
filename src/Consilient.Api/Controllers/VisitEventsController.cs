using Consilient.Common.Services;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Models;
using Consilient.Visits.Contracts.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("visit/{visitId:int}/event")]
    [ApiController]
    [Authorize]
    public class VisitEventsController(IVisitEventService _visitEventService, ICurrentUserService currentUserService) : ControllerBase
    {
        /// <summary>
        /// Creates a new visit event for the specified visit.
        /// </summary>
        /// <param name="visitId">The visit ID from the route</param>
        /// <param name="request">The visit event details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The ID of the created visit event</returns>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InsertVisitEvent(
            int visitId, 
            [FromBody] InsertVisitEventRequest request, 
            CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            if (visitId <= 0)
            {
                return BadRequest("Visit ID must be greater than zero.");
            }

            // Ensure the visitId from route matches or overrides the request
            request.VisitId = visitId;
            var userId = currentUserService.UserId;
            var visitEventId = await _visitEventService.InsertVisitEventAsync(request, userId, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetVisitEvents), new { visitId }, visitEventId);
        }

        /// <summary>
        /// Gets all events for the specified visit.
        /// </summary>
        /// <param name="visitId">The visit ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of visit events</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VisitEventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVisitEvents(int visitId, CancellationToken cancellationToken = default)
        {
            if (visitId <= 0)
            {
                return BadRequest("Visit ID must be greater than zero.");
            }

            var visitEvents = await _visitEventService.GetVisitEventsByVisitId(visitId, cancellationToken).ConfigureAwait(false);
            return Ok(visitEvents);
        }

        /// <summary>
        /// Deletes a specific visit event.
        /// </summary>
        /// <param name="visitId">The visit ID (for route consistency)</param>
        /// <param name="visitEventId">The visit event ID to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [HttpDelete("{visitEventId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteVisitEvent(
            int visitId, 
            int visitEventId, 
            CancellationToken cancellationToken = default)
        {
            if (visitId <= 0)
            {
                return BadRequest("Visit ID must be greater than zero.");
            }

            if (visitEventId <= 0)
            {
                return BadRequest("Visit Event ID must be greater than zero.");
            }

            var request = new DeleteVisitEventRequest { VisitEventId = visitEventId };
            var deleted = await _visitEventService.DeleteVisitEventAsync(request, cancellationToken).ConfigureAwait(false);
            return deleted > 0 ? NoContent() : NotFound();
        }

        /// <summary>
        /// Gets all available visit event types (not specific to a visit).
        /// </summary>
        [HttpGet("~/visit/event/types")]  // Results in: GET /visit/event/types
        [ProducesResponseType(typeof(IEnumerable<VisitEventTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVisitEventTypes(CancellationToken cancellationToken = default)
        {
            var eventTypes = await _visitEventService.GetVisitEventTypesAsync(cancellationToken).ConfigureAwait(false);
            return Ok(eventTypes);
        }
    }
}
