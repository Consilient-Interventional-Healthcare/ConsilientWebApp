using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class PatientVisitsController(IPatientVisitsApi patientVisitsApi, IGraphQlApi graphQlApi) : Controller
    {
        // GET: PatientVisits
        public async Task<IActionResult> Index(DateOnly? selectedDate)
        {
            selectedDate ??= DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            var patientVisits = (await graphQlApi.Query<PatientVisitViewModel>("").ConfigureAwait(false)).Unwrap()!;
            var viewModel = new PatientVisitsIndexViewModel
            {
                PatientVisits = [.. patientVisits]
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
                    var npName = visit.NursePractitionerEmployee?.FullName;
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
        public async Task<IActionResult> Details(int id)
        {
            var patientVisit = (await patientVisitsApi.GetByIdAsync(id)).Unwrap();
            if (patientVisit == null)
            {
                return NotFound();
            }
            return View(patientVisit);
        }
    }
}
