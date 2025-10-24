using AutoMapper;
using Consilient.Data;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class EmployeesController(ConsilientDbContext context, IMapper mapper) : Controller
    {
        private readonly ConsilientDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var providers = await _context.Employees.ToListAsync();

            var viewModel = _mapper.Map<List<EmployeeViewModel>>(providers);

            return View(viewModel);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<EmployeeViewModel>(employee);
            return View(viewModel);
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
                var provider = _mapper.Map<Employee>(viewModel);
                _context.Add(provider);
                await _context.SaveChangesAsync();
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

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            var viewModel = _mapper.Map<EmployeeViewModel>(employee);
            return View(viewModel);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel viewModel)
        {
            if (id != viewModel.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var employee = _mapper.Map<Employee>(viewModel);
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(viewModel.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
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

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<EmployeeViewModel>(employee);
            return View(viewModel);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var provider = await _context.Employees.FindAsync(id);
            if (provider != null)
            {
                _context.Employees.Remove(provider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
