using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class EmployeesController(IEmployeesApi employeesApi) : Controller
    {
        private readonly IEmployeesApi _employeesApi = employeesApi;

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var providers = (await _employeesApi.GetAllAsync()).Unwrap();
            return View(providers);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var employee = (await _employeesApi.GetByIdAsync(id.Value)).Unwrap();
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
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
            if (ModelState.IsValid)
            {
                (await _employeesApi.CreateAsync(new Employees.Contracts.Requests.CreateEmployeeRequest
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
            return View(viewModel);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = (await _employeesApi.GetByIdAsync(id.Value)).Unwrap();
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var employee = (await _employeesApi.UpdateAsync(viewModel.EmployeeId, new Employees.Contracts.Requests.UpdateEmployeeRequest
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
            return View(viewModel);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = (await _employeesApi.GetByIdAsync(id.Value)).Unwrap();
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = (await _employeesApi.DeleteAsync(id)).Unwrap();
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
