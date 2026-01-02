using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.Employees.Contracts.Dtos;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class EmployeesController(IEmployeesApi employeesApi) : Controller
    {
        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var providers = (await employeesApi.GetAllAsync())
                .Unwrap()!
                .Select(MapToViewModel);
            return View(providers);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var employee = (await employeesApi.GetByIdAsync(id.Value)).Unwrap();
            if (employee == null)
            {
                return NotFound();
            }
            var employeeViewModel = MapToViewModel(employee);
            return View(employeeViewModel);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            var viewModel = new EmployeeViewModel();
            return View(viewModel);
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            (await employeesApi.CreateAsync(new Employees.Contracts.Requests.CreateEmployeeRequest
            {
                CanApproveVisits = viewModel.CanApproveVisits,
                Email = viewModel.Email,
                FirstName = viewModel.FirstName,
                IsAdministrator = viewModel.IsAdministrator,
                IsProvider = viewModel.IsProvider,
                LastName = viewModel.LastName,
                Role = viewModel.Role,
                TitleExtension = viewModel.TitleExtension
            })).Unwrap();
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = (await employeesApi.GetByIdAsync(id.Value)).Unwrap();
            if (employee == null)
            {
                return NotFound();
            }
            var employeeViewModel = MapToViewModel(employee);
            return View(employeeViewModel);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                return View(viewModel);
            }
            var employee = (await employeesApi.UpdateAsync(viewModel.EmployeeId, new Employees.Contracts.Requests.UpdateEmployeeRequest
            {
                CanApproveVisits = viewModel.CanApproveVisits,
                FirstName = viewModel.FirstName,
                IsAdministrator = viewModel.IsAdministrator,
                IsProvider = viewModel.IsProvider,
                LastName = viewModel.LastName,
                Role = viewModel.Role,
                TitleExtension = viewModel.TitleExtension
            })).Unwrap();
            if (employee == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = (await employeesApi.GetByIdAsync(id.Value)).Unwrap();
            if (employee == null)
            {
                return NotFound();
            }
            var employeeViewModel = MapToViewModel(employee);
            return View(employeeViewModel);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = (await employeesApi.DeleteAsync(id)).Unwrap();
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        private static EmployeeViewModel MapToViewModel(EmployeeDto employee) =>
            new()
            {
                CanApproveVisits = employee.CanApproveVisits,
                Email = employee.Email,
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                IsAdministrator = employee.IsAdministrator,
                IsProvider = employee.IsProvider,
                Role = employee.Role,
                TitleExtension = employee.TitleExtension
            };
    }
}
