using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleCompany.Contexts;
using VehicleCompany.Models;
using Microsoft.AspNetCore.Authorization;

namespace VehicleCompany.Controllers
{
    [Authorize]
    public class SeatsController : Controller
    {
        private readonly UserContext _context;

        public SeatsController(UserContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var seats = await _context.Seat
                .Include(s => s.AssignedVehicle)
                .OrderBy(s => s.AssignedVehicleId)
                .ThenBy(s => s.Id)
                .ToListAsync();
            return View(seats);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();
            var seat = await _context.Seat
                .Include(s => s.AssignedVehicle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (seat == null) return NotFound();
            return View(seat);
        }

        public IActionResult Create()
        {
            ViewData["AssignedVehicleId"] = new SelectList(_context.Vehicle, "Id", "Vehicle_info");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignedVehicleId,IsBooked")] Seat seat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedVehicleId"] = new SelectList(_context.Vehicle, "Id", "Vehicle_info", seat.AssignedVehicleId);
            return View(seat);
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();
            var seat = await _context.Seat.FindAsync(id);
            if (seat == null) return NotFound();
            ViewData["AssignedVehicleId"] = new SelectList(_context.Vehicle, "Id", "Vehicle_info", seat.AssignedVehicleId);
            return View(seat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,AssignedVehicleId,IsBooked")] Seat seat)
        {
            if (id != seat.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeatExists(seat.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedVehicleId"] = new SelectList(_context.Vehicle, "Id", "Vehicle_info", seat.AssignedVehicleId);
            return View(seat);
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();
            var seat = await _context.Seat.Include(s => s.AssignedVehicle).FirstOrDefaultAsync(m => m.Id == id);
            if (seat == null) return NotFound();
            return View(seat);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var seat = await _context.Seat.FindAsync(id);
            if (seat != null) _context.Seat.Remove(seat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeatExists(long id) => _context.Seat.Any(e => e.Id == id);
    }
}
