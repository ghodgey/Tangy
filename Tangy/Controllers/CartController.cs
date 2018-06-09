using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.OrderDetailsViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public OrderDetailsCart DetailsCart { get; set; }


        public CartController(ApplicationDbContext db) => _db = db;


        public IActionResult Index()
        {
            DetailsCart = new OrderDetailsCart()
            {
                OrderHeader = new OrderHeader()
            };

            DetailsCart.OrderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);

            if(cart != null)
            {
                DetailsCart.ListCart = cart.ToList();
            }

            foreach(var list in DetailsCart.ListCart)
            {
                list.MenuItem = _db.MenuItem.FirstOrDefault(m => m.Id == list.MenuItemId);
                DetailsCart.OrderHeader.OrderTotal = DetailsCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);

                if(list.MenuItem.Description.Length > 100)
                {
                    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                }

            }

            DetailsCart.OrderHeader.PickupTime = DateTime.Now;

            return View(DetailsCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            DetailsCart.ListCart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToList();

            DetailsCart.OrderHeader.OrderDate = DateTime.Now;
            DetailsCart.OrderHeader.UserId = claim.Value;
            DetailsCart.OrderHeader.Status = SD.StatusSubmitted;

            OrderHeader orderHeader = DetailsCart.OrderHeader;

            _db.OrderHeader.Add(orderHeader);
            _db.SaveChanges();

            foreach(var item in DetailsCart.ListCart)
            {
                item.MenuItem = _db.MenuItem.FirstOrDefault(m => m.Id == item.MenuItemId);
                OrderDetails orderDetails = new OrderDetails()
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = orderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                _db.OrderDetails.Add(orderDetails);
            }

            _db.ShoppingCart.RemoveRange(DetailsCart.ListCart);
            _db.SaveChanges();
            HttpContext.Session.SetInt32("CartCount", 0);

            return RedirectToAction("Confirm", "Order", new { id = orderHeader.Id });


        }

        public IActionResult Plus(int cartId)
        {
            var cart = _db.ShoppingCart.Where(c => c.Id == cartId).FirstOrDefault();
            cart.Count += 1;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Minus(int cartId)
        {
            var cart = _db.ShoppingCart.Where(c => c.Id == cartId).FirstOrDefault();

            if(cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                _db.SaveChanges();

                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("CartCount", cnt);
            }
            else
            {
                cart.Count -= 1;
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}