﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.OrderDetailsViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class OrderController : Controller
    {
        private ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public IActionResult Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = _db.OrderHeader.Where(o => o.Id == id && o.UserId == claim.Value).FirstOrDefault(),
                OrderDetail = _db.OrderDetails.Where(o => o.OrderId == id).ToList()
            };

            return View(orderDetailsViewModel);
        }

        [Authorize]
        public IActionResult OrderHistory()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<OrderDetailsViewModel> OrderDetailsVM = new List<OrderDetailsViewModel>();

            List<OrderHeader> OrderHeaderList = _db.OrderHeader
                .Where(u => u.UserId == claim.Value)
                .OrderByDescending(u => u.OrderDate)
                .ToList();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetail = _db.OrderDetails.Where(o => o.OrderId == item.Id).ToList();
                OrderDetailsVM.Add(individual);
            }

            return View(OrderDetailsVM);

        }

        [Authorize(Roles = SD.AdminEndUser)]
        public IActionResult ManageOrder()
        {
            List<OrderDetailsViewModel> OrderDetailsVM = new List<OrderDetailsViewModel>();


            List<OrderHeader> OrderHeaderList = _db.OrderHeader
                .Where(o => o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess)
                .OrderByDescending(u => u.PickupTime)
                .ToList();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetail = _db.OrderDetails.Where(o => o.OrderId == item.Id).ToList();
                OrderDetailsVM.Add(individual);
            }

            return View(OrderDetailsVM);
        }
    }
}