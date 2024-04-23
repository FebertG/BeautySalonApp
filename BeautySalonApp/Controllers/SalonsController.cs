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
using BeautySalonApp.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BeautySalonApp.Controllers
{
    public class SalonsController : Controller
    {
        private readonly BeautySalonAppDbContext _context;
        private readonly UserManager<UserApp> _userManager;
        private readonly SignInManager<UserApp> _signInManager;

        public SalonsController(BeautySalonAppDbContext context, UserManager<UserApp> userManager, SignInManager<UserApp> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Salons
        public async Task<IActionResult> Index(string searchString, string searchCity)
        {
            var salons = from s in _context.Salon.Include(s => s.Address)
                         select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                salons = salons.Where(s => s.Name.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(searchCity))
            {
                salons = salons.Where(s => s.Address.City.Contains(searchCity));
            }

            return View(await salons.ToListAsync());
        }


        // GET: Salons/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Salon == null)
            {
                return NotFound();
            }

            var salon = await _context.Salon
                .Include(s => s.Address)
                .Include(s => s.Opinions)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (salon == null)
            {
                return NotFound();
            }

            double averageRating = salon.Opinions.Any() ? Math.Round(salon.Opinions.Average(o => o.Rating), 2) : 0;

            var viewModel = new SalonViewModel
            {
                Salon = salon,
                Opinions = salon.Opinions,
                Services = salon.Services,
                AverageRating = averageRating
            };

            return View(viewModel);
        }

        // GET: Salons/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            
            var user = await _userManager.GetUserAsync(User);

            var existingSalon = await _context.Salon.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (existingSalon != null)
            {
                return RedirectToAction("Details", new { id = existingSalon.Id });
            }

            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Id");

            return View(new SalonViewModel());
        }

        // POST: Salons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Salon,Address,UserId")] SalonViewModel viewModel)
        {

            viewModel.UserApp = _context.Users.Find(viewModel.UserId);
            ModelState.Clear();
            var user = await _userManager.GetUserAsync(User);

            var existingSalon = await _context.Salon.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (existingSalon != null)
            {
                return RedirectToAction("Details", new { id = existingSalon.Id });
            }

            if (ModelState.IsValid)
            {
                var adres = viewModel.Address;
                _context.Add(adres);
                await _context.SaveChangesAsync();

                var salon = viewModel.Salon;
                salon.UserId = user.Id;
                salon.Address = _context.Address.AsQueryable().Where(a => a.Id == adres.Id).First();
                _context.Add(salon);
                await _context.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, "SalonOwner");

                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction(nameof(Index));
            }

            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Id", viewModel.Salon.AddressId);
            return View(viewModel);
        }

        // GET: Salons/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var salon = await _context.Salon.FindAsync(id);
            if (salon == null || salon.UserId != user.Id)
            {
                return NotFound();
            }

            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Id", salon.AddressId);

            return View(new SalonViewModel { Salon = salon });
        }

        // POST: Salons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Salon,Address")] SalonViewModel viewModel)
        {
            if (id != viewModel.Salon.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (viewModel.Salon.UserId != user.Id)
            {
                return Unauthorized();
            }



            viewModel.UserApp = _context.Users.Find(viewModel.UserId);
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                try
                {
                    var existingAddress = await _context.Address.FindAsync(viewModel.Address.Id);
                    if (existingAddress == null)
                    {
                        return NotFound();
                    }

                    existingAddress.Street = viewModel.Address.Street;
                    existingAddress.City = viewModel.Address.City;
                    existingAddress.PostalCode = viewModel.Address.PostalCode;

                    _context.Update(viewModel.Salon);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalonExists(viewModel.Salon.Id))
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

            return View(viewModel);
        }


        // GET: Salons/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Salon == null)
            {
                return NotFound();
            }

            var salon = await _context.Salon
                .Include(s => s.Address)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (salon == null)
            {
                return NotFound();
            }

            return View(salon);
        }

        // POST: Salons/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var salon = await _context.Salon.FindAsync(id);
            if (salon == null)
            {
                return NotFound();
            }

            try
            {
                var address = await _context.Address.FirstOrDefaultAsync(a => a.Id == salon.AddressId);

                if (address != null)
                {
                    _context.Address.Remove(address);
                }

                var services = await _context.Service.Where(s => s.SalonId == id).ToListAsync();

                _context.Service.RemoveRange(services);

                var opinions = await _context.Opinion.Where(o => o.SalonId == id).ToListAsync();

                _context.Opinion.RemoveRange(opinions);

                _context.Salon.Remove(salon);

                var user = await _userManager.GetUserAsync(User);

                if (user != null && await _userManager.IsInRoleAsync(user, "SalonOwner"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "SalonOwner");

                    await _signInManager.RefreshSignInAsync(user);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred while deleting the salon: {ex.Message}");
            }
        }





        private bool SalonExists(long id)
        {
          return (_context.Salon?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        [HttpPost]
        public async Task<IActionResult> AddOpinion(long salonId, double rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);

            var opinion = new Opinion
            {
                Rating = rating,
                Comment = comment,
                DateAdded = DateTime.Now,
                UserId = user.Id,
                SalonId = salonId
            };

            var salon = await _context.Salon.Include(s => s.Opinions).FirstOrDefaultAsync(s => s.Id == salonId);
            salon.Opinions.Add(opinion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = salonId });
        }
    }
}
