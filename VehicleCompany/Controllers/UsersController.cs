using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleCompany.Attributes;
using VehicleCompany.Contexts;
using VehicleCompany.Models;

namespace VehicleCompany.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserContext _context;

        public UsersController(UserContext context)
        {
            _context = context;
        }

        // GET: Users
        [Permission("edit_user")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        [Permission("edit_user")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [Permission("create_user")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("create_user")]
        public async Task<IActionResult> Create([Bind("Id,UserName,Password,Email")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();

                var user_role = new UserRole();
                user_role.UserId = user.Id;

                //Нада чёнить по умнее придумать
                user_role.RoleId = 2;

                _context.Add(user_role);

                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [Permission("edit_user")]
        [HttpGet]
        public async Task<IActionResult> Edit(long? id)
        {

            var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var allRoles = await _context.Roles.ToListAsync();
            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();

            var vm = new EditUserRolesViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Password = user.Password,
                Roles = allRoles
                    .Select(r => new RoleSelectionViewModel
                    {
                        RoleId = r.Id,
                        RoleName = r.RoleName,
                        IsSelected = userRoleIds.Contains(r.Id)
                    })
                    .ToList()
            };

            return View(vm);
        }

        [Permission("edit_user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserRolesViewModel model)
        {


            if (ModelState.IsValid)
            {
                try
                {
                    var user_part1 = new User();

                    user_part1.UserName = model.UserName;
                    user_part1.Password = model.Password;
                    user_part1.Email = model.Email;
                    user_part1.Id = model.Id;

                    _context.Update(user_part1);
                    await _context.SaveChangesAsync();


                    var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == model.Id);

                    if (user == null)
                    {
                        return NotFound();
                    }


                    _context.UserRoles.RemoveRange(user.UserRoles);

                    var selectedRoleIds = model.Roles
                        .Where(r => r.IsSelected)
                        .Select(r => r.RoleId)
                        .ToList();

                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = model.SelectedRoleId
                    });

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }



                }
            }


            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5
        [Permission("delete_user")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission("delete_user")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
