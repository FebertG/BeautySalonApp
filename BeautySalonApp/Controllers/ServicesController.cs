using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeautySalonApp.Data;
using BeautySalonApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using BeautySalonApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using BeautySalonApp.ViewModels;

namespace BeautySalonApp.Controllers
{
    public class ServicesController : Controller
    {
        private readonly BeautySalonAppDbContext _context;
        private readonly UserManager<UserApp> _userManager;

        public ServicesController(BeautySalonAppDbContext context, UserManager<UserApp> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Services
        public async Task<IActionResult> Index(string serviceName, string sortOrder)
        {
            var services = _context.Service.AsQueryable();

            if (!string.IsNullOrEmpty(serviceName))
            {
                services = services.Where(s => s.ServiceName.Contains(serviceName));
            }


            ViewData["CurrentSort"] = sortOrder;

            switch (sortOrder)
            {
                case "price_desc":
                    services = services.OrderByDescending(s => s.Price);
                    break;
                case "price":
                    services = services.OrderBy(s => s.Price);
                    break;
                case "duration_desc":
                    services = services.OrderByDescending(s => s.DurationMin);
                    break;
                case "duration":
                    services = services.OrderBy(s => s.DurationMin);
                    break;
                case "name_desc":
                    services = services.OrderByDescending(s => s.ServiceName);
                    break;
                case "name":
                    services = services.OrderBy(s => s.ServiceName);
                    break;
                default:
                    services = services.OrderBy(s => s.Price);
                    break;
            }

            return View(await services.ToListAsync());
        }

        public async Task<IActionResult> AllServices(string serviceName, string sortOrder)
        {
            var services = _context.Service.Include(s => s.Salon).AsQueryable();

            if (!string.IsNullOrEmpty(serviceName))
            {
                services = services.Where(s => s.ServiceName.Contains(serviceName));
            }

            ViewData["CurrentSort"] = sortOrder;

            switch (sortOrder)
            {
                case "price_desc":
                    services = services.OrderByDescending(s => s.Price);
                    break;
                case "price":
                    services = services.OrderBy(s => s.Price);
                    break;
                case "duration_desc":
                    services = services.OrderByDescending(s => s.DurationMin);
                    break;
                case "duration":
                    services = services.OrderBy(s => s.DurationMin);
                    break;
                case "name_desc":
                    services = services.OrderByDescending(s => s.ServiceName);
                    break;
                case "name":
                    services = services.OrderBy(s => s.ServiceName);
                    break;
                default:
                    services = services.OrderBy(s => s.Price);
                    break;
            }

            return View(await services.ToListAsync());
        }



        // GET: Services/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Service == null)
            {
                return NotFound();
            }

            var service = await _context.Service
                .Include(s => s.Employee)
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            var viewModel = new ServiceViewModel
            {
                Service = service,
                Employee = service.Employee,
                Salon = service.Salon
            };

            return View(viewModel);
        }


        // GET: Services/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var employees = _context.Employee.AsQueryable();

            employees = employees.Where(e => e.UserId == currentUser.Id);

            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name");

            ViewData["User"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(ViewData["User"]);

            return View();
        }


        // POST: Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ServiceName,DurationMin,Price,Description,EmployeeId")] Service service)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            service.UserId = currentUser.Id;


            service.UserApp = await _userManager.GetUserAsync(User);
           ModelState.Clear();
            if (ModelState.IsValid)
            {
                var employeeExists = _context.Employee.Any(e => e.Id == service.EmployeeId);
                if (!employeeExists)
                {
                    ModelState.AddModelError("EmployeeId", "Wybrany pracownik nie istnieje.");
                    return View(service);
                }

                _context.Add(service);
                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(User);

                var userSalon = await _context.Salon.FirstOrDefaultAsync(s => s.UserId == user.Id);

                userSalon.Services.Add(service);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Services/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Service == null)
            {
                return NotFound();
            }

            var service = await _context.Service
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var employees = _context.Employee.AsQueryable();

            employees = employees.Where(e => e.UserId == currentUser.Id);

            ViewData["EmployeeId"] = new SelectList(employees, "Id", "Name");

            if (service.UserApp != currentUser)
            {
                return Forbid();
            }

            ViewData["User"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(ViewData["User"]);

            return View(service);
        }



        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ServiceName,DurationMin,Price,Description")] Service serviceModel)
        {
            if (id != serviceModel.Id)
            {
                return NotFound();
            }


            serviceModel.UserApp = await _userManager.GetUserAsync(User);
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var serviceToUpdate = await _context.Service.FindAsync(id);
                    if (serviceToUpdate.UserApp != currentUser)
                    {
                        return Forbid();
                    }

                    serviceToUpdate.ServiceName = serviceModel.ServiceName;
                    serviceToUpdate.DurationMin = serviceModel.DurationMin;
                    serviceToUpdate.Price = serviceModel.Price;
                    serviceToUpdate.Description = serviceModel.Description;

                    _context.Update(serviceToUpdate);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(serviceModel.Id))
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
            return View(serviceModel);
        }


        // GET: Services/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Service.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (service.UserApp != currentUser)
            {
                return Forbid();
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var service = await _context.Service.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (service.UserApp != currentUser)
            {
                return Forbid();
            }

            _context.Service.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(long id)
        {
          return (_context.Service?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
