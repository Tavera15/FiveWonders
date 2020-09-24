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

            foreach(BasketItemViewModel model in allItems)
            {
                Product product = productContext.Find(model.productID);
                model.productID = product.mID;
                model.product = product;
            }

            return View(allItems);
        }

        public ActionResult RemoveFromCart(string Id)
        {
            try
            {
                basketService.RemoveFromBasket(HttpContext, Id);
            }
            catch(Exception e)
            {
                _ = e;
            }
            
            return RedirectToAction("Index", "Basket");
        }

        public ActionResult EditBasketItem(string Id)
        {
            try
            {
                BasketItemViewModel viewModel = GetSingleBasketItemViewModel(Id);

                if (viewModel == null)
                    throw new Exception("Cannot find basket item");

                return View(viewModel);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditBasketItem(BasketItemViewModel viewModel, string Id)
        {
            try
            {
                if(!ModelState.IsValid || viewModel.basketItem.mQuantity <= 0)
                {
                    BasketItemViewModel errorViewModel = GetSingleBasketItemViewModel(Id);
                    errorViewModel.basketItem = viewModel.basketItem;
                    return View(errorViewModel);
                }

                BasketItem oldBasketItem = basketItemContext.Find(Id, true);

                BasketItem newBasketItem = new BasketItem()
                {
                    mID = Id,
                    mQuantity = viewModel.basketItem.mQuantity,
                    mSize = viewModel.basketItem.mSize,
                    mCustomNum = viewModel.basketItem.mCustomNum,
                    mCustomText = viewModel.basketItem.mCustomText,
                    customDate = viewModel.basketItem.customDate,
                    customTime = viewModel.basketItem.customTime,
                    mProductID = oldBasketItem.mProductID,
                    basketID = oldBasketItem.basketID,
                };

                basketService.UpdateBasketItem(HttpContext, newBasketItem);
                return RedirectToAction("Index", "Basket");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("EditBasketItem", "Basket", new { Id = Id });
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
                _ = e;
            }
            
            return RedirectToAction("Index", "Basket");
        }

        private BasketItemViewModel GetSingleBasketItemViewModel(string Id)
        {
            BasketItemViewModel viewModel = null;
            BasketItem basketItem;

            if (basketService.IsItemInUserBasket(HttpContext, Id, out basketItem))
            {
                Product product = productContext.Find(basketItem.mProductID, true);

                SizeChart sizeChart = null;

                if (product.mSizeChart != "0")
                    sizeChart = sizeChartContext.Find(product.mSizeChart, true);

                viewModel = new BasketItemViewModel()
                {
                    productID = product.mID,
                    product = product,
                    basketItem = basketItem,
                    basketItemID = basketItem.mID,
                    sizeChart = sizeChart
                };
            }

           return viewModel;
        }
    }
}