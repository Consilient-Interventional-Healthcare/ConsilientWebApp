using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.Insurances.Contracts.Requests;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class InsurancesController(IInsurancesApi insurancesApi) : Controller
    {
        // GET: Insurances
        public async Task<IActionResult> Index()
        {
            var insurances = await insurancesApi.GetAllAsync();
            return View(insurances);
        }

        // GET: Insurances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = (await insurancesApi.GetByIdAsync(id.Value)).Unwrap();
            if (insurance == null)
            {
                return NotFound();
            }

            return View(insurance);
        }

        // GET: Insurances/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Insurances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            _ = await insurancesApi.CreateAsync(new CreateInsuranceRequest
            {
                InsuranceCode = viewModel.InsuranceCode,
                InsuranceDescription = viewModel.InsuranceDescription,
                PhysicianIncluded = viewModel.PhysicianIncluded,
                IsContracted = viewModel.IsContracted
            });
            return RedirectToAction(nameof(Index));
        }

        // GET: Insurances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = (await insurancesApi.GetByIdAsync(id.Value)).Unwrap();
            if (insurance == null)
            {
                return NotFound();
            }
            return View(insurance);
        }

        // POST: Insurances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InsuranceViewModel viewModel)
        {
            if (id != viewModel.InsuranceId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            var insurance = (await insurancesApi.UpdateAsync(viewModel.InsuranceId, new UpdateInsuranceRequest
            {
                InsuranceCode = viewModel.InsuranceCode,
                InsuranceDescription = viewModel.InsuranceDescription,
                PhysicianIncluded = viewModel.PhysicianIncluded,
                IsContracted = viewModel.IsContracted
            })).Unwrap();

            if (insurance == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Insurances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = (await insurancesApi.GetByIdAsync(id.Value)).Unwrap();
            if (insurance == null)
            {
                return NotFound();
            }
            return View(insurance);
        }

        // POST: Insurances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = (await insurancesApi.DeleteAsync(id)).Unwrap();
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
