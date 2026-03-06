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
    public class VehiclesController : Controller
    {
        private readonly UserContext _context;

        public VehiclesController(UserContext context)
        {
            _context = context;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Vehicle.ToListAsync());
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Vehicle_info")] Vehicle vehicle)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(vehicle);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(vehicle);
        //}

        public async Task<IActionResult> Create([Bind("Vehicle_info")] Vehicle vehicle, int numberOfSeats)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();

                // Create the specified number of seats for this vehicle
                if (numberOfSeats > 0)
                {
                    var seats = new List<Seat>();
                    for (int i = 0; i < numberOfSeats; i++)
                    {
                        seats.Add(new Seat
                        {
                            AssignedVehicleId = vehicle.Id,
                            IsBooked = false
                        });
                    }

                    _context.AddRange(seats);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Vehicle_info")] Vehicle vehicle, int numberOfSeats)
        {
            if (id != vehicle.Id)
            {
                ModelState.AddModelError("numberOfSeats",
                               "Транспорт не найден.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();

                    // Handle seats adjustment
                    var existingSeats = _context.Seat.Where(s => s.AssignedVehicleId == vehicle.Id).ToList();
                    var currentSeatCount = existingSeats.Count;

                    if (numberOfSeats > currentSeatCount)
                    {
                        // Add more seats
                        var seatsToAdd = new List<Seat>();
                        for (int i = 0; i < numberOfSeats - currentSeatCount; i++)
                        {
                            seatsToAdd.Add(new Seat
                            {
                                AssignedVehicleId = vehicle.Id,
                                IsBooked = false
                            });
                        }
                        _context.AddRange(seatsToAdd);
                    }
                    else if (numberOfSeats < currentSeatCount)
                    {
                        var seatsToRemove = currentSeatCount - numberOfSeats;

                        // First, check if we have enough unbooked seats to remove using SQL
                        var checkSeatsSql = @"
                    SELECT COUNT(*) 
                    FROM Seat 
                    WHERE assigned_vehicle = {0} AND is_booked = 0";

                        var unbookedCount = await _context.Database
                            .ExecuteSqlRawAsync(checkSeatsSql, vehicle.Id);

                        // Better to use scalar query for getting count
                        var unbookedSeatsCount = await _context.Seat
                            .FromSqlRaw("SELECT * FROM Seat WHERE assigned_vehicle = {0} AND is_booked = 0", vehicle.Id)
                            .CountAsync();

                        if (unbookedSeatsCount < seatsToRemove)
                        {
                            ModelState.AddModelError("numberOfSeats",
                                $"Cannot remove {seatsToRemove} seat(s). Only {unbookedSeatsCount} unbooked seat(s) available to remove.");
                            vehicle.Seats = existingSeats;
                            return View(vehicle);
                        }


                        // For MySQL:
                        var deleteSql = @"
                             DELETE FROM Seat 
                             WHERE assigned_vehicle = {1} AND is_booked = 0
                             ORDER BY Id
                             LIMIT {0}";

                        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(deleteSql, seatsToRemove, vehicle.Id);

                        TempData["SuccessMessage"] = $"Removed {rowsAffected} seat(s) from the vehicle using SQL.";
                    }

                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    ModelState.AddModelError("numberOfSeats",
               "AAAAAAAA.");
                    if (!VehicleExists(vehicle.Id))
                    {
                        ModelState.AddModelError("numberOfSeats",
                               "Транспорт не найден.");
                        return NotFound();
                    }
                    else { 
                        ModelState.AddModelError("numberOfSeats",
                               "Транспорт не найден.");
                    throw;
                }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicle.Remove(vehicle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(long id)
        {
            return _context.Vehicle.Any(e => e.Id == id);
        }
    }
}
