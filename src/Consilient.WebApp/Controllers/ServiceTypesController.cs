using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class ServiceTypesController(IServiceTypesApi serviceTypesApi) : Controller
    {
        // GET: ServiceTypes
        public async Task<IActionResult> Index()
        {
            var serviceTypes = (await serviceTypesApi.GetAllAsync())
                .Unwrap()!
                .Select(MapToViewModel);
            return View(serviceTypes);
        }

        // GET: ServiceTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceType = (await serviceTypesApi.GetByIdAsync(id.Value)).Unwrap();
            if (serviceType == null)
            {
                return NotFound();
            }
            var serviceTypeViewModel = MapToViewModel(serviceType);
            return View(serviceTypeViewModel);
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
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            (await serviceTypesApi.CreateAsync(new Shared.Contracts.Requests.CreateServiceTypeRequest
            {
                CptCode = viewModel.Cptcode,
                Description = viewModel.Description
            })).Unwrap();
            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceType = (await serviceTypesApi.GetByIdAsync(id.Value)).Unwrap();
            if (serviceType == null)
            {
                return NotFound();
            }
            var serviceTypeViewModel = MapToViewModel(serviceType);
            return View(serviceTypeViewModel);
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
                return View(viewModel);
            }
            var serviceType = (await serviceTypesApi.UpdateAsync(id, new Shared.Contracts.Requests.UpdateServiceTypeRequest
            {
                CptCode = viewModel.Cptcode,
                Description = viewModel.Description
            })).Unwrap();
            if (serviceType == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceType = (await serviceTypesApi.GetByIdAsync(id.Value)).Unwrap();
            if (serviceType == null)
            {
                return NotFound();
            }
            var serviceTypeViewModel = MapToViewModel(serviceType);
            return View(serviceTypeViewModel);
        }

        // POST: ServiceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = (await serviceTypesApi.DeleteAsync(id)).Unwrap();
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        private static ServiceTypeViewModel MapToViewModel(ServiceTypeDto serviceType) =>
            new()
            {
                CodeAndDescription = $"{serviceType.CptCode} {serviceType.Description}",
                Cptcode = serviceType.CptCode,
                Description = serviceType.Description,
                ServiceTypeId = serviceType.Id
            };
    }
}
