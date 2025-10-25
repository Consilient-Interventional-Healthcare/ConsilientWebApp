using Consilient.Insurances.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InsurancesController(IInsuranceService insuranceService) : ControllerBase
    {
        private readonly IInsuranceService _insuranceService = insuranceService;

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInsuranceById(int id)
        {
            var insurance = await _insuranceService.GetById(id);
            if (insurance == null)
            {
                return NotFound();
            }

            return Ok(insurance);
        }
    }
}