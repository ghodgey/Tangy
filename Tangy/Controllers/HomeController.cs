using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.HomeViewModels;

namespace Tangy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {

            IndexViewModel IndexVM = new IndexViewModel()
            {
                MenuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .ToListAsync(),

                Category = await _db.Category
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync(),

                Coupons = await _db.Coupons
                .Where(c => c.IsActive == true)
                .ToListAsync()
            };

            return View(IndexVM);

        }

        //Get
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var MenuItemFromDb = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            ShoppingCart CartObj = new ShoppingCart()
            {
                MenuItem = MenuItemFromDb,
                MenuItemId = MenuItemFromDb.Id
            };

            return View(CartObj);

        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;

            if(ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                var cartFromDb = await _db.ShoppingCart
                    .Where(c => c.ApplicationUserId == CartObject.ApplicationUserId && c.MenuItemId == CartObject.MenuItemId)
                    .FirstOrDefaultAsync();

                if (cartFromDb == null)
                {
                    //this menu item does not exist
                    _db.ShoppingCart.Add(CartObject);
                }
                else
                {
                    //Menu item exists in the shopping cart for that user, just update the count
                    cartFromDb.Count = cartFromDb.Count + CartObject.Count;
                }

                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart
                    .Where(c => c.ApplicationUserId == CartObject.ApplicationUserId)
                    .ToList()
                    .Count();

                HttpContext.Session.SetInt32("CartCount", count); //in startup add services session (storing the count of the session for the items in the shooping cart for a user

                return RedirectToAction("Index");
            }
            else
            {
                var MenuItemFromDb = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Where(m => m.Id == CartObject.MenuItemId)
                .FirstOrDefaultAsync();

                ShoppingCart CartObj = new ShoppingCart()
                {
                    MenuItem = MenuItemFromDb,
                    MenuItemId = MenuItemFromDb.Id
                };

                return View(CartObj);
            }
        }
    }
}
