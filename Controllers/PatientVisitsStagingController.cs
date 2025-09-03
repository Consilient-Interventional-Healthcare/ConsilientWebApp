using AutoMapper;
using ClosedXML.Excel;
using ConsilientWebApp.Data;
using ConsilientWebApp.Models;
using ConsilientWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Linq.Expressions;

namespace ConsilientWebApp.Controllers
{
    [Authorize]
    public class PatientVisitsStagingController : Controller
    {
        private readonly ConsilientContext _context;
        private readonly IMapper _mapper;

        public PatientVisitsStagingController(ConsilientContext context,
                                       IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: PatientVisitsStaging
        public async Task<IActionResult> Index(DateOnly? selectedDate,
                                                int? selectedFacilityId,
                                                int? selectedProviderId)
        {
            var query = _context.PatientVisitsStagings.AsQueryable();

            var sessionSelectedDate = HttpContext.Session.GetString("SelectedDate");
            if (!selectedDate.HasValue && string.IsNullOrEmpty(sessionSelectedDate))
                selectedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            else if (!selectedDate.HasValue)
                selectedDate = DateOnly.FromDateTime(Convert.ToDateTime(sessionSelectedDate));

            HttpContext.Session.SetString("SelectedDate", selectedDate.Value.ToString("yyyy-MM-dd"));

            query = query.Where(p => p.DateServiced == selectedDate.Value);

            if (selectedFacilityId.HasValue)
                query = query.Where(p => p.Facility.FacilityId == selectedFacilityId);
            if (selectedProviderId.HasValue)
                query = query.Where(p => (p.PhysicianEmployeeId == selectedProviderId || p.NursePractitionerEmployeeId == selectedProviderId));

            ViewBag.SelectedDate = selectedDate;
            ViewBag.SelectedFacility = selectedFacilityId;
            ViewBag.SelectedProvider = selectedProviderId;

            var patientVisitsStaging = await query
                                            .Where(m => !m.AddedToMainTable && m.DateServiced == selectedDate.Value)
                                            .Include(m => m.CosigningPhysicianEmployee)
                                            .Include(m => m.Facility)
                                            .Include(m => m.Insurance)
                                            .Include(m => m.NursePractitionerEmployee)
                                            .Include(m => m.Patient)
                                            .Include(m => m.PhysicianEmployee)
                                            .Include(m => m.ScribeEmployee)
                                            .Include(m => m.ServiceType)
                                .ToListAsync();

            var patientVisitsStagingViewModels = _mapper.Map<List<PatientVisitsStagingViewModel>>(patientVisitsStaging);
            var viewModel = new PatientVisitsStagingIndexViewModel
            {
                PatientVisitsStaging = patientVisitsStagingViewModels,
                SelectedDate = selectedDate.Value,
                SelectedFacilityId = selectedFacilityId ?? 0,
                SelectedProviderId = selectedProviderId ?? 0
            };

            CreateIndexSelectLists(viewModel);

            foreach (var visit in viewModel.PatientVisitsStaging)
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

        // GET: PatientVisitsStaging/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientVisitStaging = await _context.PatientVisitsStagings
                                        .Include(m => m.CosigningPhysicianEmployee)
                                        .Include(m => m.Facility)
                                        .Include(m => m.Insurance)
                                        .Include(m => m.NursePractitionerEmployee)
                                        .Include(m => m.Patient)
                                        .Include(m => m.PhysicianEmployee)
                                        .Include(m => m.ScribeEmployee)
                                        .Include(m => m.ServiceType)
                .FirstOrDefaultAsync(m => m.PatientVisitStagingId == id);
            if (patientVisitStaging == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PatientVisitsStagingViewModel>(patientVisitStaging);
            return View(viewModel);
        }

        // GET: PatientVisitsStaging/Create
        public IActionResult Create()
        {
            var viewModel = new PatientVisitsStagingViewModel();
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SelectedDate")))
                viewModel.DateServiced = DateOnly.FromDateTime(Convert.ToDateTime(HttpContext.Session.GetString("SelectedDate")));

            CreateSelectLists(viewModel);
            return View(viewModel);
        }

        // POST: PatientVisitsStaging/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientVisitsStagingViewModel viewModel, string patientOption)
        {
            if (!ModelState.IsValid)
            {
                CreateSelectLists(viewModel);
                return View(viewModel);
            }

            int patientId;

            if (patientOption == "existing" && viewModel.PatientId != null)
            {
                patientId = viewModel.PatientId.Value;
            }
            else
            {
                var patientExists = _context.Patients
                    .FirstOrDefault(p => p.PatientMrn == viewModel.NewPatient.PatientMrn);
                if (patientExists != null)
                {
                    TempData["ErrorMessage"] = $"Patient with MRN {viewModel.NewPatient.PatientMrn} already exists. Please select the patient from the dropdown.";
                    viewModel.NewPatient = new PatientViewModel(); // clears values of new patient
                    viewModel.Patient.PatientMrn = patientExists.PatientMrn;
                    CreateSelectLists(viewModel);
                    return View(viewModel);
                }

                var patient = new Patient
                {
                    PatientMrn = viewModel.NewPatient.PatientMrn,
                    PatientFirstName = viewModel.NewPatient.PatientFirstName,
                    PatientLastName = viewModel.NewPatient.PatientLastName,
                    PatientBirthDate = viewModel.NewPatient.PatientBirthDate,
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                patientId = patient.PatientId;
            }


            var patientVisitStaging = _mapper.Map<PatientVisitsStaging>(viewModel);
            patientVisitStaging.PatientId = patientId;
            _context.PatientVisitsStagings.Add(patientVisitStaging);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: PatientVisitsStaging/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var patientVisitStaging = await _context.PatientVisitsStagings
                                        .Include(m => m.CosigningPhysicianEmployee)
                                        .Include(m => m.Facility)
                                        .Include(m => m.Insurance)
                                        .Include(m => m.NursePractitionerEmployee)
                                        .Include(m => m.Patient)
                                        .Include(m => m.PhysicianEmployee)
                                        .Include(m => m.ScribeEmployee)
                                        .Include(m => m.ServiceType)
                                        .FirstOrDefaultAsync(m => m.PatientVisitStagingId == id);
            if (patientVisitStaging == null) return NotFound();

            var viewModel = _mapper.Map<PatientVisitsStagingViewModel>(patientVisitStaging);
            CreateSelectLists(viewModel);
            return View(viewModel);
        }

        // POST: PatientVisitsStaging/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PatientVisitsStagingViewModel viewModel, string patientOption)
        {
            if (ModelState.IsValid)
            {
                int patientId;

                if (patientOption == "existing" && viewModel.PatientId != null)
                {
                    patientId = viewModel.PatientId.Value;
                }
                else
                {
                    var patient = new Patient
                    {
                        PatientMrn = viewModel.NewPatient.PatientMrn,
                        PatientFirstName = viewModel.NewPatient.PatientFirstName,
                        PatientLastName = viewModel.NewPatient.PatientLastName,
                        PatientBirthDate = viewModel.NewPatient.PatientBirthDate,
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                    patientId = patient.PatientId;
                }

                try
                {
                    var patientVisitStaging = _mapper.Map<PatientVisitsStaging>(viewModel);
                    patientVisitStaging.PatientId = patientId;
                    _context.Update(patientVisitStaging);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientVisitStagingExists(viewModel.PatientVisitStagingId))
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

            CreateSelectLists(viewModel);
            return View(viewModel);
        }

        // GET: PatientVisitsStaging/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientVisitStaging = await _context.PatientVisitsStagings
                                        .Include(m => m.CosigningPhysicianEmployee)
                                        .Include(m => m.Facility)
                                        .Include(m => m.Insurance)
                                        .Include(m => m.NursePractitionerEmployee)
                                        .Include(m => m.Patient)
                                        .Include(m => m.PhysicianEmployee)
                                        .Include(m => m.ScribeEmployee)
                                        .Include(m => m.ServiceType)
                                    .FirstOrDefaultAsync(m => m.PatientVisitStagingId == id);
            if (patientVisitStaging == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PatientVisitsStagingViewModel>(patientVisitStaging);
            return View(viewModel);
        }

        // POST: PatientVisitsStaging/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patientVisitStaging = await _context.PatientVisitsStagings.FindAsync(id);
            if (patientVisitStaging != null)
            {
                _context.PatientVisitsStagings.Remove(patientVisitStaging);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult SaveApprovals(PatientVisitsStagingIndexViewModel viewModel)
        {
            foreach (var visit in viewModel.PatientVisitsStaging)
            {
                var patientVisitStaging = _context.PatientVisitsStagings
                    .FirstOrDefault(p => p.PatientVisitStagingId == visit.PatientVisitStagingId);
                if (patientVisitStaging != null)
                {
                    patientVisitStaging.PhysicianApproved = visit.PhysicianApproved;
                    patientVisitStaging.PhysicianApprovedDateTime = DateTime.Now;
                    _context.Update(patientVisitStaging);
                }
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ApproveAllByDay(DateOnly selectedDate)
        {
            var visits = _context.PatientVisitsStagings
                .Where(p => p.DateServiced == selectedDate && !p.AddedToMainTable)
                .ToList();

            foreach (var visit in visits)
            {
                visit.PhysicianApproved = true;
                visit.NursePractitionerApproved = true;
                visit.PhysicianApprovedDateTime = DateTime.Now;
                visit.PhysicianApprovedBy = User?.Identity?.Name;
                _context.Update(visit);
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new {selectedDate = selectedDate});
        }

        [HttpPost]
        public async Task<IActionResult> PushApprovedPatientVisits()
        {
            var approvedPatientVisitsStaging = await _context.PatientVisitsStagings
                .Where(p => p.PhysicianApproved && !p.AddedToMainTable)
                .ToListAsync();
            if (approvedPatientVisitsStaging.Count == 0)
            {
                TempData["ErrorMessage"] = "No approved patient encounters to push.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var patientVisitStaging in approvedPatientVisitsStaging)
            {
                var patientVisit = _mapper.Map<PatientVisit>(patientVisitStaging);
                _context.Update(patientVisit);  

                patientVisitStaging.AddedToMainTable = true; // Mark as added to main table
                _context.Update(patientVisitStaging);
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Approved patient encounters have been pushed successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UploadSpreadsheet(IFormFile spreadsheet)
        {
            if (spreadsheet == null || spreadsheet.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid spreadsheet file.";
                return RedirectToAction(nameof(Index));
            }

            var records = new List<PatientVisitsStaging>();
            using (var stream = new MemoryStream())
            {
                await spreadsheet.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["ErrorMessage"] = "The spreadsheet is empty or not formatted correctly.";
                        return RedirectToAction(nameof(Index));
                    }

                    var rows = worksheet.RowsUsed().Skip(8); // Skip header rows

                    foreach (var row in rows)
                    {
                        var facilityId = _context.Facilities
                                .Where(f => f.FacilityName == "Santa Rosa Hospital")
                                .Select(f => f.FacilityId)
                                .FirstOrDefault();

                        if (facilityId == 0)
                        {
                            TempData["ErrorMessage"] = "Facility 'Santa Rosa Hospital' not found. Please add it via the Administration menu.";
                            return RedirectToAction(nameof(Index));
                        }

                        var serviceTypeId = _context.ServiceTypes
                            .Where(s => s.Cptcode == Convert.ToInt32(row.Cell(4).GetString().Trim())
                                        && s.Description == row.Cell(5).GetString().Trim())
                            .Select(s => s.ServiceTypeId)
                            .FirstOrDefault();

                        if (serviceTypeId == 0)
                        {
                            TempData["ErrorMessage"] += $"Service type with CPT code {row.Cell(4).GetString().Trim()} and description {row.Cell(5).GetString().Trim()} not found. Please correct on spreadsheet or add service type to records via the Administration menu.";
                            return RedirectToAction(nameof(Index));
                        }

                        var physicianEmployeeId = _context.Employees
                                .Where(e => e.LastName == row.Cell(6).GetString().Trim())
                                .Select(e => e.EmployeeId)
                                .FirstOrDefault();

                        if (physicianEmployeeId == 0)
                        {
                            TempData["ErrorMessage"] += $"Physician {row.Cell(6).GetString().Trim()} not found. Please correct name on spreadsheet or add employee to records via the Administration menu.";
                            return RedirectToAction(nameof(Index));
                        }

                        var nursePractitionerCell = row.Cell(7).GetString().Trim();

                        var nursePractitionerEmployeeId = _context.Employees
                                .Where(e => e.LastName == nursePractitionerCell)
                                .Select(e => e.EmployeeId)
                                .FirstOrDefault();

                        if (!string.IsNullOrEmpty(nursePractitionerCell) && nursePractitionerEmployeeId == 0)
                        {
                            TempData["WarningMessage"] += $"Nurse Practitioner {nursePractitionerCell} not found.\r\n";
                            return RedirectToAction(nameof(Index));
                        }

                        var cellH = row.Cell(8).GetString().ToLower();
                        int? scribeEmployeeId = null;
                        if (!string.IsNullOrEmpty(cellH))
                        {
                            var scribeFirstName = cellH.Split(' ')[0].Trim();
                            var scribeLastInitial = cellH.Split(' ')[1].Trim().TrimEnd('.');

                            scribeEmployeeId = _context.Employees
                                    .Where(e => e.FirstName.ToLower() == scribeFirstName && e.LastName.ToLower().Substring(0, 1) == scribeLastInitial)
                                    .Select(e => e.EmployeeId)
                                    .FirstOrDefault();
                        }


                        var patientVisitStaging = new PatientVisitsStaging
                        {
                            DateServiced = DateOnly.FromDateTime(row.Cell(1).GetDateTime()),
                            //PatientName = row.Cell(2).GetString(),
                            //PatientMrn = Convert.ToInt32(row.Cell(3).GetString()),
                            FacilityId = facilityId,
                            ServiceTypeId = serviceTypeId,
                            PhysicianEmployeeId = physicianEmployeeId,
                            NursePractitionerEmployeeId = nursePractitionerEmployeeId == 0 ? null : nursePractitionerEmployeeId,
                            ScribeEmployeeId = scribeEmployeeId == 0 ? null : scribeEmployeeId,
                        };
                        records.Add(patientVisitStaging);
                    }
                }
            }
            

            _context.PatientVisitsStagings.AddRange(records);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{records.Count} patient visits have been added to staging.";
            return RedirectToAction(nameof(Index));
        }

        private bool PatientVisitStagingExists(int id)
        {
            return _context.PatientVisitsStagings.Any(e => e.PatientVisitStagingId == id);
        }


        private void CreateSelectLists(PatientVisitsStagingViewModel viewModel)
        {
            // PHYSICIANS
            var physicianList = _context.Employees
                .Where(p => p.Role == "Physician")
                .Select(p => new SelectListItem
                {
                    Value = p.EmployeeId.ToString(),
                    Text = p.FullName,
                    Selected = (p.EmployeeId == viewModel.PhysicianEmployeeId)
                })
                .ToList();

            // Only add default if no match was selected
            if (!physicianList.Any(p => p.Selected))
            {
                physicianList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a provider -- ",
                    Selected = true
                });
            }
            else
            {
                physicianList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a provider -- "
                });
            }

            viewModel.PhysiciansSelectList = physicianList;

            // NURSE PRACTITIONERS
            var nurseList = _context.Employees
                .Where(p => p.Role == "Nurse Practitioner")
                .Select(p => new SelectListItem
                {
                    Value = p.EmployeeId.ToString(),
                    Text = p.FullName,
                    Selected = (p.EmployeeId == viewModel.NursePractitionerEmployeeId)
                })
                .ToList();

            if (!nurseList.Any(p => p.Selected))
            {
                nurseList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- None -- ",
                    Selected = true
                });
            }
            else
            {
                nurseList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- None -- "
                });
            }

            viewModel.NursePractitionersSelectList = nurseList;

            // INSURANCES
            var insuranceList = _context.Insurances
                .Select(p => new SelectListItem
                {
                    Value = p.InsuranceId.ToString(),
                    Text = p.CodeAndDescription,
                    Selected = (p.InsuranceId == viewModel.InsuranceId)
                })
                .ToList();

            if (!insuranceList.Any(p => p.Selected))
            {
                insuranceList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select an insurance -- ",
                    Selected = true
                });
            }
            else
            {
                insuranceList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select an insurance -- "
                });
            }

            viewModel.InsurancesSelectList = insuranceList;

            // FACILITIES
            var facilitiesList = _context.Facilities
                .Select(p => new SelectListItem
                {
                    Value = p.FacilityId.ToString(),
                    Text = p.FacilityName,
                    Selected = (p.FacilityId == viewModel.FacilityId)
                })
                .ToList();

            if (!facilitiesList.Any(p => p.Selected))
            {
                facilitiesList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a facility -- ",
                    Selected = true
                });
            }
            else
            {
                facilitiesList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a facility -- "
                });
            }

            viewModel.FacilitiesSelectList = facilitiesList;

            // SERVICE TYPES
            var serviceTypesList = _context.ServiceTypes
                .Select(p => new SelectListItem
                {
                    Value = p.ServiceTypeId.ToString(),
                    Text = p.CodeAndDescription,
                    Selected = (p.ServiceTypeId == viewModel.ServiceTypeId)
                })
                .ToList();

            if (!serviceTypesList.Any(p => p.Selected))
            {
                serviceTypesList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a service type -- ",
                    Selected = true
                });
            }
            else
            {
                serviceTypesList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a service type -- "
                });
            }

            viewModel.ServiceTypesSelectList = serviceTypesList;

            // SCRIBES
            var scribeList = _context.Employees
                .Where(p => p.Role == "Scribe")
                .Select(p => new SelectListItem
                {
                    Value = p.EmployeeId.ToString(),
                    Text = p.FullName,
                    Selected = (p.EmployeeId == viewModel.ScribeEmployeeId)
                })
                .ToList();

            if (!scribeList.Any(p => p.Selected))
            {
                scribeList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- None -- ",
                    Selected = true
                });
            }
            else
            {
                scribeList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- None -- "
                });
            }

            viewModel.ScribesSelectList = scribeList;

            // PATIENTS
            var patientsList = _context.Patients
                .OrderBy(p => p.PatientMrn)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.PatientMrn + " - " + p.PatientFullName,
                    Selected = (p.PatientId == viewModel.PatientId)
                })
                .ToList();

            if (!patientsList.Any(p => p.Selected))
            {
                patientsList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a patient -- ",
                    Selected = true
                });
            }
            else
            {
                patientsList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a patient -- "
                });
            }

            viewModel.PatientsSelectList = patientsList;

            // COSIGNING PHYSICIANS
            var cosigningPhysicianList = _context.Employees
                .Where(p => p.Role == "Physician")
                .Select(p => new SelectListItem
                {
                    Value = p.EmployeeId.ToString(),
                    Text = p.FullName,
                    Selected = (p.EmployeeId == viewModel.CosigningPhysicianEmployeeId)
                })
                .ToList();

            // Only add default if no match was selected
            if (!cosigningPhysicianList.Any(p => p.Selected))
            {
                cosigningPhysicianList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- None -- ",
                    Selected = true
                });
            }
            else
            {
                cosigningPhysicianList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- None -- "
                });
            }

            viewModel.CosigningPhysiciansSelectList = cosigningPhysicianList;
        }

        private void CreateIndexSelectLists(PatientVisitsStagingIndexViewModel viewModel)
        {
            // FACILITIES
            var facilitiesList = _context.Facilities
                .Select(p => new SelectListItem
                {
                    Value = p.FacilityId.ToString(),
                    Text = p.FacilityName,
                    Selected = (p.FacilityId == viewModel.SelectedFacilityId)
                })
                .ToList();
            // Only add default if no match was selected
            if (!facilitiesList.Any(p => p.Selected))
            {
                facilitiesList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a facility -- ",
                    Selected = true
                });
            }
            else
            {
                facilitiesList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a facililty -- "
                });
            }
            viewModel.FacilitiesSelectList = facilitiesList;


            // EMPLOYEES (PROVIDERS)
            var providersList = _context.Employees
                .Where(p => p.IsProvider)
                .Select(p => new SelectListItem
                {
                    Value = p.EmployeeId.ToString(),
                    Text = p.FullName,
                    Selected = (p.EmployeeId == viewModel.SelectedProviderId)
                })
                .ToList();
            // Only add default if no match was selected
            if (!providersList.Any(p => p.Selected))
            {
                providersList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a provider -- ",
                    Selected = true
                });
            }
            else
            {
                providersList.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a provider -- "
                });
            }
            viewModel.ProvidersSelectList = providersList;
        }
    }
}
