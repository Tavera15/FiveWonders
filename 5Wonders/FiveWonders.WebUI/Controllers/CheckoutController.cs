using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
        IRepository<CustomOptionList> customListContext;

        public CheckoutController(IBasketServices services, IRepository<Customer> customerRepository, IRepository<FWonderOrder> orderRepository, IRepository<Product> productsRepository, IRepository<OrderItem> orderItemRepository, IRepository<CustomOptionList> customListRepository)
        {
            basketService = services;
            orderContext = orderRepository;
            customerContext = customerRepository;
            productContext = productsRepository;
            orderItemContext = orderItemRepository;
            customListContext = customListRepository;
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
                _ = e;
                return RedirectToAction("Index", "Basket");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(FWonderOrder order)
        {
            try
            {
                // Get user id, if any, from the customer during checkout
                string possibleCustomerUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

                // Delete any cancelled or incomplete orders from the customer 
                if(!String.IsNullOrWhiteSpace(possibleCustomerUserId))
                {
                    Customer customer = customerContext.GetCollection()
                        .FirstOrDefault(c => c.mUserID == possibleCustomerUserId);

                    if(customer != null)
                    {
                        ClearExistingCustomerOrders(customer.mID, order.mID);
                        order.mCustomerId = customer.mID;
                    }
                }

                List<BasketItemViewModel> allBasketItems = basketService.GetBasketItems(HttpContext);

                if (!ModelState.IsValid || allBasketItems.Count <= 0)
                    throw new Exception("Order Model State is not valid");

                // Create variables that will be inserted into PayPal Payment object
                ItemList paypalItems = new ItemList();
                paypalItems.items = new List<Item>();
                
                decimal total = 0.00m;

                foreach (BasketItemViewModel basketItem in allBasketItems)
                {
                    // Variable to store all custom list selections. Will be used in PayPal's Item Description prop
                    string itemCustomListDesc = "";
                    
                    // list name, selected value
                    Dictionary<string, string> orderItemListOpts = new Dictionary<string, string>();
                    
                    if(!String.IsNullOrWhiteSpace(basketItem.basketItem.mCustomListOptions))
                    {
                        // Get the Custom Option Lists selections from the basket item
                        Dictionary<string, string> basketItemSelectedListOpts =
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(basketItem.basketItem.mCustomListOptions);

                        foreach(string listID in basketItemSelectedListOpts.Keys)
                        {
                            CustomOptionList optionList = customListContext.Find(listID, true);
                            
                            orderItemListOpts.Add(optionList.mName, basketItemSelectedListOpts[listID]);

                            string currentListAndInput = String.Format("{0}: {1}", optionList.mName, basketItemSelectedListOpts[listID]);
                            
                            itemCustomListDesc = String.IsNullOrWhiteSpace(itemCustomListDesc)
                                ? currentListAndInput
                                : itemCustomListDesc + " | " + currentListAndInput; 
                        }
                    }

                    // Initialize new Order Item per Basket Item, and link them to Order Id
                    OrderItem orderItem = new OrderItem
                    {
                        mBaseOrderID = order.mID,

                        // Apply Basket Item's data into Order Item 
                        mProductID = basketItem.productID,
                        mProductName = basketItem.product.mName,
                        mPrice = basketItem.product.mPrice,
                        mQuantity = basketItem.basketItem.mQuantity,
                        mSize = basketItem.basketItem.mSize,
                        mCustomText = basketItem.basketItem.mCustomText,
                        mCustomNumber = basketItem.basketItem.mCustomNum,
                        mCustomDate = basketItem.basketItem.customDate,
                        mCustomTime = basketItem.basketItem.customTime,
                        mCustomListOpts = orderItemListOpts.Count == 0 
                                        ? "" 
                                        : JsonConvert.SerializeObject(orderItemListOpts) 
                    };

                    // Add the Order Item into the Order's list
                    order.mOrderItems.Add(orderItem);

                    total += (basketItem.product.mPrice * basketItem.basketItem.mQuantity);

                    string paypalDesc = String.Format("{0}{1}{2}{3}{4}{5}",
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.mSize) ? "Size: " + basketItem.basketItem.mSize + " | " : ""),
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.mCustomText) ? "Text: " + basketItem.basketItem.mCustomText + " | " : ""),
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.mCustomNum) ? "Custom Number: " + basketItem.basketItem.mCustomNum : ""),
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.customDate) ? "Custom Date: " + basketItem.basketItem.customDate + " | " : ""),
                        (!String.IsNullOrWhiteSpace(basketItem.basketItem.customTime) ? "Custom Time: " + basketItem.basketItem.customTime + " | " : ""),
                        (itemCustomListDesc)
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
                    
                    redirect_urls = new RedirectUrls()
                    {
                        cancel_url = Url.Action("Cancel", "Checkout", new { orderId = order.mID }, Request.Url.Scheme),
                        return_url = Url.Action("Confirm", "Checkout", new { orderId = order.mID }, Request.Url.Scheme)
                    }
                };

                // Creates the Payment object - Ready to redirect to PayPal with data entered
                APIContext apiContext = GetApiContext();
                Payment createdPayment = payment.Create(apiContext);

                var approvalUrl =
                   createdPayment.links.FirstOrDefault(
                       x => x.rel.Equals("approval_url", StringComparison.OrdinalIgnoreCase));

                // TODO Send confirmation to admins

                return Redirect(approvalUrl.href);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.InnerException.Message);
                _ = e;
                return RedirectToAction("Index", "Checkout");
            }
        }

        [Route("Confirm/{paymentId}/{payerId}/{orderId}")]
        public ActionResult Confirm(string paymentId, string payerId, string orderId)
        {
            try
            {
                FWonderOrder order = orderContext.Find(orderId, true);
                APIContext apiContext = GetApiContext();

                PaymentExecution paymentExecution = new PaymentExecution()
                {
                    payer_id = payerId
                };

                Payment payment = new Payment()
                {
                    id = paymentId
                };

                Payment executedPayment = payment.Execute(apiContext, paymentExecution);

                basketService.ClearBasket(HttpContext);
                order.isCompleted = true;
                order.mVerificationId = Guid.NewGuid().ToString();

                orderContext.Commit();

                string adminUrl = "https://localhost:44372/OrdersManager/Details/?Id=" + order.mID;
                string customerUrl = "https://localhost:44372/Orders/Details/?Id=" + order.mID + "&VerificationId=" + order.mVerificationId;

                System.Diagnostics.Debug.WriteLine("Admin: " + adminUrl);
                System.Diagnostics.Debug.WriteLine("Customer: " + customerUrl);

                /*
                // TODO Send Confirmation to admin and customer...?
                MailMessage adminMessage = GetFilledMailedMessage(order.mID, "5Wonders Order Confirmation" + order.mID, adminUrl);
                MailMessage customerMessage = GetFilledMailedMessage(order.mID, "5Wonders Order Confirmation" + order.mID, customerUrl);

                //throw new Exception("stop");

                SmtpClient smtp = new SmtpClient();
                smtp.Send(message);
                 */

                return RedirectToAction("ThankYou", "Checkout");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Cancel", "Checkout", new { orderId = orderId });
            }
        }

        public ActionResult ThankYou()
        {
            return View();
        }

        public ActionResult Cancel(string orderId)
        {
            try
            {
                if(!String.IsNullOrWhiteSpace(orderId))
                {
                    FWonderOrder order = orderContext.Find(orderId, true);

                    if(!order.isCompleted)
                    {
                        // Find Order, clear order items, and delete it first
                        order.mOrderItems.Clear();
                        orderContext.Delete(order);
                        orderContext.Commit();

                        // Delete each order item
                        foreach(OrderItem orderItem in orderItemContext.GetCollection().Where(o => o.mBaseOrderID == orderId))
                        {
                            OrderItem itemToDelete = orderItemContext.Find(orderItem.mID, true);
                            orderItemContext.Delete(itemToDelete);
                        }

                        orderItemContext.Commit();
                    }
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return RedirectToAction("CancelOrder", "Checkout");
        }

        public ActionResult CancelOrder()
        {
            return View();
        }

        private MailMessage GetFilledMailedMessage(string orderId, string subject, string orderUrl)
        {
            // On five wonder order: attach another separate Id in order to validate anonymous people looking at their order
            
            MailMessage message = new MailMessage();
            message.To.Add("");
            message.From = new MailAddress("");
            message.Subject = subject + ": " + orderId;
            message.IsBodyHtml = false;
            message.Body = "View order details: " + orderUrl;

            throw new Exception("stop");

        }

        private void ClearExistingCustomerOrders(string customerId, string orderId)
        {
            if(!String.IsNullOrWhiteSpace(customerId))
            {
                FWonderOrder[] ordersToDelete = orderContext.GetCollection()
                    .Where(fwo => !fwo.isCompleted && fwo.mCustomerId == customerId && fwo.mID != orderId)
                    .ToArray();

                if(ordersToDelete.Length > 0)
                {
                    foreach(FWonderOrder order in ordersToDelete)
                    {
                        string tempOrderId = order.mID;
                        FWonderOrder orderToDelete = orderContext.Find(tempOrderId);
                        orderToDelete.mOrderItems.Clear();

                        if(orderToDelete == null) { continue; }

                        orderContext.Delete(orderToDelete);
                        orderContext.Commit();

                        OrderItem[] orderItemsToDelete = orderItemContext.GetCollection()
                            .Where(item => item.mBaseOrderID == tempOrderId).ToArray();

                        if(orderItemsToDelete.Length > 0)
                        {
                            foreach(OrderItem item in orderItemsToDelete)
                            {
                                OrderItem orderItemToDelete = orderItemContext.Find(item.mID);

                                if(orderItemToDelete == null) { continue; }

                                orderItemContext.Delete(orderItemToDelete);
                            }

                            orderItemContext.Commit();
                        }
                    }

                }
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
