using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeautySalonApp.Data;
using BeautySalonApp.Models;
using Microsoft.AspNetCore.Identity;
using BeautySalonApp.Areas.Identity.Data;
using BeautySalonApp.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BeautySalonApp.Controllers
{
    [Authorize]
    public class OpinionsController : Controller
    {
        private readonly BeautySalonAppDbContext _context;
        private readonly UserManager<UserApp> _userManager;

        public OpinionsController(BeautySalonAppDbContext context, UserManager<UserApp> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Opinions
        public async Task<IActionResult> Index()
        {
              return _context.Opinion != null ? 
                          View(await _context.Opinion.ToListAsync()) :
                          Problem("Entity set 'BeautySalonAppDbContext.Opinion'  is null.");
        }

        // GET: Opinions/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Opinion == null)
            {
                return NotFound();
            }

            var opinion = await _context.Opinion
                .FirstOrDefaultAsync(m => m.Id == id);
            if (opinion == null)
            {
                return NotFound();
            }

            return View(opinion);
        }

        // GET: Opinions/Create
        public IActionResult Create(long salonId)
        {
            ViewBag.SalonId = salonId;
            return View();
        }

        // POST: Opinions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Opinion")] SalonViewModel viewModel, long salonId)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var opinion = new Opinion
                {
                    Rating = viewModel.Opinion.Rating,
                    Comment = viewModel.Opinion.Comment,
                    DateAdded = DateTime.Now,
                    UserId = user.Id,
                    SalonId = salonId
                };

                _context.Add(opinion);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Salons", new { id = salonId });
            }

            return View(viewModel);
        }




        // GET: Opinions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Opinion == null)
            {
                return NotFound();
            }

            var opinion = await _context.Opinion.FindAsync(id);
            if (opinion == null)
            {
                return NotFound();
            }
            return View(opinion);
        }

        // POST: Opinions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Rating,Comment,DateAdded,UserId")] Opinion opinion)
        {
            if (id != opinion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(opinion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpinionExists(opinion.Id))
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
            return View(opinion);
        }

        // GET: Opinions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Opinion == null)
            {
                return NotFound();
            }

            var opinion = await _context.Opinion
                .FirstOrDefaultAsync(m => m.Id == id);
            if (opinion == null)
            {
                return NotFound();
            }

            return View(opinion);
        }

        // POST: Opinions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Opinion == null)
            {
                return Problem("Entity set 'BeautySalonAppDbContext.Opinion'  is null.");
            }
            var opinion = await _context.Opinion.FindAsync(id);
            if (opinion != null)
            {
                _context.Opinion.Remove(opinion);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OpinionExists(long id)
        {
          return (_context.Opinion?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }

}
