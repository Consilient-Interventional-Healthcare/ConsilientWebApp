using Consilient.Employees.Contracts;
using Consilient.Employees.Contracts.Dtos;
using Consilient.Employees.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmployeesController(IEmployeeService _employeeService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            var created = await _employeeService.CreateAsync(request).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _employeeService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllAsync().ConfigureAwait(false);
            return Ok(employees);
        }

        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var employee = await _employeeService.GetByEmailAsync(email).ConfigureAwait(false);
            return employee == null ? NotFound() : Ok(employee);
        }

        [HttpGet("{id:int}", Name = "GetEmployeeById")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id).ConfigureAwait(false);
            return employee == null ? NotFound() : Ok(employee);
        }

        [HttpGet("visit-counts")]
        [ProducesResponseType(typeof(List<EmployeeVisitCountDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EmployeeVisitCountDto>>> GetEmployeesWithVisitCount([FromQuery] DateOnly date)
        {
            var result = await _employeeService.GetEmployeesWithVisitCountPerDayAsync(date);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
        {
            var updated = await _employeeService.UpdateAsync(id, request).ConfigureAwait(false);
            return updated == null ? NotFound() : Ok(updated);
        }
    }
}
