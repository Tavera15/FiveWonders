using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        public IRepository<FWonderOrder> ordersContext;
        public IRepository<Customer> customerContext;

        public OrdersController(IRepository<FWonderOrder> ordersRepository, IRepository<Customer> customerRepository)
        {
            ordersContext = ordersRepository;
            customerContext = customerRepository;
        }

        // GET: Orders
        public ActionResult Index()
        {
            try
            {
                string customerEmail = HttpContext.User.Identity.Name;
                Customer customer = customerContext.GetCollection().FirstOrDefault(x => x.mEmail == customerEmail);

                if (customer == null)
                {
                    throw new Exception("Orders: Customer not found");
                }

                // Get customers past completed orders - Sort them by date (Newest are at top) 
                FWonderOrder[] allCustomerOrders = ordersContext.GetCollection()
                    .Where(x => x.isCompleted && x.mCustomerId == customer.mID)
                    .OrderByDescending(y => y.mTimeEntered).ToArray();

                return View(allCustomerOrders);
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        [Route(Name = "/{Id}{VerificationId}")]
        public ActionResult Details(string Id, string VerificationId)
        {
            try
            {
                // TODO If Email is confirmed, make sure they have to login first

                FWonderOrder order = ordersContext.Find(Id, true);

                if (!order.isCompleted || String.IsNullOrWhiteSpace(order.mVerificationId) || VerificationId != order.mVerificationId)
                {
                    throw new Exception("Order not accessible.");
                }

                string possibleCustomerUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

                // Order is linked with customer
                // Check that the right customer is logged in.
                if (!String.IsNullOrWhiteSpace(order.mCustomerId))
                {
                    Customer customer = customerContext.GetCollection()
                        .FirstOrDefault(c => c.mUserID == possibleCustomerUserId);

                    if (customer == null || order.mCustomerId != customer.mID)
                    {
                        throw new Exception("Order not accessible.");
                    }
                }
                else
                {
                    // Order is not linked with customer
                    // Check that no customer is logged in.
                    if(!String.IsNullOrWhiteSpace(possibleCustomerUserId))
                    {
                        throw new Exception("Order not accessible.");
                    }
                }

                return View(order);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }
    }
}