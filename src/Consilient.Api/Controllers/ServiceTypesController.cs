using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ServiceTypesController(IServiceTypeService serviceTypeService) : ControllerBase
    {
        private readonly IServiceTypeService _serviceTypeService = serviceTypeService;

        [HttpGet]
        public async Task<IActionResult> GetServiceTypes()
        {
            var items = await _serviceTypeService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceTypeById(int id)
        {
            var item = await _serviceTypeService.GetById(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceType([FromBody] CreateServiceTypeRequest request)
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
                var created = await _serviceTypeService.CreateAsync(request);
                return CreatedAtAction(nameof(GetServiceTypeById), new { id = created.ServiceTypeId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateServiceType(int id, [FromBody] UpdateServiceTypeRequest request)
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
                var updated = await _serviceTypeService.UpdateAsync(id, request);
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
        public async Task<IActionResult> DeleteServiceType(int id)
        {
            var deleted = await _serviceTypeService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}