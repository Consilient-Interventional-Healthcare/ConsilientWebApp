using System.Diagnostics;
using ConsilientWebApp.Data;
using ConsilientWebApp.Models;
using ConsilientWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static ConsilientWebApp.ViewModels.HomeIndexViewModel;

namespace ConsilientWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConsilientContext _context;

        public HomeController(ILogger<HomeController> logger,
                            ConsilientContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(TimeframeOptions? timeframe)
        {
            HomeIndexViewModel viewModel = new HomeIndexViewModel();

            if (timeframe.HasValue)
                viewModel.SelectedTimeframe = timeframe.Value;

            var userEmail = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userEmail))
            {
                var employee = _context.Employees.FirstOrDefault(e => e.Email.ToLower() == userEmail.ToLower());
                if (employee != null)
                {
                    var employeeVisits = _context.PatientVisitsStagings.Where(e => e.DateServiced >= viewModel.LowerDateRange).ToList(); // lower date range set within the view model
                    employeeVisits = employeeVisits.Where(e => e.PhysicianEmployeeId == employee.EmployeeId || e.NursePractitionerEmployeeId == employee.EmployeeId).ToList();
                    viewModel.EncountersToday = employeeVisits.Count;

                    viewModel.PendingApprovals = employeeVisits.Count(e => e.AddedToMainTable == false);
                    viewModel.ApprovedEncounters = employeeVisits.Count(e => e.AddedToMainTable == true);
                    viewModel.ErrorEncounters = 0;

                    var last7Days = DateTime.Today.AddDays(-7);
                    viewModel.Last7DaysApproved = _context.PatientVisitsStagings.Count(e => e.DateServiced >= DateOnly.FromDateTime(last7Days) && e.AddedToMainTable == true && (e.PhysicianEmployeeId == employee.EmployeeId || e.NursePractitionerEmployeeId == employee.EmployeeId));
                    viewModel.Last7DaysPending = _context.PatientVisitsStagings.Count(e => e.DateServiced >= DateOnly.FromDateTime(last7Days) && e.AddedToMainTable == false && (e.PhysicianEmployeeId == employee.EmployeeId || e.NursePractitionerEmployeeId == employee.EmployeeId));
                }
            }

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
