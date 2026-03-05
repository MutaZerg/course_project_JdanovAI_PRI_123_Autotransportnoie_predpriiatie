using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleCompany.Attributes;
using VehicleCompany.Contexts;
using VehicleCompany.Models;
using Route = VehicleCompany.Models.Route;

namespace VehicleCompany.Controllers
{

    public class RoutesController : Controller
    {
        private readonly UserContext _context;

        public RoutesController(UserContext context)
        {
            _context = context;
        }

        // GET: Routes
        public async Task<IActionResult> Index(string sortOrder,
            string sortBy,
            string searchString,
            int? pageNumber,
            int pageSize = 10)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PageNumber = pageNumber ?? 1;

            var routes = _context.Route
                .Include(r => r.RouteStops)
                    .ThenInclude(rs => rs.Stop)
                .AsQueryable();

            // Поиск по названию остановки
            if (!string.IsNullOrEmpty(searchString))
            {
                routes = routes.Where(r => r.RouteStops
                    .Any(rs => rs.Stop.Name.Contains(searchString)));
            }

            // Сортировка
            routes = sortBy switch
            {
                "travel_time" => sortOrder == "asc"
                    ? routes.OrderBy(r => r.Travel_time)
                    : routes.OrderByDescending(r => r.Travel_time),
                "price" => sortOrder == "asc"
                    ? routes.OrderBy(r => r.Price)
                    : routes.OrderByDescending(r => r.Price),
                "stops" => sortOrder == "asc"
                    ? routes.OrderBy(r => r.RouteStops.Count)
                    : routes.OrderByDescending(r => r.RouteStops.Count),
                _ => routes.OrderBy(r => r.Id)
            };

            // Пагинация
            var totalItems = await routes.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await routes
                .Skip(((pageNumber ?? 1) - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
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
        [Authorize]
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
        [Authorize]
        [Permission("create_route")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Routes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Permission("create_route")]

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
        [Authorize]
        [Permission("edit_route")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var route = await _context.Route.FindAsync(id);
            if (route.RouteStops != null)
            {
                route.RouteStops = route.RouteStops
                    .OrderBy(rs => rs.stop_number)
                    .ToList();
            }
            var availableStops = await _context.Stop
                .OrderBy(s => s.Name)
                .ToListAsync();

            // Получаем ID выбранных остановок в правильном порядке
            var selectedStopIds = route.RouteStops?
                .OrderBy(rs => rs.stop_number)
                .Select(rs => rs.StopId)
                .ToList() ?? new List<long>();

            ViewBag.AvailableStops = availableStops;
            ViewBag.SelectedStopIds = selectedStopIds;

            
            if (route == null)
            {
                return NotFound();
            }
            return View(route);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Permission("edit_route")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Travel_time,Price")] Route route, Dictionary<int, long> SelectedStops)
        {
            if (id != route.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update route basic info
                    _context.Update(route);

                    // Remove existing route stops
                    var existingStops = _context.RouteStop.Where(rs => rs.RouteId == id);
                    _context.RouteStop.RemoveRange(existingStops);

                    // Add new route stops in order
                    if (SelectedStops != null && SelectedStops.Any())
                    {
                        foreach (var item in SelectedStops.OrderBy(kv => kv.Key))
                        {
                            _context.RouteStop.Add(new RouteStop
                            {
                                RouteId = id,
                                StopId = item.Value,
                                stop_number = item.Key // Сохраняем порядковый номер
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Route updated successfully!";
                    return RedirectToAction(nameof(Index));
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
            }

            // If we got this far, something failed, redisplay form
            ViewBag.AvailableStops = await _context.Stop.OrderBy(s => s.Name).ToListAsync();
            return View(route);
        }



        // GET: Routes/Delete/5
        [Authorize]
        [Permission("delete_route")]
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
        [Authorize]
        [Permission("delete_route")]
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

        public IActionResult SearchRoutes(String first_stop, String second_stop)
        {
            var search_results = new List<Route>();
            if (first_stop != null && second_stop != null)
            {
                var query = @"
                SELECT 
                r.id as RouteId,
                r.travel_time as Travel_time,
                r.price as Price
                FROM Route r
                INNER JOIN Route_stop rs ON rs.route_id = r.id
                INNER JOIN Stop s ON s.id = rs.stop_id AND s.name = "
                + @first_stop + 
                "INNER JOIN Route_stop rs2 ON rs2.route_id = r.id INNER JOIN Stop s2 ON s2.id = rs2.stop_id AND s2.name ="
                + @second_stop +
                "WHERE rs.stop_number < rs2.stop_number";

                search_results = _context.Database
                    .SqlQueryRaw<Route>(query)
                    .ToList();
            }
            else
            {
                search_results = _context.Database
                    .SqlQueryRaw<Route>(@"Select * from Route")
                    .ToList();
            }
            return View(search_results);
        }
    }
}
