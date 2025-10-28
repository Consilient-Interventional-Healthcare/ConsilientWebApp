using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FacilitiesController(IFacilityService facilityService) : ControllerBase
    {
        private readonly IFacilityService _facilityService = facilityService ?? throw new ArgumentNullException(nameof(facilityService));

        [HttpPost]
        public async Task<IActionResult> CreateFacility([FromBody] CreateFacilityRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var created = await _facilityService.CreateAsync(request);
            return CreatedAtAction(nameof(GetFacilityById), new { id = created.FacilityId }, created);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            var deleted = await _facilityService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetFacilities()
        {
            var facilities = await _facilityService.GetAllAsync();
            return Ok(facilities);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFacilityById(int id)
        {
            var facility = await _facilityService.GetByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            return Ok(facility);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFacility(int id, [FromBody] UpdateFacilityRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            try
            {
                var updated = await _facilityService.UpdateAsync(id, request);
                if (updated == null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }
    }
}