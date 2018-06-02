using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.AccountViewModels;
using Tangy.Models.ManageViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;

        }

        //get

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var users = await _db.ApplicationUser.Where(u => u.Id != user.Id).ToListAsync();
            
            

            return View(users);
        }

        //Get Edit coupons
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _db.ApplicationUser.SingleOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            //added into a VM for further data annotations on the Identity table AspNetUser
            var userVM = new IndexViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                LockoutEnd = user.LockoutEnd,
                LockoutReason = user.LockoutReason,
                AccessFailedCount = user.AccessFailedCount
            };


            return View(userVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser user)
        {

            if (id != user.Id)
            {
                return NotFound();

            }

            var userFromDb = await _db.ApplicationUser.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                

                userFromDb.FirstName = user.FirstName;
                userFromDb.LastName = user.LastName;
                userFromDb.PhoneNumber = user.PhoneNumber;
                userFromDb.LockoutEnd = user.LockoutEnd;
                userFromDb.LockoutReason = user.LockoutReason;



                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }

            
            return View(user);
        }


        //Get Edit coupons
        public async Task<IActionResult> Lock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _db.ApplicationUser.SingleOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id, ApplicationUser user)
        {

            if (id != user.Id)
            {
                return NotFound();

            }

            var userFromDb = await _db.ApplicationUser.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {


               
                userFromDb.LockoutEnd = DateTime.Now.AddYears(100);
                userFromDb.LockoutReason = user.LockoutReason;

                userFromDb.FirstName = user.FirstName;
                userFromDb.LastName = user.LastName;
                userFromDb.PhoneNumber = user.PhoneNumber;
                



                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }


            return View(user);
        }


    }
}