using Consilient.Insurances.Contracts;
using Consilient.Insurances.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InsurancesController(IInsuranceService insuranceService) : ControllerBase
    {
        private readonly IInsuranceService _insuranceService = insuranceService ?? throw new ArgumentNullException(nameof(insuranceService));

        [HttpPost]
        public async Task<IActionResult> CreateInsurance([FromBody] CreateInsuranceRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            try
            {
                var created = await _insuranceService.CreateAsync(request);
                return CreatedAtAction(nameof(GetInsuranceById), new { id = created?.InsuranceId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInsuranceById(int id)
        {
            var insurance = await _insuranceService.GetByIdAsync(id);
            if (insurance == null)
            {
                return NotFound();
            }

            return Ok(insurance);
        }

        [HttpGet]
        public async Task<IActionResult> GetInsurances()
        {
            var items = await _insuranceService.GetAllAsync();
            return Ok(items);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInsurance(int id, [FromBody] UpdateInsuranceRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            try
            {
                var updated = await _insuranceService.UpdateAsync(id, request);
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