using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.Constants;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Consilient.WebApp.Infra;
using Consilient.WebApp.ViewModels;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Consilient.WebApp.Controllers
{
    [Authorize]
    public class PatientVisitsStagingController(IConsilientApiClient apiClient, ICurrentUserService currentUserService) : Controller
    {
        // GET: PatientVisitStaging
        public async Task<IActionResult> Index(DateOnly? selectedDate,
                                                int? selectedFacilityId,
                                                int? selectedProviderId)
        {
            DateOnly? sDate;
            if (!selectedDate.HasValue)
            {
                var sessionSelectedDate = HttpContext.Session.GetString("SelectedDate");
                sDate = !string.IsNullOrEmpty(sessionSelectedDate) ? DateOnly.FromDateTime(Convert.ToDateTime(sessionSelectedDate)) : DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            }
            else
            {
                sDate = selectedDate.Value;
            }


            HttpContext.Session.SetString("SelectedDate", sDate.Value.ToString("yyyy-MM-dd"));

            var predicate = PredicateBuilder.New<PatientVisitsStagingViewModel>(v => !v.AddedToMainTable);

            if (selectedFacilityId.HasValue)
            {
                predicate = predicate.And(v => v.FacilityId == selectedFacilityId.Value);
            }

            if (selectedProviderId.HasValue)
            {
                var providerId = selectedProviderId.Value;
                predicate = predicate.And(v => v.PhysicianEmployeeId == providerId || v.NursePractitionerEmployeeId == providerId);
            }

            ViewBag.SelectedDate = selectedDate;
            ViewBag.SelectedFacility = selectedFacilityId;
            ViewBag.SelectedProvider = selectedProviderId;

            // API returns IEnumerable<T>, so compile the expression to Func<T,bool> before applying Where
            var query = $@"{sDate.Value}";
            var patientVisitsStaging = (await apiClient.GraphQl.Query<PatientVisitsStagingViewModel>(query)).Unwrap()!
                .Where(predicate.Compile())
                .ToList();

            var viewModel = new PatientVisitsStagingIndexViewModel
            {
                PatientVisitsStaging = [.. patientVisitsStaging],
                SelectedDate = sDate.Value,
                SelectedFacilityId = selectedFacilityId ?? 0,
                SelectedProviderId = selectedProviderId ?? 0
            };

            await CreateIndexSelectLists(viewModel);

            foreach (var visit in viewModel.PatientVisitsStaging)
            {
                var physicianName = visit.PhysicianEmployee?.FullName ?? "Unknown Physician";
                if (viewModel.PhysicianSummaries.TryGetValue(physicianName, out var value))
                {
                    viewModel.PhysicianSummaries[physicianName] = ++value;
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
                        if (viewModel.NursePractitionerSummaries.TryGetValue(npName, out var nursePractitionerSummaries))
                        {
                            viewModel.NursePractitionerSummaries[npName] = ++nursePractitionerSummaries;
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
                        if (viewModel.ScribeSummaries.TryGetValue(scribeName, out var scribeNamevalue))
                        {
                            viewModel.ScribeSummaries[scribeName] = ++scribeNamevalue;
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

        // GET: PatientVisitsStaging/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var patientVisitStaging = (await apiClient.StagingPatientVisits.GetByIdAsync(id)).Unwrap();
            if (patientVisitStaging == null)
            {
                return NotFound();
            }
            return View(patientVisitStaging);
        }

        // GET: PatientVisitsStaging/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new PatientVisitsStagingViewModel();
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SelectedDate")))
            {
                viewModel.DateServiced = DateOnly.FromDateTime(Convert.ToDateTime(HttpContext.Session.GetString("SelectedDate")));
            }

            await CreateSelectLists(viewModel);
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
                await CreateSelectLists(viewModel);
                return View(viewModel);
            }

            int patientId;

            if ((viewModel.ServiceTypeId ?? 0) == 0)
            {
                ModelState.AddModelError("ServiceTypeId", "The Service Type field is required.");
                await CreateSelectLists(viewModel);
                return View(viewModel);
            }

            if (patientOption == "existing" && viewModel.PatientId != null)
            {
                patientId = viewModel.PatientId.Value;
            }
            else
            {
                var patient = (await apiClient.Patients.GetByMrnAsync(viewModel.NewPatient.PatientMrn)).Unwrap();
                if (patient != null)
                {
                    TempData["ErrorMessage"] = $"Patient with MRN {viewModel.NewPatient.PatientMrn} already exists. Please select the patient from the dropdown.";
                    viewModel.NewPatient = new PatientViewModel();
                    viewModel.Patient.PatientMrn = patient.PatientMrn;
                    await CreateSelectLists(viewModel);
                    return View(viewModel);
                }

                if (string.IsNullOrEmpty(viewModel.NewPatient.PatientFirstName) || string.IsNullOrEmpty(viewModel.NewPatient.PatientLastName))
                {
                    TempData["ErrorMessage"] = "First Name and Last Name are required for new patients.";
                    return View(viewModel);
                }
                var newPatient = (await apiClient.Patients.CreateAsync(new CreatePatientRequest
                {
                    PatientMrn = viewModel.NewPatient.PatientMrn,
                    PatientFirstName = viewModel.NewPatient.PatientFirstName,
                    PatientLastName = viewModel.NewPatient.PatientLastName,
                    PatientBirthDate = viewModel.NewPatient.PatientBirthDate
                })).Unwrap()!;
                patientId = newPatient.PatientId;
            }


            await apiClient.StagingPatientVisits.CreateAsync(new CreateStagingPatientVisitRequest
            {
                PatientId = patientId,
                AddedToMainTable = viewModel.AddedToMainTable,
                AdmissionNumber = viewModel.AdmissionNumber,
                CosigningPhysicianEmployeeId = viewModel.CosigningPhysicianEmployeeId,
                DateServiced = viewModel.DateServiced,
                FacilityId = viewModel.FacilityId,
                InsuranceId = viewModel.InsuranceId,
                IsScribeServiceOnly = viewModel.IsScribeServiceOnly,
                NursePractitionerEmployeeId = viewModel.NursePractitionerEmployeeId
            });
            return RedirectToAction(nameof(Index));
        }

        // GET: PatientVisitsStaging/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var patientVisitStaging = (await apiClient.StagingPatientVisits.GetByIdAsync(id)).Unwrap();
            if (patientVisitStaging == null)
            {
                return NotFound();
            }
            var viewModel = new PatientVisitsStagingViewModel
            {
                AddedToMainTable = patientVisitStaging.AddedToMainTable,
                AdmissionNumber = patientVisitStaging.AdmissionNumber,
                CosigningPhysicianEmployeeId = patientVisitStaging.CosigningPhysicianEmployeeId,
                DateServiced = patientVisitStaging.DateServiced,
                FacilityId = patientVisitStaging.FacilityId,
                InsuranceId = patientVisitStaging.InsuranceId,
                IsScribeServiceOnly = patientVisitStaging.IsScribeServiceOnly,
                NursePractitionerEmployeeId = patientVisitStaging.NursePractitionerEmployeeId,
                PatientVisitStagingId = patientVisitStaging.PatientVisitStagingId,
                PatientId = patientVisitStaging.PatientId,
                PhysicianEmployeeId = patientVisitStaging.PhysicianEmployeeId,
                ScribeEmployeeId = patientVisitStaging.ScribeEmployeeId,
                ServiceTypeId = patientVisitStaging.ServiceTypeId
            };
            await CreateSelectLists(viewModel);
            return View(patientVisitStaging);
        }

        // POST: PatientVisitsStaging/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PatientVisitsStagingViewModel viewModel, string patientOption)
        {
            if (!ModelState.IsValid)
            {
                await CreateSelectLists(viewModel);
                return View(viewModel);
            }
            //int patientId;

            if (!(patientOption == "existing" && viewModel.PatientId != null))
            {
                _ = (await apiClient.Patients.CreateAsync(new CreatePatientRequest
                {
                    PatientMrn = viewModel.NewPatient.PatientMrn,
                    PatientFirstName = viewModel.NewPatient.PatientFirstName,
                    PatientLastName = viewModel.NewPatient.PatientLastName,
                    PatientBirthDate = viewModel.NewPatient.PatientBirthDate
                })).Unwrap()!;
                //patientId = newPatient.PatientId;
            }
            else
            {
                //patientId = viewModel.PatientId.Value;
            }

            _ = (await apiClient.StagingPatientVisits.UpdateAsync(viewModel.PatientVisitStagingId, new UpdateStagingPatientVisitRequest
            {
                CosigningPhysicianEmployeeId = viewModel.CosigningPhysicianEmployeeId,
                //DateServiced = viewModel.DateServiced,
                FacilityId = viewModel.FacilityId,
                InsuranceId = viewModel.InsuranceId,
                IsScribeServiceOnly = viewModel.IsScribeServiceOnly,
                NursePractitionerEmployeeId = viewModel.NursePractitionerEmployeeId,
                PhysicianEmployeeId = viewModel.PhysicianEmployeeId,
                ScribeEmployeeId = viewModel.ScribeEmployeeId
                //ServiceTypeId = viewModel.ServiceTypeId,
            })).Unwrap();
            return RedirectToAction(nameof(Index));

        }

        // GET: PatientVisitsStaging/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientVisitStaging = (await apiClient.StagingPatientVisits.GetByIdAsync(id.Value)).Unwrap();
            if (patientVisitStaging == null)
            {
                return NotFound();
            }

            return View(patientVisitStaging);
        }

        // POST: PatientVisitsStaging/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = (await apiClient.StagingPatientVisits.DeleteAsync(id)).Unwrap();
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SaveApprovals(PatientVisitsStagingIndexViewModel viewModel)
        {
            foreach (var visit in viewModel.PatientVisitsStaging)
            {
                (await apiClient.StagingPatientVisits.UpdateAsync(visit.PatientVisitStagingId, new UpdateStagingPatientVisitRequest
                {
                    PhysicianApproved = visit.PhysicianApproved,
                    NursePractitionerApproved = visit.NursePractitionerApproved,
                    PhysicianApprovedBy = currentUserService.UserId,
                    PhysicianApprovedDateTime = DateTime.Now
                })).Unwrap();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ApproveAllByDay(DateOnly selectedDate)
        {
            var visits = (await apiClient.StagingPatientVisits.GetByDateAsync(selectedDate)).Unwrap()!
                .Where(p => p.DateServiced == selectedDate && !p.AddedToMainTable)
                .ToList();

            foreach (var visit in visits)
            {
                (await apiClient.StagingPatientVisits.UpdateAsync(visit.PatientVisitStagingId, new UpdateStagingPatientVisitRequest
                {
                    PhysicianApproved = true,
                    NursePractitionerApproved = true,
                    PhysicianApprovedBy = currentUserService.UserId,
                    PhysicianApprovedDateTime = DateTime.Now
                })).Unwrap();
            }

            return RedirectToAction(nameof(Index), new { selectedDate });
        }

        [HttpPost]
        public async Task<IActionResult> PushApprovedPatientVisits()
        {
            var approvedPatientVisitsStagingCount = (await apiClient.StagingPatientVisits.PushApprovedPatientVisitsAsync()).Unwrap();
            if (approvedPatientVisitsStagingCount == 0)
            {
                TempData["ErrorMessage"] = "No approved patient encounters to push.";
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = "Approved patient encounters have been pushed successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UploadSpreadsheet(IFormFile spreadsheet)
        {
            if (spreadsheet.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid spreadsheet file.";
                return RedirectToAction(nameof(Index));
            }

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await spreadsheet.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Construct the API model file. Fully qualified to avoid needing an extra using.
            var apiFile = new Api.Client.Models.File(fileBytes, spreadsheet.ContentType, spreadsheet.FileName);
            var result = (await apiClient.StagingPatientVisits.UploadSpreadsheetAsync(apiFile)).Unwrap()!;
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }




        private async Task CreateSelectLists(PatientVisitsStagingViewModel viewModel)
        {
            var employees = (await apiClient.Employees.GetAllAsync()).Unwrap()!.ToList();

            // PHYSICIANS
            var physicianList = employees
                .Where(p => p.Role == ApplicationConstants.Roles.Physician)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName,
                    Selected = p.Id == viewModel.PhysicianEmployeeId
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
            var nurseList = employees.Where(p => p.Role == ApplicationConstants.Roles.NursePracticioner)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName,
                    Selected = p.Id == viewModel.NursePractitionerEmployeeId
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
            //var insurances = (await apiClient.Insurances.GetAllAsync()).Unwrap()!;
            //var insuranceList = insurances.Select(p => new SelectListItem
            //{
            //    Value = p.InsuranceId.ToString(),
            //    Text = p.CodeAndDescription,
            //    Selected = p.InsuranceId == viewModel.InsuranceId
            //})
            //    .ToList();

            //if (!insuranceList.Any(p => p.Selected))
            //{
            //    insuranceList.Insert(0, new SelectListItem
            //    {
            //        Value = "",
            //        Text = "-- Select an insurance -- ",
            //        Selected = true
            //    });
            //}
            //else
            //{
            //    insuranceList.Insert(0, new SelectListItem
            //    {
            //        Value = "",
            //        Text = "-- Select an insurance -- "
            //    });
            //}

            //viewModel.InsurancesSelectList = insuranceList;

            // FACILITIES
            var facilities = (await apiClient.Facilities.GetAllAsync()).Unwrap()!;
            var facilitiesList = facilities
                .Select(p => new SelectListItem
                {
                    Value = p.FacilityId.ToString(),
                    Text = p.FacilityName,
                    Selected = p.FacilityId == viewModel.FacilityId
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
            var serviceTypes = (await apiClient.ServiceTypes.GetAllAsync()).Unwrap()!;
            var serviceTypesList = serviceTypes
                .Select(p => new SelectListItem
                {
                    Value = p.ServiceTypeId.ToString(),
                    Text = p.CodeAndDescription,
                    Selected = p.ServiceTypeId == viewModel.ServiceTypeId
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
            var scribeList = employees.Where(p => p.Role == ApplicationConstants.Roles.Scribe)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName,
                    Selected = p.Id == viewModel.ScribeEmployeeId
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
            var patients = (await apiClient.Patients.GetAllAsync()).Unwrap()!;
            var patientsList = patients
                .OrderBy(p => p.PatientMrn)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.PatientMrn + " - " + p.PatientFullName,
                    Selected = p.PatientId == viewModel.PatientId
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
            var cosigningPhysicianList = employees.Where(p => p.Role == ApplicationConstants.Roles.Physician)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName,
                    Selected = p.Id == viewModel.CosigningPhysicianEmployeeId
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

        private async Task CreateIndexSelectLists(PatientVisitsStagingIndexViewModel viewModel)
        {
            // FACILITIES
            var facilities = (await apiClient.Facilities.GetAllAsync()).Unwrap()!;
            var facilitiesList = facilities
                .Select(p => new SelectListItem
                {
                    Value = p.FacilityId.ToString(),
                    Text = p.FacilityName,
                    Selected = p.FacilityId == viewModel.SelectedFacilityId
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
            var providersList = (await apiClient.Employees.GetAllAsync()).Unwrap()!
                .Where(p => p.IsProvider)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName,
                    Selected = p.Id == viewModel.SelectedProviderId
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
