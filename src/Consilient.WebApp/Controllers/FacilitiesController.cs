using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class FacilitiesController(IFacilitiesApi facilitiesApi) : Controller
    {
        // GET: Facilities
        public async Task<IActionResult> Index()
        {
            var facilities = (await facilitiesApi.GetAllAsync())
                .Unwrap()!
                .Select(MapToViewModel);
            return View(facilities);
        }

        // GET: Facilities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = (await facilitiesApi.GetByIdAsync(id.Value)).Unwrap();
            if (facility == null)
            {
                return NotFound();
            }
            var facilityViewModel = MapToViewModel(facility);
            return View(facilityViewModel);
        }

        // GET: Facilities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Facilities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacilityViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            _ = (await facilitiesApi.CreateAsync(new CreateFacilityRequest
            {
                FacilityName = viewModel.FacilityName,
                FacilityAbbreviation = viewModel.FacilityAbbreviation
            })).Unwrap();
            return RedirectToAction(nameof(Index));
        }

        // GET: Facilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = (await facilitiesApi.GetByIdAsync(id.Value)).Unwrap();
            if (facility == null)
            {
                return NotFound();
            }
            var facilityViewModel = MapToViewModel(facility);
            return View(facilityViewModel);
        }

        // POST: Facilities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacilityViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            var facility = (await facilitiesApi.UpdateAsync(id, new UpdateFacilityRequest
            {
                FacilityName = viewModel.FacilityName,
                FacilityAbbreviation = viewModel.FacilityAbbreviation
            })).Unwrap();
            if (facility == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Facilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = (await facilitiesApi.GetByIdAsync(id.Value)).Unwrap();
            if (facility == null)
            {
                return NotFound();
            }
            var facilityViewModel = MapToViewModel(facility);
            return View(facilityViewModel);
        }

        // POST: Facilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = (await facilitiesApi.DeleteAsync(id)).Unwrap();
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        private static FacilityViewModel MapToViewModel(FacilityDto dto) => new()
        {
            FacilityId = dto.Id,
            FacilityName = dto.Name,
            FacilityAbbreviation = dto.Abbreviation
        };
    }
}
