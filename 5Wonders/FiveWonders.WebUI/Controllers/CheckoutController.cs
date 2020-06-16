using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class CheckoutController : Controller
    {
        IBasketServices basketService;
        IRepository<FWonderOrder> orderContext;
        IRepository<Customer> customerContext;
        IRepository<Product> productContext;
        IRepository<OrderItem> orderItemContext;

        public CheckoutController(IBasketServices services, IRepository<Customer> customerRepository, IRepository<FWonderOrder> orderRepository, IRepository<Product> productsRepository, IRepository<OrderItem> orderItemRepository)
        {
            basketService = services;
            orderContext = orderRepository;
            customerContext = customerRepository;
            productContext = productsRepository;
            orderItemContext = orderItemRepository;
        }

        // GET: Checkout
        public ActionResult Index()
        {
            try
            {
                List<BasketItemViewModel> allItems = basketService.GetBasketItems(HttpContext);

                if (allItems.Count == 0)
                    throw new Exception("Basket is empty");

                FWonderOrder order = new FWonderOrder();
                string userEmail = HttpContext.User.Identity.Name;

                if (!String.IsNullOrWhiteSpace(userEmail))
                {
                    Customer customer = customerContext.GetCollection().FirstOrDefault(x => x.mEmail == userEmail);

                    if (customer != null)
                    {
                        order.mCustomerName = customer.mFullName;
                        order.mCustomerEmail = userEmail;
                    }
                }

                ViewBag.basketItems = allItems;
                return View(order);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Basket");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(FWonderOrder order)
        {
            try
            {
                decimal total = 0.00m;
                List<BasketItemViewModel> allItems = basketService.GetBasketItems(HttpContext);

                if (!ModelState.IsValid || allItems.Count == 0)
                    throw new Exception("Order Model State is not valid");

                ItemList paypalItems = new ItemList();
                paypalItems.items = new List<Item>();

                // If successful, save order to DB
                foreach (var x in allItems)
                {
                    // Save Info on DB
                    OrderItem orderItem = new OrderItem();
                    orderItem.mBaseOrderID = order.mID;
                    orderItem.mProductID = x.productID;
                    orderItem.mProductName = x.productName;
                    orderItem.mQuantity = x.quantity;
                    orderItem.mPrice = x.price;
                    orderItem.mSize = x.size;
                    total += (x.price * x.quantity);

                    order.mOrderItems.Add(orderItem);

                    // Create an Item object to add to PayPal's List Items Object
                    Item item = new Item()
                    {
                        name = x.productName,
                        description = !String.IsNullOrWhiteSpace(x.size) ? String.Format("Size: {0}", x.size) : "",
                        currency = "USD",
                        price = x.price.ToString(),
                        quantity = x.quantity.ToString()
                    };

                    paypalItems.items.Add(item);
                }

                orderContext.Insert(order);
                orderContext.Commit();

                // TODO PayPal here
                var apiContext = GetApiContext();

                // List of transactions include list of products and total
                var itemTransactions = new List<Transaction>()
                {
                    new Transaction
                    {
                        description = "Five Wonders Test PayPal",
                        amount = new Amount()
                        {
                            currency = "USD",
                            total = total.ToString()
                        },
                        item_list = paypalItems,
                    }
                };

                Payment payment = new Payment();
                payment.experience_profile_id = "";
                payment.intent = "sale";
                payment.payer = new Payer() { payment_method = "paypal" };
                payment.transactions = itemTransactions;
                payment.redirect_urls = new RedirectUrls()
                {
                    cancel_url = Url.Action("CancelOrder", "Checkout", new { orderId = order.mID}, Request.Url.Scheme),
                    return_url = Url.Action("ThankYou", "Checkout", new { orderId = order.mID }, Request.Url.Scheme)
                };

                Payment createdPayment = payment.Create(apiContext);

                var approvalUrl =
                   createdPayment.links.FirstOrDefault(
                       x => x.rel.Equals("approval_url", StringComparison.OrdinalIgnoreCase));

                return Redirect(approvalUrl.href);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return View(order);
            }
        }

        [Route("ThankYou/{orderId}")]
        public ActionResult ThankYou(string orderId, string PaymentId)
        {
            try
            {
                FWonderOrder order = orderContext.Find(orderId);
                if(String.IsNullOrWhiteSpace(order.paypalRef) && !String.IsNullOrWhiteSpace(PaymentId))
                {
                    order.paypalRef = PaymentId;
                    orderContext.Commit();
                    basketService.ClearBasket(HttpContext);

                    return View(order);
                }

                throw new Exception("Order: " + orderId + " already has a PayPal ID");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("CancelOrder/{orderId}")]
        public ActionResult CancelOrder(string orderId)
        {
            try
            {
                FWonderOrder order = orderContext.Find(orderId);

                if(String.IsNullOrWhiteSpace(order.paypalRef))
                {
                    foreach(OrderItem item in order.mOrderItems)
                    {
                        OrderItem itemToDelete = orderItemContext.Find(item.mID);
                        orderItemContext.Delete(itemToDelete);
                    }

                    orderItemContext.Commit();
                    
                    return View();
                }

                throw new Exception("Can't delete order");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Basket");
            }
        }

        private APIContext GetApiContext()
        {
            // Authenticate with PayPal
            var config = ConfigManager.Instance.GetProperties();
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            var apiContext = new APIContext(accessToken);
            return apiContext;
        }

        
    }
}
