using AutoMapper;
using ConsilientWebApp.Data;
using ConsilientWebApp.Models;
using ConsilientWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsilientWebApp.Controllers
{
    [Authorize]
    public class PatientVisitsController : Controller
    {
        private readonly ConsilientContext _context;
        private readonly IMapper _mapper;

        public PatientVisitsController(ConsilientContext context,
                                       IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: PatientVisits
        public async Task<IActionResult> Index(DateTime? selectedDate)
        {
            if (!selectedDate.HasValue)
                selectedDate = DateTime.Now.AddDays(-1).Date;

            var patientVisits = await _context.PatientVisits
                            .Where(v => v.DateServiced == DateOnly.FromDateTime(selectedDate.Value))
                            .Include(v => v.CosigningPhysicianEmployee)
                            .Include(v => v.Facility)
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
                string physicianName = visit.PhysicianEmployee?.FullName ?? "Unknown Physician";
                if (viewModel.PhysicianSummaries.ContainsKey(physicianName))
                {
                    viewModel.PhysicianSummaries[physicianName]++;
                }
                else
                {
                    viewModel.PhysicianSummaries[physicianName] = 1;
                }
                if (visit.NursePractitionerEmployee != null)
                {
                    var npName = visit.NursePractitionerEmployee.FullName;
                    if (viewModel.NursePractitionerSummaries.ContainsKey(npName))
                    {
                        viewModel.NursePractitionerSummaries[npName]++;
                    }
                    else
                    {
                        viewModel.NursePractitionerSummaries[npName] = 1;
                    }
                }
                if (visit.ScribeEmployee != null)
                {
                    var scribeName = visit.ScribeEmployee.FullName;
                    if (viewModel.ScribeSummaries.ContainsKey(scribeName))
                    {
                        viewModel.ScribeSummaries[scribeName]++;
                    }
                    else
                    {
                        viewModel.ScribeSummaries[scribeName] = 1;
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
