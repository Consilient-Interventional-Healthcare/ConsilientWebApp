using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class PatientVisitsController() : Controller
    {

        // GET: PatientVisits
        public async Task<IActionResult> Index(DateTime? selectedDate)
        {
            if (!selectedDate.HasValue)
            {
                selectedDate = DateTime.Now.AddDays(-1).Date;
            }

            var patientVisits = await _context.PatientVisits
                            .Where(v => v.DateServiced == DateOnly.FromDateTime(selectedDate.Value))
                            .Include(v => v.CosigningPhysicianEmployee)
                            .Include(v => v.Facility)
                            .Include(v => v.Insurance)
                            .Include(v => v.NursePractitionerEmployee)
                            .Include(v => v.Patient)
                            .Include(v => v.PhysicianEmployee)
                            .Include(v => v.ScribeEmployee)
                            .Include(v => v.ServiceType)
                            .ToListAsync();

            var patientVisitsViewModel = _mapper.Map<List<PatientVisitViewModel>>(patientVisits);
            var viewModel = new PatientVisitsIndexViewModel
            {
                PatientVisits = patientVisitsViewModel,
            };
            ViewBag.SelectedDate = selectedDate.Value;
            foreach (var visit in viewModel.PatientVisits)
            {
                var physicianName = visit.PhysicianEmployee?.FullName ?? "Unknown Physician";
                if (viewModel.PhysicianSummaries.TryGetValue(physicianName, out var physicianNameValue))
                {
                    viewModel.PhysicianSummaries[physicianName] = ++physicianNameValue;
                }
                else
                {
                    viewModel.PhysicianSummaries[physicianName] = 1;
                }
                if (visit.NursePractitionerEmployee != null)
                {
                    var npName = visit.NursePractitionerEmployee.FullName;
                    if (!string.IsNullOrEmpty(npName))
                    {
                        if (viewModel.NursePractitionerSummaries.TryGetValue(npName, out var npNameValue))
                        {
                            viewModel.NursePractitionerSummaries[npName] = ++npNameValue;
                        }
                        else
                        {
                            viewModel.NursePractitionerSummaries[npName] = 1;
                        }
                    }
                }
                if (visit.ScribeEmployee != null)
                {
                    var scribeName = visit.ScribeEmployee.FullName;
                    if (!string.IsNullOrEmpty(scribeName))
                    {
                        if (viewModel.ScribeSummaries.TryGetValue(scribeName, out var scribeNameValue))
                        {
                            viewModel.ScribeSummaries[scribeName] = ++scribeNameValue;
                        }
                        else
                        {
                            viewModel.ScribeSummaries[scribeName] = 1;
                        }
                    }
                }
            }
            return View(viewModel);
        }

        // GET: PatientVisits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientVisit = await _context.PatientVisits
                            .Include(v => v.CosigningPhysicianEmployee)
                            .Include(v => v.Facility)
                            .Include(v => v.Insurance)
                            .Include(v => v.NursePractitionerEmployee)
                            .Include(v => v.Patient)
                            .Include(v => v.PhysicianEmployee)
                            .Include(v => v.ScribeEmployee)
                            .Include(v => v.ServiceType)
                .FirstOrDefaultAsync(m => m.PatientVisitId == id);
            if (patientVisit == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PatientVisitViewModel>(patientVisit);
            return View(viewModel);
        }
    }
}
