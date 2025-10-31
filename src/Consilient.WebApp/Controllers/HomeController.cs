using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.WebApp.Infra;
using Consilient.WebApp.Models;
using Consilient.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static Consilient.WebApp.ViewModels.HomeIndexViewModel;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class HomeController(IEmployeesApi employeesApi, IStagingPatientVisitsApi stagingPatientVisitsApi, ICurrentUserService currentUserService) : Controller
    {
        public async Task<IActionResult> Index(TimeframeOptions? timeframe)
        {
            var viewModel = new HomeIndexViewModel();

            if (timeframe.HasValue)
            {
                viewModel.SelectedTimeframe = timeframe.Value;
            }

            var userEmail = currentUserService.UserEmail;
            if (string.IsNullOrEmpty(userEmail))
            {
                return View(viewModel);
            }
            var employee = (await employeesApi.GetByEmailAsync(userEmail)).Unwrap();
            if (employee == null)
            {
                return View(viewModel);
            }
            var employeeVisits = (await stagingPatientVisitsApi.GetByEmployeeAsync(employee.EmployeeId)).Unwrap()!
                .Where(e => e.DateServiced >= viewModel.LowerDateRange)
                .ToList(); // lower date range set within the view model
            viewModel.EncountersToday = employeeVisits.Count;

            viewModel.PendingApprovals = employeeVisits.Count(e => !e.AddedToMainTable);
            viewModel.ApprovedEncounters = employeeVisits.Count(e => e.AddedToMainTable);
            viewModel.ErrorEncounters = 0;

            // TODO: H
            //var last7Days = DateTime.Today.AddDays(-7);
            //viewModel.Last7DaysApproved = _context.PatientVisitsStagings.Count(e => e.DateServiced >= DateOnly.FromDateTime(last7Days) && e.AddedToMainTable == true && (e.PhysicianEmployeeId == employee.EmployeeId || e.NursePractitionerEmployeeId == employee.EmployeeId));
            //viewModel.Last7DaysPending = _context.PatientVisitsStagings.Count(e => e.DateServiced >= DateOnly.FromDateTime(last7Days) && e.AddedToMainTable == false && (e.PhysicianEmployeeId == employee.EmployeeId || e.NursePractitionerEmployeeId == employee.EmployeeId));

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
