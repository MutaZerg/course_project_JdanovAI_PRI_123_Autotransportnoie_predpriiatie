using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleCompany.Contexts;
using VehicleCompany.Models;
using Route = VehicleCompany.Models.Route;
using Microsoft.AspNetCore.Authorization;

namespace VehicleCompany.Controllers
{
    [Authorize]
    public class RoutesController : Controller
    {
        private readonly UserContext _context;

        public RoutesController(UserContext context)
        {
            _context = context;
        }

        // GET: Routes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Route.ToListAsync());
        }

        // GET: Routes/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Route
                .Include(r => r.RouteStops)
                .ThenInclude(rs => rs.Stop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        /// <summary>Book a trip: shows all trips on this route and available seats per trip.</summary>
        [HttpGet]
        public async Task<IActionResult> BookTrip(long? id)
        {
            if (id == null) return NotFound();
            var route = await _context.Route
                .Include(r => r.RouteStops)
                .ThenInclude(rs => rs.Stop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (route == null) return NotFound();

            var trips = await _context.Trip
                .Where(t => t.Route_id == id.Value)
                .OrderBy(t => t.Start_time)
                .ToListAsync();

            var vm = new BookTripViewModel { Route = route };
            foreach (var trip in trips)
            {
                var seats = await _context.Seat
                    .Where(s => s.AssignedVehicleId == trip.Assigned_vehicle && !s.IsBooked)
                    .OrderBy(s => s.Id)
                    .ToListAsync();
                vm.TripsWithSeats.Add(new TripSeatsViewModel { Trip = trip, AvailableSeats = seats });
            }

            return View(vm);
        }

        // GET: Routes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Routes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Travel_time,Price")] Route route)
        {
            if (ModelState.IsValid)
            {
                _context.Add(route);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(route);
        }

        // GET: Routes/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Route.FindAsync(id);
            if (route == null)
            {
                return NotFound();
            }
            return View(route);
        }

        // POST: Routes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Travel_time,Price")] Route route)
        {
            if (id != route.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(route);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteExists(route.Id))
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
            return View(route);
        }

        // GET: Routes/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Route
                .FirstOrDefaultAsync(m => m.Id == id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // POST: Routes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var route = await _context.Route.FindAsync(id);
            if (route != null)
            {
                _context.Route.Remove(route);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RouteExists(long id)
        {
            return _context.Route.Any(e => e.Id == id);
        }
    }
}
