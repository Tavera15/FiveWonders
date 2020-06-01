using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using FiveWonders.core.ViewModels;
using FiveWonders.core.Contracts;

namespace FiveWonders.Services
{
    public class BasketServices : IBasketServices
    {
        IRepository<Product> productsContext;
        IRepository<Basket> basketContext;
        IRepository<BasketItem> basketItemsContext;
        IRepository<Customer> customerContext;

        public const string basketCookieName = "FiveWondersBasket";
        public const double cookieDuration = 30;

        public BasketServices(IRepository<Product> productsRepository, IRepository<Basket> basketRepository, IRepository<Customer> customerRepository, IRepository<BasketItem> basketItemsRepository)
        {
            productsContext = productsRepository;
            basketContext = basketRepository;
            customerContext = customerRepository;
            basketItemsContext = basketItemsRepository;
        }

        public void AddToBasket(HttpContextBase httpContext, BasketItem item)
        {
            try
            {
                Basket basket = GetBasket(httpContext, true);

                // Checks if item is already in the basket
                BasketItem basketItem = basket.mBasket.
                    FirstOrDefault(x => x.mProductID == item.mProductID && x.mSize == item.mSize);

                // Update the quantity if item already exists
                if (basketItem != null)
                {
                    basketItem.mQuantity += item.mQuantity;
                }
                else
                {
                    // Add new item to basket
                    basketItem = item;
                    basketItem.basketID = basket.mID;
                    basket.mBasket.Add(basketItem);
                }

                // Save changes
                basketContext.Commit();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public void RemoveFromBasket(HttpContextBase httpContext, string itemToRemoveID)
        {
            try
            {
                Basket basket = GetBasket(httpContext, false);

                if (basket == null)
                {
                    throw new Exception("Basket not found");
                }

                BasketItem item = basketItemsContext.Find(itemToRemoveID);

                if (item == null)
                {
                    throw new Exception("Item was not found");
                }

                basketItemsContext.Delete(item);
                basketItemsContext.Commit();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public List<BasketItemViewModel> GetBasketItems(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            
            if(basket != null)
            {
                // Gets Basket Items where the Product ID matches the Product ID that's stored in Basket Item object
                var items = (from b in basket.mBasket
                             join p in productsContext.GetCollection() on b.mProductID equals p.mID
                             select new BasketItemViewModel()
                             {
                                 basketItemID = b.mID,
                                 quantity = b.mQuantity,
                                 size = b.mSize,
                                 productID = p.mID,
                             }
                             ).ToList();
                
                return items;
            }

            return new List<BasketItemViewModel>();
        }

        public void UpdateBasketItem(HttpContextBase httpContext, BasketItem newBasketItem)
        {
            try
            {
                // Check if items can be combined and only update the quantity
                BasketItem similarBasketItem = basketItemsContext.GetCollection()
                    .FirstOrDefault(x => x.mProductID == newBasketItem.mProductID
                                    && x.mSize == newBasketItem.mSize
                                    && x.basketID == newBasketItem.basketID
                                    && x.mID != newBasketItem.mID);

                if(similarBasketItem != null)
                {
                    // Update quantity
                    similarBasketItem.mQuantity += newBasketItem.mQuantity;
                    similarBasketItem.mSize = newBasketItem.mSize;

                    // Delete the newest Basket Item entry from DB
                    BasketItem basketItemToDelete = basketItemsContext.Find(newBasketItem.mID);
                    basketItemsContext.Delete(basketItemToDelete);
                }
                else
                {
                    // Update singular Basket Item in DB
                    BasketItem basketItem = basketItemsContext.Find(newBasketItem.mID);
                    basketItem.mQuantity = newBasketItem.mQuantity;
                    basketItem.mSize = newBasketItem.mSize;
                }
                
                basketItemsContext.Commit();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public bool IsItemInUserBasket(HttpContextBase httpContext, string basketItemID, out BasketItem basketItem)
        {
            try
            {
                Basket basket = GetBasket(httpContext, false);

                if (basket == null)
                    throw new Exception("User has no basket");

                basketItem = basketItemsContext.Find(basketItemID);

                return (basketItem != null && basket.mID == basketItem.basketID);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                basketItem = null;
                return false;
            }
        }

        private Basket GetBasket(HttpContextBase httpContext, bool bCreateIfNull)
        {
            try
            {
                string basketID = GetBasketID(httpContext, bCreateIfNull);
                Basket basket = basketContext.Find(basketID);

                return basket;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return new Basket();
            }
        }

        private Basket CreateNewBasket(HttpContextBase httpContext, string customerID = null)
        {
            Basket basket = new Basket();

            // Link the basket to the customer
            if (customerID != null)
            {
                Customer customer = customerContext.Find(customerID);
                customer.mBasketID = basket.mID;

                customerContext.Commit();
            }

            basketContext.Insert(basket);
            basketContext.Commit();

            // Create cookie
            HttpCookie cookie = new HttpCookie(basketCookieName, basket.mID);
            cookie.Expires = DateTime.Now.AddDays(cookieDuration);
            httpContext.Response.Cookies.Add(cookie);

            return basket;
        }

        private string GetBasketID(HttpContextBase httpContext, bool bCreateIfNull)
        {
            // Search for cookie. User is either logged in or anonymous
            HttpCookie cookie = httpContext.Request.Cookies[basketCookieName];

            // Extend expiration date, and returns cookie's value if it's a valid basket ID
            if (cookie != null && IsValidBasketID(cookie.Value))
            {
                cookie.Expires = DateTime.Now.AddDays(cookieDuration);
                httpContext.Response.SetCookie(cookie);

                return cookie.Value;
            }

            // If user is logged in, get the user ID and find their customer record in DB
            string userID = HttpContext.Current.User.Identity.GetUserId();
            Customer customer;

            // If customer record is found, return the basket ID. Has option to create a new basket if null or return null
            if (IsUserACustomer(userID, out customer))
            {
                if (customer.mBasketID == null && bCreateIfNull)
                {
                    return CreateNewBasket(httpContext, customer.mID).mID;
                }

                return customer.mBasketID;
            }

            // User is a new anonymous user.
            return bCreateIfNull ? CreateNewBasket(httpContext).mID : null;
        }

        private bool IsValidBasketID(string basketID)
        {
            try
            {
                Basket basket = basketContext.Find(basketID);

                return basket != null;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }

        private bool IsUserACustomer(string userID, out Customer customer)
        {
            try
            {
                customer = customerContext.GetCollection().Where(x => x.mUserID == userID).FirstOrDefault();

                return customer != null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                customer = null;
                return false;
            }
        }
    }
}
