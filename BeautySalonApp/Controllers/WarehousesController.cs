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
    public class WarehousesController : Controller
    {
        private readonly BeautySalonAppDbContext _context;
        private readonly UserManager<UserApp> _userManager;
        private readonly ILogger<WarehousesController> _logger;

        public WarehousesController(BeautySalonAppDbContext context, UserManager<UserApp> userManager, ILogger<WarehousesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Warehouses
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Error");
            }

            var warehouse = await _context.Warehouse
                        .Where(w => w.UserId == currentUser.Id)
                        .Include(w => w.Products)
                        .FirstOrDefaultAsync();

          
                ViewBag.CanCreateWarehouse = true;

            return View(warehouse);
        }



        // GET: Warehouses/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Warehouse == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Warehouse
                .FirstOrDefaultAsync(m => m.Id == id);
            if (warehouse == null)
            {
                return NotFound();
            }

            return View(warehouse);
        }

        // GET: Warehouses/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View();
        }

        // POST: Warehouses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId")] Warehouse warehouse)
        {
            warehouse.UserApp = _context.Users.Find(warehouse.UserId);
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);

                    if (currentUser == null)
                    {
                        return RedirectToAction("Error");
                    }

                    warehouse.UserId = currentUser.Id;

                    _context.Warehouse.Add(warehouse);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas tworzenia magazynu.");

                    return RedirectToAction("Error");
                }
            }
            return View(warehouse);
        }





        // GET: Warehouses/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Warehouse == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Warehouse.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound();
            }
            return View(warehouse);
        }

        // POST: Warehouses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id")] Warehouse warehouse)
        {
            if (id != warehouse.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(warehouse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarehouseExists(warehouse.Id))
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
            return View(warehouse);
        }

        // GET: Warehouses/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Warehouse == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Warehouse
                .FirstOrDefaultAsync(m => m.Id == id);
            if (warehouse == null)
            {
                return NotFound();
            }

            return View(warehouse);
        }

        // POST: Warehouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Warehouse == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Warehouse'  is null.");
            }
            var warehouse = await _context.Warehouse.FindAsync(id);
            if (warehouse != null)
            {
                _context.Warehouse.Remove(warehouse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WarehouseExists(long id)
        {
            return (_context.Warehouse?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // POST: AddProduct
        [HttpPost]
        public async Task<IActionResult> AddProduct(long warehouseId, Product newProduct)
        {
            var warehouse = await _context.Warehouse.Include(w => w.Products).FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse != null && ModelState.IsValid)
            {
                warehouse.Products.Add(newProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("Error");
        }

        // POST: ModifyProductAmount
        [HttpPost]
        public async Task<IActionResult> ModifyProductAmount(long productId, int modifyAmount)
        {
            var product = await _context.Product.FindAsync(productId);
            if (product != null)
            {
                product.Amount += modifyAmount;

                if (product.Amount <= 0)
                {
                    _context.Product.Remove(product);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View("Error");
        }

        // GET: ProductDetails
        public async Task<IActionResult> ProductDetails(long productId)
        {
            var product = await _context.Product.FindAsync(productId);
            if (product != null)
            {
                return View(product);
            }
            return NotFound();
        }
    }
}
