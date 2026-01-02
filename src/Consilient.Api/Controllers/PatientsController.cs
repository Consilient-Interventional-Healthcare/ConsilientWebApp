using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Models;
using Consilient.Patients.Contracts.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController(IPatientService patientService) : ControllerBase
    {

        [HttpPost]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
        {
            var created = await patientService.CreateAsync(request).ConfigureAwait(false);
            return created is null
                ? BadRequest()
                : CreatedAtAction(nameof(GetByMrn), new { mrn = created.Mrn }, created);
        }

        [HttpGet("{mrn:int}")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByMrn(int mrn)
        {
            var patient = await patientService.GetByMrnAsync(mrn);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
    }
}
