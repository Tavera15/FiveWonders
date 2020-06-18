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
        IRepository<FWonderOrder> orderContext;

        public const string basketCookieName = "FiveWondersBasket";
        public const double cookieDuration = 30;        // In Days ex. 30 days

        public BasketServices(IRepository<Product> productsRepository, IRepository<Basket> basketRepository, IRepository<Customer> customerRepository, IRepository<BasketItem> basketItemsRepository, IRepository<FWonderOrder> orderRepository)
        {
            productsContext = productsRepository;
            basketContext = basketRepository;
            customerContext = customerRepository;
            basketItemsContext = basketItemsRepository;
            orderContext = orderRepository;
        }

        public void AddToBasket(HttpContextBase httpContext, BasketItem item)
        {
            try
            {
                Basket basket = GetBasket(httpContext, true);

                // Checks if a similar item is already in the basket
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
            Basket basket = GetBasket(httpContext, true);

            // Gets Basket Items where the Product ID matches the Product ID that's stored in Basket Item object
            var items = (from b in basket.mBasket
                            join p in productsContext.GetCollection() on b.mProductID equals p.mID
                            select new BasketItemViewModel()
                            {
                                basketItemID = b.mID,
                                quantity = b.mQuantity,
                                size = b.mSize,
                                productID = p.mID,
                                image = p.mImage,
                                price = p.mPrice,
                                productName = p.mName,
                                sizeChart = p.mSizeChart
                            }
                            ).ToList();
                
            return items;
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

        public void ClearBasket(HttpContextBase httpContext)
        {
            try
            {
                Basket basket = GetBasket(httpContext, false);
                
                foreach(var item in basket.mBasket)
                {
                    BasketItem basketItemToDelete = basketItemsContext.Find(item.mID);
                    basketItemsContext.Delete(basketItemToDelete);
                }

                basketItemsContext.Commit();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
            string userID = HttpContext.Current.User.Identity.GetUserId();
            HttpCookie cookie = httpContext.Request.Cookies[basketCookieName];
            Customer customer;

            // Customer is logged in
            if(!String.IsNullOrWhiteSpace(userID) && IsUserACustomer(userID, out customer))
            {
                if (customer.mBasketID != null)
                {
                    return customer.mBasketID;
                }
                else if(bCreateIfNull && cookie != null && CanMigrateBasket(customer.mID, cookie.Value))
                {
                    // CanMigrateBasket links the customer with the Basket ID that was found in the cookie
                    return customer.mBasketID;
                }

                return bCreateIfNull ? CreateNewBasket(httpContext, customer.mID).mID : null;
            }
            else
            {
                // User is not logged in - Check if cookie contains a basket
                if (cookie != null && IsValidBasketID(cookie.Value))
                {
                    // Check if basket is not owned by anyone else
                    if(!customerContext.GetCollection().Any(x => x.mBasketID == cookie.Value))
                    {
                        cookie.Expires = DateTime.Now.AddDays(cookieDuration);
                        httpContext.Response.SetCookie(cookie);

                        return cookie.Value;
                    }
                }

                return bCreateIfNull ? CreateNewBasket(httpContext).mID : null;
            }
        }

        private bool CanMigrateBasket(string customerID, string basketID)
        {
            try
            {
                Customer customer = customerContext.Find(customerID);

                if (customer.mBasketID != null || !IsValidBasketID(basketID) || customerContext.GetCollection().Any(x => x.mBasketID == basketID))
                    return false;

                Basket basket = basketContext.Find(basketID);
                customer.mBasketID = basketID;
                customerContext.Commit();

                return true;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
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
            customer = customerContext.GetCollection().Where(x => x.mUserID == userID).FirstOrDefault() ?? null;
            
            return customer != null;
        }
    }
}
