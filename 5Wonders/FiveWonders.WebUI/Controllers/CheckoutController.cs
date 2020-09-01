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
                List<BasketItemViewModel> allBasketItems = basketService.GetBasketItems(HttpContext);

                if (!ModelState.IsValid || allBasketItems.Count == 0)
                    throw new Exception("Order Model State is not valid");

                // Create variables that will be inserted into PayPal Payment object
                ItemList paypalItems = new ItemList();
                paypalItems.items = new List<Item>();
                
                decimal total = 0.00m;

                foreach (BasketItemViewModel basketItem in allBasketItems)
                {
                    // Initialize new Order Item per Basket Item, and link them to Order Id
                    OrderItem orderItem = new OrderItem();
                    orderItem.mBaseOrderID = order.mID;

                    // Apply Basket Item's data into Order Item 
                    orderItem.mProductID = basketItem.productID;
                    orderItem.mProductName = basketItem.product.mName;
                    orderItem.mPrice = basketItem.product.mPrice;
                    orderItem.mQuantity = basketItem.basketItem.mQuantity;
                    orderItem.mSize = basketItem.basketItem.mSize;

                    // Add the Order Item into the Order's list
                    order.mOrderItems.Add(orderItem);

                    total += (basketItem.product.mPrice * basketItem.basketItem.mQuantity);

                    string paypalDesc = String.Format("{0}{1}{2}",
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.mSize) ? "Size: " + basketItem.basketItem.mSize + " | " : ""),
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.mProductText) ? "Text: " + basketItem.basketItem.mProductText + " | " : ""),
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.mCustomNum) ? "Custom Number: " + basketItem.basketItem.mCustomNum : "")
                    );

                    // Create an Item object to add to PayPal's List Items Object
                    Item item = new Item()
                    {
                        name = basketItem.product.mName,
                        currency = "USD",
                        price = basketItem.product.mPrice.ToString(),
                        quantity = basketItem.basketItem.mQuantity.ToString(),
                        description = paypalDesc,
                    };

                    paypalItems.items.Add(item);
                }

                // Save Order object to DB - Should not have PayPal ref. yet
                orderContext.Insert(order);
                orderContext.Commit();
                
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

                // Begin putting together the PayPal Payment object using data from above
                Payment payment = new Payment
                {
                    intent = "sale",
                    payer = new Payer() { payment_method = "paypal" },
                    transactions = itemTransactions,
                    
                    // TODO Don't send order Id in parameters - Maybe add a bool if transaction is complete - ENCRYPT
                    redirect_urls = new RedirectUrls()
                    {
                        cancel_url = Url.Action("CancelOrder", "Checkout", new { orderId = order.mID }, Request.Url.Scheme),
                        return_url = Url.Action("ThankYou", "Checkout", new { orderId = order.mID }, Request.Url.Scheme)
                    }
                };

                // Creates the Payment object - Ready to redirect to PayPal with data entered
                APIContext apiContext = GetApiContext();
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

        [Route("ThankYou/{paymentId}/{payerId}/{orderId}")]
        public ActionResult ThankYou(string paymentId, string payerId, string orderId)
        {
            FWonderOrder order;
            Payment executedPayment;

            try
            {
                order = orderContext.Find(orderId);

                if (order == null)
                    throw new Exception("Order Not found");

                // If order is already completed, display Thank You page
                if (!String.IsNullOrWhiteSpace(order.paypalRef) && order.paypalRef == paymentId)
                    return View(order);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("CancelOrder", "Checkout", new { paymentId = paymentId });
            }

            if(!String.IsNullOrWhiteSpace(order.paypalRef) || String.IsNullOrWhiteSpace(paymentId) || String.IsNullOrWhiteSpace(payerId))
            {
                return RedirectToAction("CancelOrder", "Checkout");
            }
            
            // Try to execute the payment
            try
            {
                APIContext apiContext = GetApiContext();

                PaymentExecution paymentExecution = new PaymentExecution()
                {
                    payer_id = payerId
                };

                // Identify the payment to execute
                Payment payment = new Payment()
                {
                    id = paymentId
                };

                // Execute the Payment - Completes transaction
                executedPayment = payment.Execute(apiContext, paymentExecution);

                if (executedPayment == null)
                    throw new Exception("Executed payment is null");

                // Save PayPal Payment Id to order that's stored in DB
                order.paypalRef = executedPayment.id;
                orderContext.Commit();

                // Empty basket when order is complete, and display Thank You page
                basketService.ClearBasket(HttpContext);
                return View(order);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                return RedirectToAction("CancelOrder", "Checkout");
            }
        }

        public ActionResult CancelOrder(string orderId)
        {
            try
            {
                FWonderOrder order = orderContext.Find(orderId);

                if (order == null || !String.IsNullOrWhiteSpace(order.paypalRef))
                    throw new Exception("Order cannot be deleted");

                return View();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
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
