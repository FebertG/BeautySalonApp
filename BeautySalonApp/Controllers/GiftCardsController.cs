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
using Microsoft.AspNetCore.Authorization;

namespace BeautySalonApp.Controllers
{
    [Authorize]
    public class GiftCardsController : Controller
    {
        private readonly BeautySalonAppDbContext _context;

        public GiftCardsController(BeautySalonAppDbContext context)
        {
            _context = context;
        }

        // GET: GiftCards
        public async Task<IActionResult> Index(string searchCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var giftCards = _context.GiftCard.Where(gc => gc.UserId == userId);

            if (!string.IsNullOrEmpty(searchCode))
            {
                giftCards = giftCards.Where(gc => gc.Code.Contains(searchCode));
            }

            return View(await giftCards.ToListAsync());
        }


        // GET: GiftCards/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.GiftCard == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var giftCard = await _context.GiftCard.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (giftCard == null)
            {
                return NotFound();
            }

            return View(giftCard);
        }

        // GET: GiftCards/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(ViewData["UserId"]);
            return View();
        }

        // POST: GiftCards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Value,ExpirationDate,UserId")] GiftCard giftCard)
        {
            giftCard.UserApp = _context.Users.Find(giftCard.UserId);
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                string uniqueCode;
                do
                {
                    uniqueCode = GenerateRandomCode(10);
                } while (_context.GiftCard.Any(gc => gc.Code == uniqueCode));

                giftCard.Code = uniqueCode;

                _context.Add(giftCard);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(giftCard);
        }


        // GET: GiftCards/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.GiftCard == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var giftCard = await _context.GiftCard.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (giftCard == null)
            {
                return NotFound();
            }
            return View(giftCard);
        }

        // POST: GiftCards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Code,Name,Description,Price,Value,ExpirationDate")] GiftCard giftCardInput)
        {

            giftCardInput.UserApp = _context.Users.Find(giftCardInput.UserId);
            ModelState.Clear();

            if (id != giftCardInput.Id)
            {
                return NotFound();
            }

            var giftCard = await _context.GiftCard.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

            if (giftCard == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (giftCard.UserId != userId)
            {
                return Forbid();
            }

            giftCard.Name = giftCardInput.Name;
            giftCard.Description = giftCardInput.Description;
            giftCard.Price = giftCardInput.Price;
            giftCard.Value = giftCardInput.Value;
            giftCard.ExpirationDate = giftCardInput.ExpirationDate;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(giftCard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiftCardExists(giftCard.Id))
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
            return View(giftCard);
        }


        // GET: GiftCards/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.GiftCard == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var giftCard = await _context.GiftCard
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (giftCard == null)
            {
                return NotFound();
            }

            return View(giftCard);
        }

        // POST: GiftCards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.GiftCard == null)
            {
                return Problem("Entity set 'BeautySalonAppDbContext.GiftCard'  is null.");
            }

            var giftCard = await _context.GiftCard.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (giftCard != null)
            {
                _context.GiftCard.Remove(giftCard);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GiftCardExists(long id)
        {
          return (_context.GiftCard?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private string GenerateRandomCode(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
