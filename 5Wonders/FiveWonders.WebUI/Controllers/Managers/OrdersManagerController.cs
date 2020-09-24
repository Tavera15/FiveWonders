using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    [Authorize]
    public class OrdersManagerController : Controller
    {
        IRepository<FWonderOrder> ordersContext;

        public OrdersManagerController(IRepository<FWonderOrder> ordersRepository)
        {
            ordersContext = ordersRepository;
        }

        // GET: OrdersManager
        [Route(Name = "/{emailFilter?}{nameFilter?}")]
        public ActionResult Index(string emailFilter, string nameFilter)
        {
            emailFilter = !String.IsNullOrWhiteSpace(emailFilter) ? emailFilter.Trim() : "";
            nameFilter = !String.IsNullOrWhiteSpace(nameFilter) ? nameFilter.Trim() : "";
            FWonderOrder[] processedOrders = { };

            // Get all completed orders
            FWonderOrder[] allCompletedOrders = ordersContext.GetCollection()
                .Where(o => o.paypalRef != null && o.paypalRef.Trim() != string.Empty)
                .OrderByDescending(t => t.mTimeEntered).ToArray();

            // Apply both filters
            if(!String.IsNullOrWhiteSpace(emailFilter) && !String.IsNullOrWhiteSpace(nameFilter))
            {
                FWonderOrder[] emailFilterOrders = allCompletedOrders
                    .Where(o => o.mCustomerEmail.ToLower() == emailFilter.ToLower()).ToArray();

                FWonderOrder[] nameFilterOrders = allCompletedOrders
                    .Where(o => o.mCustomerName.ToLower() == nameFilter.ToLower()).ToArray();

                processedOrders = String.IsNullOrWhiteSpace(emailFilter) && String.IsNullOrWhiteSpace(nameFilter)
                    ? allCompletedOrders
                    : emailFilterOrders.Intersect(nameFilterOrders).ToArray();
            }
            // Filter by email input
            else if(!String.IsNullOrWhiteSpace(emailFilter))
            {
                processedOrders = allCompletedOrders
                    .Where(o => o.mCustomerEmail.ToLower() == emailFilter.ToLower()).ToArray();
            }
            // Filter by name input
            else if(!String.IsNullOrWhiteSpace(nameFilter))
            {
                processedOrders = allCompletedOrders
                    .Where(o => o.mCustomerName.ToLower() == nameFilter.ToLower()).ToArray();
            }
            // Both filters are empty
            else
            {
                processedOrders = allCompletedOrders;
            }

            ViewBag.Title = "Orders";
            ViewBag.emailFilter = emailFilter;
            ViewBag.nameFilter = nameFilter;
            return View(processedOrders);
        }

        [ActionName("Index")]
        [HttpPost]
        public ActionResult SearchIndex(string emailFilter, string nameFilter)
        {
            return RedirectToAction("Index", "OrdersManager", new { emailFilter = emailFilter, nameFilter = nameFilter});
        }

        public ActionResult Details(string Id)
        {
            try
            {
                FWonderOrder order = ordersContext.Find(Id, true);

                if(String.IsNullOrWhiteSpace(order.paypalRef))
                {
                    throw new Exception("Not a completed order");
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