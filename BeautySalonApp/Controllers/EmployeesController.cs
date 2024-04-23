using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeautySalonApp.Data;
using BeautySalonApp.Models;
using BeautySalonApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BeautySalonApp.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly BeautySalonAppDbContext _context;
        private readonly UserManager<UserApp> _userManager;

        public EmployeesController(BeautySalonAppDbContext context, UserManager<UserApp> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchName, string searchSurname)
        {
            var employees = _context.Employee.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                employees = employees.Where(e => e.Name.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchSurname))
            {
                employees = employees.Where(e => e.Surname.Contains(searchSurname));
            }

            return View(await employees.ToListAsync());
        }


        // GET: Employees/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Employee == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                        .Include(e => e.WorkingDate)
                        .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["User"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(ViewData["User"]);
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Surname,Description,UserId")] Employee employee, List<string> workingDays, string startOfWork, string endOfWork)
        {
            employee.UserApp = _context.Users.Find(employee.UserId);
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    employee.UserId = user.Id;
                }

                if (workingDays != null && workingDays.Any())
                {
                    foreach (var day in workingDays)
                    {
                        if (Enum.TryParse(day, out DayOfWeek weekDay))
                        {
                            var workingDate = new EmployeesWorkingDate
                            {
                                WorkDay = weekDay,
                                StartOfWork = TimeSpan.Parse(startOfWork),
                                EndOfWork = TimeSpan.Parse(endOfWork),
                                EmployeeId = employee.Id
                            };
                            employee.WorkingDate.Add(workingDate);
                        }
                    }
                }

                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Employee == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Surname,Description,UserId")] Employee employee, List<string> workingDays, string startOfWork, string endOfWork)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }


            employee.UserApp = _context.Users.Find(employee.UserId);
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.Employee
                                            .Include(e => e.WorkingDate)
                                            .FirstOrDefaultAsync(m => m.Id == id);

                    if (existingEmployee == null)
                    {
                        return NotFound();
                    }

                    existingEmployee.Name = employee.Name;
                    existingEmployee.Surname = employee.Surname;
                    existingEmployee.Description = employee.Description;
                    existingEmployee.WorkingDate.Clear();

                    if (workingDays != null && workingDays.Any())
                    {
                        foreach (var day in workingDays)
                        {
                            if (Enum.TryParse(day, out DayOfWeek weekDay))
                            {
                                var workingDate = new EmployeesWorkingDate
                                {
                                    WorkDay = weekDay,
                                    StartOfWork = TimeSpan.Parse(startOfWork),
                                    EndOfWork = TimeSpan.Parse(endOfWork),
                                    EmployeeId = existingEmployee.Id
                                };
                                existingEmployee.WorkingDate.Add(workingDate);
                            }
                        }
                    }

                    _context.Update(existingEmployee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Employee == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Employee == null)
            {
                return Problem("Entity set 'BeautySalonAppDbContext.Employee'  is null.");
            }
            var employee = await _context.Employee.FindAsync(id);
            if (employee != null)
            {
                _context.Employee.Remove(employee);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(long id)
        {
          return (_context.Employee?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
