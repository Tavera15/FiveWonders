using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using FiveWonders.WebUI.Models;
using Microsoft.AspNet.Identity.Owin;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class BasketController : Controller
    {
        IBasketServices basketService;
        IRepository<Product> productContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<BasketItem> basketItemContext;

        public BasketController(IBasketServices services, IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartRepository, IRepository<BasketItem> basketItemRepository)
        {
            basketService = services;
            productContext = productsRepository;
            sizeChartContext = sizeChartRepository;
            basketItemContext = basketItemRepository;
        }

        // GET: Basket
        public ActionResult Index()
        {
            List<BasketItemViewModel> allItems = basketService.GetBasketItems(HttpContext);

            foreach(var model in allItems)
            {
                Product product = productContext.Find(model.productID);
                SizeChart sizeChart = sizeChartContext.Find(product.mSizeChart);
                model.productID = product.mID;
                model.productName = product.mName;
                model.image = product.mImage;
                model.price = product.mPrice;

                if (sizeChart != null)
                    model.sizeChart = sizeChart.mSizesToDisplay;
            }

            return View(allItems);
        }

        public ActionResult RemoveFromCart(string Id)
        {
            try
            {
                basketService.RemoveFromBasket(HttpContext, Id);
                return RedirectToAction("Index", "Basket");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Basket");
            }
        }

        public ActionResult EditBasketItem(string Id)
        {
            try
            {
                BasketItem basketItem;
                if (basketService.IsItemInUserBasket(HttpContext, Id, out basketItem))
                {
                    Product product = productContext.Find(basketItem.mProductID);

                    SizeChart sizeChart = null;

                    if (product.mSizeChart != "0")
                        sizeChart = sizeChartContext.Find(product.mSizeChart);

                    BasketItemViewModel viewModel = new BasketItemViewModel()
                    {
                        productID = product.mID,
                        productName = product.mName,
                        price = product.mPrice,
                        image = product.mImage,
                        basketItemID = Id,
                        quantity = basketItem.mQuantity,
                        size = sizeChart != null ? basketItem.mSize : null,
                        sizeChart = sizeChart != null ? sizeChart.mSizesToDisplay : "",
                    };

                    return View(viewModel);
                }

                throw new Exception("Item Not in basket");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditBasketItem(BasketItemViewModel viewModel, string Id)
        {
            try
            {
                BasketItem oldBasketItem = basketItemContext.Find(Id);

                if (!ModelState.IsValid)
                    return View(oldBasketItem);

                BasketItem newBasketItem = new BasketItem()
                {
                    mID = Id,
                    mQuantity = viewModel.quantity,
                    mSize = viewModel.size,
                    mProductID = oldBasketItem.mProductID,
                    basketID = oldBasketItem.basketID
                };

                basketService.UpdateBasketItem(HttpContext, newBasketItem);
                return RedirectToAction("Index", "Basket");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                BasketItem oldBasketItem = basketItemContext.Find(Id);

                return View(oldBasketItem);
            }
        }

        public ActionResult ClearBasket()
        {
            try
            {
                basketService.ClearBasket(HttpContext);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            
            return RedirectToAction("Index", "Basket");
        }
    }
}