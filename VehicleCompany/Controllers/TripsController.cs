using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VehicleCompany.Attributes;
using VehicleCompany.Contexts;
using VehicleCompany.Models;
using MySqlConnector;

namespace VehicleCompany.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly UserContext _context;

        public TripsController(UserContext context)
        {
            _context = context;
        }

        // GET: Trips
        [Permission("view_trips")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Trip.ToListAsync());
        }

        // GET: Trips/Details/5
        [Permission("view_trips")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }
        [Permission("create_trip")]
        // GET: Trips/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Trips/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("create_trip")]
        public async Task<IActionResult> Create([Bind("Id,Route_id,Start_time,End_time,Assigned_vehicle")] Trip trip)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trip);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trip);
        }

        // GET: Trips/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }
            return View(trip);
        }

        // POST: Trips/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("edit_trip")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Route_id,Start_time,End_time,Assigned_vehicle")] Trip trip)
        {
            if (id != trip.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TripExists(trip.Id))
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
            return View(trip);
        }

        // GET: Trips/Delete/5
        [Permission("delete_trip")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // POST: Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [Permission("delete_trip")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var trip = await _context.Trip.FindAsync(id);
            if (trip != null)
            {
                _context.Trip.Remove(trip);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TripExists(long id)
        {
            return _context.Trip.Any(e => e.Id == id);
        }


        // POST: Trips/Finish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Finish_trip")]
        public async Task<IActionResult> FinishConfirmed(long id)
        {
            try
            {
                // Parameters for stored procedure
                var tripIdParam = new MySqlParameter("@p_trip_id", id);

                var messageParam = new MySqlParameter("@p_message", MySqlDbType.VarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };

                var bookingsDeletedParam = new MySqlParameter("@p_bookings_deleted", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                var seatsUpdatedParam = new MySqlParameter("@p_seats_updated", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                // Execute stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL sp_FinishTrip(@p_trip_id, @p_seats_updated, @p_bookings_deleted, @p_message)",
                    tripIdParam, seatsUpdatedParam, bookingsDeletedParam, messageParam);

                // Get output values
                string message = messageParam.Value?.ToString() ?? "Trip finished";
                int bookingsDeleted = Convert.ToInt32(bookingsDeletedParam.Value);
                int seatsUpdated = Convert.ToInt32(seatsUpdatedParam.Value);

                if (message == "Trip finished successfully. ")
                {
                    TempData["SuccessMessage"] = message;
                    TempData["BookingsDeleted"] = bookingsDeleted;
                    TempData["SeatsCleared"] = seatsUpdated;
                    return View(await _context.Trip.ToListAsync());
                }
                else
                {
                    ModelState.AddModelError("", message);
                    return View(await _context.Trip.ToListAsync());
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error finishing trip: {ex.Message}");
                return View(await _context.Trip.ToListAsync());
            }
        }
    }
}
