using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class PatientVisitsController(IGraphQlApi graphQlApi) : Controller
    {
        // GET: PatientVisits
        public async Task<IActionResult> Index(DateOnly? selectedDate)
        {
            selectedDate ??= DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            var query = @"query {
                patientVisits {
                    id, dateServiced,
                    patient {
    	                patientMrn, patientLastName, patientFirstName
                    }
                }
            }";
            var patientVisits = (await graphQlApi.Query(query).ConfigureAwait(false))
                .Unwrap()!;
            
            var patientVisitsDtos = patientVisits.Unwrap<IEnumerable<PatientVisitViewModel>>("patientVisits")!.ToList();
            var viewModel = new PatientVisitsIndexViewModel
            {
                PatientVisits = patientVisitsDtos
            };
            ViewBag.SelectedDate = selectedDate.Value;
            viewModel.PhysicianSummaries ??= [];
            viewModel.NursePractitionerSummaries ??= [];
            viewModel.ScribeSummaries ??= [];

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
            var query = @"query {
                patientVisits {
                id
                }
            }";
            var patientVisit = (await graphQlApi.Query(query).ConfigureAwait(false))
                .Unwrap()!
                .Unwrap<IEnumerable<PatientVisitViewModel>>("patientVisits")!.SingleOrDefault();
            if (patientVisit == null)
            {
                return NotFound();
            }
            return View(patientVisit);
        }
    }
}
