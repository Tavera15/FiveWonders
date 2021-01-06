using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
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
        public IRepository<OrderItem> orderItemContext;
        public IRepository<Product> productContext;

        public OrdersController(IRepository<FWonderOrder> ordersRepository, IRepository<Customer> customerRepository, IRepository<OrderItem> orderItemRepository, IRepository<Product> productRepository)
        {
            ordersContext = ordersRepository;
            customerContext = customerRepository;
            orderItemContext = orderItemRepository;
            productContext = productRepository;
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
                FWonderOrder order = ordersContext.Find(Id, true);

                if (!order.isCompleted || VerificationId != order.mVerificationId)
                {
                    throw new Exception("Order not accessible.");
                }

                // Order is linked with customer
                // Check that the right customer is logged in.
                if (!String.IsNullOrWhiteSpace(order.mCustomerId))
                {
                    string possibleCustomerUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    
                    // User is not logged in
                    if(String.IsNullOrWhiteSpace(possibleCustomerUserId))
                    {
                        return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
                    }

                    Customer customer = customerContext.GetCollection()
                        .FirstOrDefault(c => c.mUserID == possibleCustomerUserId);

                    // Wrong customer is logged in
                    if (customer == null || order.mCustomerId != customer.mID)
                    {
                        throw new Exception("Access denied");
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

        [AllowAnonymous]
        [Route(Name = "/{Id}{VerificationId}{baseOrderId}")]
        public ActionResult OrderItemDetails(string Id, string VerificationId, string baseOrderId)
        {
            try
            {
                FWonderOrder order = ordersContext.Find(baseOrderId, true);
                OrderItem orderItem = orderItemContext.Find(Id, true);

                if (!order.isCompleted || VerificationId != order.mVerificationId)
                {
                    throw new Exception("Order Item not accessible.");
                }

                // Order is linked with customer
                // Check that the right customer is logged in.
                if (!String.IsNullOrWhiteSpace(order.mCustomerId))
                {
                    string possibleCustomerUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

                    // User is not logged in
                    if (String.IsNullOrWhiteSpace(possibleCustomerUserId))
                    {
                        return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
                    }

                    Customer customer = customerContext.GetCollection()
                        .FirstOrDefault(c => c.mUserID == possibleCustomerUserId);

                    // Wrong customer is logged in
                    if (customer == null || order.mCustomerId != customer.mID)
                    {
                        throw new Exception("Access denied");
                    }
                }

                Product product = productContext.Find(orderItem.mProductID);

                OrderItemViewModel OIviewModel = new OrderItemViewModel()
                {
                    orderItem = orderItem,
                    verificationId = order.mVerificationId,
                    productImages = product != null && !String.IsNullOrWhiteSpace(product.mImage) 
                        ? product.mImage.Split(',') 
                        : new string[] { }
                };

                Dictionary<string, string> customLists = JsonConvert.DeserializeObject<Dictionary<string, string>>(orderItem.mCustomListOpts);

                ViewBag.customLists = customLists;
                return View(OIviewModel);
            }
            catch(Exception e)
            {
                return HttpNotFound();
            }
        }
    }
}