using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleCompany.Contexts;
using VehicleCompany.Models;
using Microsoft.AspNetCore.Authorization;

namespace VehicleCompany.Controllers
{
    [Authorize]
    public class StopsController : Controller
    {
        private readonly UserContext _context;

        public StopsController(UserContext context)
        {
            _context = context;
        }

        // GET: Stops
        public async Task<IActionResult> Index()
        {
            return View(await _context.Stop.ToListAsync());
        }

        // GET: Stops/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stop = await _context.Stop
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stop == null)
            {
                return NotFound();
            }

            return View(stop);
        }

        // GET: Stops/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Stops/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Stop stop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(stop);
        }

        // GET: Stops/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stop = await _context.Stop.FindAsync(id);
            if (stop == null)
            {
                return NotFound();
            }
            return View(stop);
        }

        // POST: Stops/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name")] Stop stop)
        {
            if (id != stop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StopExists(stop.Id))
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
            return View(stop);
        }

        // GET: Stops/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stop = await _context.Stop
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stop == null)
            {
                return NotFound();
            }

            return View(stop);
        }

        // POST: Stops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var stop = await _context.Stop.FindAsync(id);
            if (stop != null)
            {
                _context.Stop.Remove(stop);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StopExists(long id)
        {
            return _context.Stop.Any(e => e.Id == id);
        }
    }
}
