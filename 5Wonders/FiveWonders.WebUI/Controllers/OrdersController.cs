using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
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
                    return RedirectToAction("Index", "Home");

                // Get customers past completed orders - Sort them by date (Newest are at top) 
                FWonderOrder[] allCustomerOrders = ordersContext.GetCollection()
                    .Where(x => x.mCustomerEmail == customerEmail && !String.IsNullOrEmpty(x.paypalRef))
                    .OrderByDescending(y => y.mTimeEntered).ToArray();

                return View(allCustomerOrders);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Details(string Id)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(Id))
                    throw new Exception("Order Not Found");

                FWonderOrder order = ordersContext.Find(Id);
                return View(order);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Orders");
            }
        }
    }
}