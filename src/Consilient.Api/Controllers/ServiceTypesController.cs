using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{


    [Route("[controller]")]
    [ApiController]
    public class ServiceTypesController(IServiceTypeService serviceTypeService) : ControllerBase
    {
        private readonly IServiceTypeService _serviceTypeService =
            serviceTypeService ?? throw new ArgumentNullException(nameof(serviceTypeService));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceTypeRequest request)
        {
            var created = await _serviceTypeService.CreateAsync(request).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _serviceTypeService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _serviceTypeService.GetByIdAsync(id).ConfigureAwait(false);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _serviceTypeService.GetAllAsync().ConfigureAwait(false);
            return Ok(items);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceTypeRequest request)
        {
            var updated = await _serviceTypeService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated == null ? NotFound() : Ok(updated);
        }
    }
}