using AutoMapper;
using Consilient.Data;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class ServiceTypesController(ConsilientDbContext context, IMapper mapper) : Controller
    {
        private readonly ConsilientDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        // GET: ServiceTypes
        public async Task<IActionResult> Index()
        {
            var serviceTypes = await _context.ServiceTypes.ToListAsync();

            var viewModel = _mapper.Map<List<ServiceTypeViewModel>>(serviceTypes);

            return View(viewModel);
        }

        // GET: ServiceTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceType = await _context.ServiceTypes
                .FirstOrDefaultAsync(m => m.ServiceTypeId == id);
            if (serviceType == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<ServiceTypeViewModel>(serviceType);
            return View(viewModel);
        }

        // GET: ServiceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServiceTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTypeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var serviceType = _mapper.Map<ServiceType>(viewModel);
                _context.Add(serviceType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: ServiceTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<ServiceTypeViewModel>(serviceType);
            return View(viewModel);
        }

        // POST: ServiceTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceTypeViewModel viewModel)
        {
            if (id != viewModel.ServiceTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var serviceType = _mapper.Map<ServiceType>(viewModel);
                    _context.Update(serviceType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceTypeExists(viewModel.ServiceTypeId))
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

        // GET: ServiceTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceType = await _context.ServiceTypes
                .FirstOrDefaultAsync(m => m.ServiceTypeId == id);
            if (serviceType == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<ServiceTypeViewModel>(serviceType);
            return View(viewModel);
        }

        // POST: ServiceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType != null)
            {
                _context.ServiceTypes.Remove(serviceType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceTypeExists(int id)
        {
            return _context.ServiceTypes.Any(e => e.ServiceTypeId == id);
        }
    }
}
