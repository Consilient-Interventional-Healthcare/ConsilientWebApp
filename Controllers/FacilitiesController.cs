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
    public class FacilitiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ConsilientContext _context;

        public FacilitiesController(ConsilientContext context,
                                    IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Facilities
        public async Task<IActionResult> Index()
        {
            var facilities = await _context.Facilities.ToListAsync();

            var viewModel = _mapper.Map<List<FacilityViewModel>>(facilities);
            return View(viewModel);
        }

        // GET: Facilities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(m => m.FacilityId == id);
            if (facility == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<FacilityViewModel>(facility);
            return View(viewModel);
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
            if (ModelState.IsValid)
            {
                var facility = _mapper.Map<Facility>(viewModel);
                _context.Add(facility);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Facilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<FacilityViewModel>(facility);
            return View(viewModel);
        }

        // POST: Facilities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacilityViewModel viewModel)
        {
            if (id != viewModel.FacilityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var facility = _mapper.Map<Facility>(viewModel);
                    _context.Update(facility);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacilityExists(viewModel.FacilityId))
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
            return View(viewModel);
        }

        // GET: Facilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(m => m.FacilityId == id);
            if (facility == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<FacilityViewModel>(facility);
            return View(viewModel);
        }

        // POST: Facilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null)
            {
                _context.Facilities.Remove(facility);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacilityExists(int id)
        {
            return _context.Facilities.Any(e => e.FacilityId == id);
        }
    }
}
