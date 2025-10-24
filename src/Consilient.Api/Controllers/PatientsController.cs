using Consilient.Patients.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController(IPatientService patientService) : ControllerBase
    {
        private readonly IPatientService _patientService = patientService;

        [HttpGet("{mrn}")]
        public async Task<IActionResult> GetByMRN(int mrn)
        {
            var patient = await _patientService.GetByMrnAsync(mrn);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
    }
}
