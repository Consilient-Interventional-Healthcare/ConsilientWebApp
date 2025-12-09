using Consilient.Insurances.Contracts;
using Consilient.Insurances.Contracts.Dtos;
using Consilient.Insurances.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InsurancesController(IInsuranceService insuranceService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(InsuranceDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateInsuranceRequest request)
        {
            var created = await insuranceService.CreateAsync(request).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(InsuranceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var insurance = await insuranceService.GetByIdAsync(id).ConfigureAwait(false);
            return insurance == null ? NotFound() : Ok(insurance);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InsuranceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await insuranceService.GetAllAsync().ConfigureAwait(false);
            return Ok(items);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(InsuranceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInsuranceRequest request)
        {
            var updated = await insuranceService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated == null ? NotFound() : Ok(updated);
        }
    }
}