﻿using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    [Authorize(Roles = "FWondersAdmin")]
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

            // Get all completed orders
            FWonderOrder[] allCompletedOrders = ordersContext.GetCollection()
                .Where(o => o.isCompleted)
                .OrderByDescending(t => t.mTimeEntered).ToArray();

            // Apply filters
            if(!String.IsNullOrWhiteSpace(emailFilter))
            {
                allCompletedOrders = allCompletedOrders
                    .Where(o => o.mCustomerEmail.ToLower() == emailFilter.ToLower()).ToArray();
            }

            if(!String.IsNullOrWhiteSpace(nameFilter))
            {
                allCompletedOrders = allCompletedOrders
                    .Where(o => o.mCustomerName.ToLower() == nameFilter.ToLower()).ToArray();
            }

            ViewBag.Title = "Orders Manager";
            ViewBag.emailFilter = emailFilter;
            ViewBag.nameFilter = nameFilter;
            return View(allCompletedOrders);
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

                if(!order.isCompleted)
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