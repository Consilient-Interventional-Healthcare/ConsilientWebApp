using Consilient.Employees.Services;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmployeesController(EmployeeService employeeService) : ControllerBase
    {
        private readonly EmployeeService _employeeService = employeeService;

        public async Task<IActionResult> GetEmployees()
        {
            var employee = await _employeeService.GetAllAsync();
            return Ok(employee);
        }
    }
}
