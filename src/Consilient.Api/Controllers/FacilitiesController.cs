using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FacilitiesController(IFacilityService facilityService) : ControllerBase
    {
        private readonly IFacilityService _facilityService = facilityService;

        [HttpGet]
        public async Task<IActionResult> GetFacilities()
        {
            var facilities = await _facilityService.GetAllAsync();
            return Ok(facilities);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFacilityById(int id)
        {
            var facility = await _facilityService.GetById(id);
            if (facility == null)
            {
                return NotFound();
            }

            return Ok(facility);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFacility([FromBody] CreateFacilityRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _facilityService.CreateAsync(request);
                return CreatedAtAction(nameof(GetFacilityById), new { id = created.FacilityId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFacility(int id, [FromBody] UpdateFacilityRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
    }
}