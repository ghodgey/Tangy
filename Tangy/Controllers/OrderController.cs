using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.OrderDetailsViewModels;
using Tangy.Services;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class OrderController : Controller
    {
        private ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private int PageSize = 2;

        public OrderController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = _db.OrderHeader.Where(o => o.Id == id && o.UserId == claim.Value).FirstOrDefault(),
                OrderDetail = _db.OrderDetails.Where(o => o.OrderId == id).ToList()
            };

            var customerEmail = _db.Users.Where(u => u.Id == orderDetailsViewModel.OrderHeader.UserId).FirstOrDefault().Email;
            await _emailSender.SendOrderStatusAsync(customerEmail, orderDetailsViewModel.OrderHeader.Id.ToString(), SD.StatusSubmitted);


            return View(orderDetailsViewModel);
        }

        [Authorize]
        public IActionResult OrderHistory(int productPage = 1)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //implemented for the paging (Custom tag helper)
            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            List<OrderHeader> OrderHeaderList = _db.OrderHeader
                .Where(u => u.UserId == claim.Value)
                .OrderByDescending(u => u.OrderDate)
                .ToList();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetail = _db.OrderDetails.Where(o => o.OrderId == item.Id).ToList()
                };
                orderListVM.Orders.Add(individual);
            }
            var count = orderListVM.Orders.Count;

            orderListVM.Orders = orderListVM.Orders.OrderBy(p => p.OrderHeader.Id)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count
            };

            return View(orderListVM);

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
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetail = _db.OrderDetails.Where(o => o.OrderId == item.Id).ToList()
                };
                OrderDetailsVM.Add(individual);
            }

            return View(OrderDetailsVM);
        }

        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderPrepare(int orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderCancel(int orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();

            var customerEmail = _db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email;
            await _emailSender.SendOrderStatusAsync(customerEmail, orderHeader.Id.ToString(), SD.StatusCancelled);

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderReady(int orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusReady;
            await _db.SaveChangesAsync();

            var customerEmail = _db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email;
            await _emailSender.SendOrderStatusAsync(customerEmail, orderHeader.Id.ToString(), SD.StatusReady);

            return RedirectToAction("ManageOrder", "Order");
        }

        //Get order pickup
        public IActionResult OrderPickup(string searchEmail = null, string searchPhone = null, string searchOrder = null)
        {

            List<OrderDetailsViewModel> OrderDetailsVM = new List<OrderDetailsViewModel>();

            if (searchEmail != null || searchPhone != null || searchOrder != null)
            {
                //filtering the criteria
                var user = new ApplicationUser();
                List<OrderHeader> OrderHeaderList = new List<OrderHeader>();

                if (searchOrder != null)
                {
                    OrderHeaderList = _db.OrderHeader.Where(o => o.Id == Convert.ToInt32(searchOrder)).ToList();
                }
                else if (searchEmail != null)
                {
                    user = _db.Users.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefault();
                }
                else if (searchPhone != null)
                {
                    user = _db.Users.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower())).FirstOrDefault();

                }
                if (user != null || OrderHeaderList.Count > 0)
                {
                    if (OrderHeaderList.Count == 0)
                    {
                        OrderHeaderList = _db.OrderHeader.Where(o => o.UserId == user.Id).OrderByDescending(o => o.OrderDate).ToList();
                    }

                    foreach (OrderHeader item in OrderHeaderList)
                    {
                        OrderDetailsViewModel individual = new OrderDetailsViewModel
                        {
                            OrderHeader = item,
                            OrderDetail = _db.OrderDetails.Where(o => o.OrderId == item.Id).ToList()
                        };
                        OrderDetailsVM.Add(individual);
                    }
                }
            }
            else
            {
                List<OrderHeader> OrderHeaderList = _db.OrderHeader
                    .Where(o => o.Status == SD.StatusReady)
                    .OrderByDescending(u => u.PickupTime)
                    .ToList();

                foreach (OrderHeader item in OrderHeaderList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel
                    {
                        OrderHeader = item,
                        OrderDetail = _db.OrderDetails.Where(o => o.OrderId == item.Id).ToList()
                    };
                    OrderDetailsVM.Add(individual);
                }
            }
            return View(OrderDetailsVM);
        }

        [Authorize(Roles = SD.AdminEndUser)]
        public IActionResult OrderPickupDetails(int orderId)
        {
            OrderDetailsViewModel OrderDetailsVM = new OrderDetailsViewModel
            {
                OrderHeader = _db.OrderHeader.Where(o => o.Id == orderId).FirstOrDefault(),


            };

            OrderDetailsVM.OrderHeader.ApplicationUser = _db.Users
                .Where(u => u.Id == OrderDetailsVM.OrderHeader.UserId)
                .FirstOrDefault();

            OrderDetailsVM.OrderDetail = _db.OrderDetails.Where(o => o.OrderId == OrderDetailsVM.OrderHeader.Id).ToList();

            return View(OrderDetailsVM);

        }

        [HttpPost]
        [Authorize(Roles = SD.AdminEndUser)]
        [ValidateAntiForgeryToken]
        [ActionName("OrderPickupDetails")]
        public async Task<IActionResult> OrderPickupDetailsPost(int orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("OrderPickup", "Order");


        }

        //Get
        public IActionResult DownloadOrderDetails(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                return View();

            if (startDate.HasValue && endDate.HasValue)
            {
                //List<OrderHeader> OrderHeaderList = _db.OrderHeader
                //    .Where(u => u.OrderDate >= startDate && u.OrderDate <= endDate)
                //   .OrderByDescending(u => u.OrderDate)
                //   .ToList();



                //var OrderDetailsList = new List<OrderDetails>();

                //query related table -> doesn't require you to load the details of OrderHeader
                var OrderDetailsList = _db.OrderDetails
                    .Where(o => o.OrderHeader.OrderDate >= startDate && o.OrderHeader.OrderDate <= endDate)
                    .Select(o => new { o.Id, o.OrderId, o.Count, o.Name, o.Price })
                    .ToList();

                
                var csvString = ConvertToString(OrderDetailsList);
                var fileName = "OrderDetails_" + DateTime.Now.ToString() + ".csv";
                return File(new System.Text.UTF8Encoding().GetBytes(csvString), "text/csv", fileName);



            }

            return View();
        }
        public String ConvertToString<T>(IList<T> data)
        {

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)

            {

                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            }

            foreach (T item in data)

            {

                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)

                {

                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                }

                table.Rows.Add(row);

            }

            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);

            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in table.Rows)

            {

                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());

                sb.AppendLine(string.Join(",", fields));

            }

            return sb.ToString();

        }


    }
}