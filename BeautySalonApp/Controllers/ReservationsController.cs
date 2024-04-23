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
    public class ReservationsController : Controller
    {
        private readonly BeautySalonAppDbContext _context;
        private readonly UserManager<UserApp> _userManager;

        public ReservationsController(BeautySalonAppDbContext context, UserManager<UserApp> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var futureReservations = await _context.Reservation
                .Include(r => r.Service)
                .ThenInclude(s => s.Salon)
                .Where(r => r.StartTime > now)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            var pastReservations = await _context.Reservation
                .Include(r => r.Service)
                .ThenInclude(s => s.Salon)
                .Where(r => r.StartTime <= now)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            return View(new Tuple<List<Reservation>, List<Reservation>>(futureReservations, pastReservations));
        }

        // GET: Reservations/SalonReservations
        public async Task<IActionResult> SalonReservations()
        {
            var now = DateTime.Now;
            var userId = _userManager.GetUserId(User);

            var futureReservations = await _context.Reservation
                .Include(r => r.Service)
                .Where(r => r.Service.UserId == userId && r.StartTime > now)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            var pastReservations = await _context.Reservation
                .Include(r => r.Service)
                .Where(r => r.Service.UserId == userId && r.StartTime <= now)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            return View(new Tuple<List<Reservation>, List<Reservation>>(futureReservations, pastReservations));
        }



        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Reservation == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .Include(r => r.Service)
                    .ThenInclude(s => s.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            ViewBag.EmployeeName = reservation.Service.Employee.Name + " " + reservation.Service.Employee.Surname;

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create(long serviceId)
        {

            var reservation = new Reservation();

            reservation.ServiceId = serviceId;

            ViewBag.ServiceId = new SelectList(_context.Service, "Id", "ServiceName", serviceId);

            ViewData["User"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(ViewData["User"]);
            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id");
            return View(reservation);
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartTime,ServiceId")] Reservation reservation)
        {
            var user = await _userManager.GetUserAsync(User);
            reservation.UserId = user?.Id;

            var service = await _context.Service
                            .Include(s => s.Employee)
                            .ThenInclude(e => e.WorkingDate)
                            .FirstOrDefaultAsync(s => s.Id == reservation.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError("", "Nie znaleziono usługi.");
                ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id", reservation.ServiceId);
                return View(reservation);
            }

            reservation.EndTime = reservation.StartTime.AddMinutes(service.DurationMin);

            reservation.Price = service.Price;

            var workDay = service.Employee.WorkingDate.FirstOrDefault(wd => wd.WorkDay == reservation.StartTime.DayOfWeek);
            if (workDay == null)
            {
                var polishDayOfWeek = GetPolishDayOfWeek(reservation.StartTime.DayOfWeek);
                ModelState.AddModelError("", $"Wybrany pracownik nie pracuje w dniu {polishDayOfWeek}.");
                ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id", reservation.ServiceId);
                return View(reservation);
            }

            var startTime = new TimeSpan(reservation.StartTime.Hour, reservation.StartTime.Minute, 0);
            var endTime = new TimeSpan(reservation.EndTime.Hour, reservation.EndTime.Minute, 0);
            if (startTime < workDay.StartOfWork || endTime > workDay.EndOfWork)
            {
                ModelState.AddModelError("", $"Wybrany pracownik pracuje w godzinach {workDay.StartOfWork} - {workDay.EndOfWork}.");
                ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id", reservation.ServiceId);
                return View(reservation);
            }

            var existingReservations = await _context.Reservation
                .Where(r => r.Service.EmployeeId == service.EmployeeId
                            && r.StartTime < reservation.EndTime
                            && r.EndTime > reservation.StartTime)
                .ToListAsync();

            if (existingReservations.Any())
            {
                ModelState.AddModelError("", "Wybrany termin nachodzi na inną rezerwację.");
                ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id", reservation.ServiceId);
                return View(reservation);
            }

            ModelState.Clear();
            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id", reservation.ServiceId);
            return View(reservation);
        }


        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Reservation == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id", reservation.ServiceId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UserId,StartTime,EndTime,ServiceId,Price")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Id", reservation.ServiceId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Reservation == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .Include(r => r.Service)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Reservation == null)
            {
                return Problem("Entity set 'BeautySalonAppDbContext.Reservation'  is null.");
            }
            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservation.Remove(reservation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(long id)
        {
          return (_context.Reservation?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private string GetPolishDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Poniedziałek";
                case DayOfWeek.Tuesday:
                    return "Wtorek";
                case DayOfWeek.Wednesday:
                    return "Środa";
                case DayOfWeek.Thursday:
                    return "Czwartek";
                case DayOfWeek.Friday:
                    return "Piątek";
                case DayOfWeek.Saturday:
                    return "Sobota";
                case DayOfWeek.Sunday:
                    return "Niedziela";
                default:
                    return "";
            }
        }
    }
}
