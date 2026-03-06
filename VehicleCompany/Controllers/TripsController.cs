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

        [HttpGet]
        [Permission("create_trip")]
        public async Task<IActionResult> Create(long? routeId)
        {
            // Create view model
            var viewModel = new TripCreateViewModel();

            // If routeId is provided, pre-select it
            if (routeId.HasValue)
            {
                var route = await _context.Route
                    .Include(r => r.RouteStops)
                    .ThenInclude(rs => rs.Stop)
                    .FirstOrDefaultAsync(r => r.Id == routeId);

                if (route != null)
                {
                    viewModel.RouteId = route.Id;
                    viewModel.RouteInfo = $"{route.Travel_time} мин, {route.Price} ₽";

                    // Get stop names for display
                    var stops = route.RouteStops?
                        .OrderBy(rs => rs.stop_number)
                        .Select(rs => rs.Stop?.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    if (stops != null && stops.Any())
                    {
                        viewModel.RouteStops = string.Join(" → ", stops);
                    }
                }
            }

            // Get ALL vehicles with their seat information
            var allVehicles = await _context.Vehicle
                .Include(v => v.Seats)
                .Select(v => new
                {
                    Vehicle = v,
                    TotalSeats = v.Seats.Count,
                    AvailableSeatsCount = v.Seats.Count(s => !s.IsBooked)
                })
                .ToListAsync();

            // Get IDs of vehicles that are ALREADY assigned to ANY trip
            var assignedVehicleIds = await _context.Trip
                .Select(t => t.Assigned_vehicle)
                .Distinct()
                .ToListAsync();

            // Create view models for ALL vehicles, marking which ones are assigned
            viewModel.AvailableVehicles = allVehicles.Select(v => new AvailableVehicleViewModel
            {
                VehicleId = v.Vehicle.Id,
                VehicleInfo = v.Vehicle.Vehicle_info,
                TotalSeats = v.TotalSeats,
                AvailableSeats = v.AvailableSeatsCount,
                IsAssigned = assignedVehicleIds.Contains(v.Vehicle.Id)
            }).ToList();

            // Add routes to ViewBag for dropdown
            ViewBag.Routes = new SelectList(
                await _context.Route
                    .Include(r => r.RouteStops)
                    .ThenInclude(rs => rs.Stop)
                    .Select(r => new {
                        r.Id,
                        DisplayName = $"{r.Travel_time} мин, {r.Price} ₽ - {string.Join(" → ", r.RouteStops.OrderBy(rs => rs.stop_number).Select(rs => rs.Stop.Name))}"
                    })
                    .ToListAsync(),
                "Id", "DisplayName", viewModel.RouteId);

            return View(viewModel);
        }

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("create_trip")]
        public async Task<IActionResult> Create(TripCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Validate that end time is after start time
                if (viewModel.EndTime <= viewModel.StartTime)
                {
                    ModelState.AddModelError("EndTime", "Время окончания должно быть позже времени начала");

                    // Reload data
                    await LoadVehicleData(viewModel);
                    return View(viewModel);
                }

                // Check if the selected vehicle is already assigned to ANY trip
                if (viewModel.AssignedVehicleId != null)
                {
                    var isVehicleAlreadyAssigned = await _context.Trip
                        .AnyAsync(t => t.Assigned_vehicle == viewModel.AssignedVehicleId
                            && t.Id != viewModel.Id); // Exclude current trip if editing

                    if (isVehicleAlreadyAssigned)
                    {
                        ModelState.AddModelError("AssignedVehicleId", "Это транспортное средство уже назначено на другую поездку");

                        // Reload data
                        await LoadVehicleData(viewModel);
                        return View(viewModel);
                    }
                }

                // Create new trip
                var trip = new Trip
                {
                    Route_id = viewModel.RouteId,
                    Start_time = viewModel.StartTime,
                    End_time = viewModel.EndTime,
                    Assigned_vehicle = viewModel.AssignedVehicleId
                };

                _context.Add(trip);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Рейс успешно создан";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            await LoadVehicleData(viewModel);
            return View(viewModel);
        }

        // Helper method to load vehicle data
        private async Task LoadVehicleData(TripCreateViewModel viewModel)
        {
            // Get ALL vehicles with their seat information
            var allVehicles = await _context.Vehicle
                .Include(v => v.Seats)
                .Select(v => new
                {
                    Vehicle = v,
                    TotalSeats = v.Seats.Count,
                    AvailableSeatsCount = v.Seats.Count(s => !s.IsBooked)
                })
                .ToListAsync();

            // Get IDs of vehicles that are ALREADY assigned to ANY trip
            var assignedVehicleIds = await _context.Trip
                .Select(t => t.Assigned_vehicle)
                .Distinct()
                .ToListAsync();

            // Create view models for ALL vehicles, marking which ones are assigned
            viewModel.AvailableVehicles = allVehicles.Select(v => new AvailableVehicleViewModel
            {
                VehicleId = v.Vehicle.Id,
                VehicleInfo = v.Vehicle.Vehicle_info,
                TotalSeats = v.TotalSeats,
                AvailableSeats = v.AvailableSeatsCount,
                IsAssigned = assignedVehicleIds.Contains(v.Vehicle.Id)
            }).ToList();

            // Refresh routes dropdown
            ViewBag.Routes = new SelectList(
                await _context.Route
                    .Include(r => r.RouteStops)
                    .ThenInclude(rs => rs.Stop)
                    .Select(r => new {
                        r.Id,
                        DisplayName = $"{r.Travel_time} мин, {r.Price} ₽ - {string.Join(" → ", r.RouteStops.OrderBy(rs => rs.stop_number).Select(rs => rs.Stop.Name))}"
                    })
                    .ToListAsync(),
                "Id", "DisplayName", viewModel.RouteId);
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

        public async Task<IActionResult> FinishConfirmed(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
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

                //// Execute stored procedure
                //await _context.Database.ExecuteSqlRawAsync(
                //    "CALL sp_FinishTrip(@p_trip_id, @p_message, @p_bookings_deleted, @p_seats_updated)",
                //    tripIdParam, messageParam, bookingsDeletedParam, seatsUpdatedParam);

                await _context.Database.ExecuteSqlRawAsync(
    "CALL sp_FinishTrip(@p_trip_id)",
    tripIdParam);

                // Get output values
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error finishing trip: {ex.Message}");
                return RedirectToAction(nameof(Index)); 
            }
        }
    }
}
