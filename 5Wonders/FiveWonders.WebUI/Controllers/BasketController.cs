using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using FiveWonders.WebUI.Models;
using FluentValidation.Results;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
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
        IRepository<CustomOptionList> customListContext;
        IRepository<ProductImage> productImageContext;

        public BasketController(IBasketServices services, IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartRepository, IRepository<BasketItem> basketItemRepository, IRepository<CustomOptionList> customListRepository, IRepository<ProductImage> productImageRepository)
        {
            basketService = services;
            productContext = productsRepository;
            sizeChartContext = sizeChartRepository;
            basketItemContext = basketItemRepository;
            productImageContext = productImageRepository;
            customListContext = customListRepository;
        }

        // GET: Basket
        public ActionResult Index()
        {
            List<BasketItemViewModel> allItems = basketService.GetBasketItems(HttpContext);

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
                    mCustomListOptions = JsonConvert.SerializeObject(viewModel.selectedCustomListOptions),
                    mProductID = oldBasketItem.mProductID,
                    basketID = oldBasketItem.basketID,
                };

                BasketItemValidator basketItemValidator = new BasketItemValidator(productContext, sizeChartContext, customListContext, viewModel.selectedCustomListOptions);
                ValidationResult validation = basketItemValidator.Validate(newBasketItem);

                if(!ModelState.IsValid || !validation.IsValid)
                {
                    BasketItemViewModel errorViewModel = GetSingleBasketItemViewModel(Id);
                    errorViewModel.basketItem = viewModel.basketItem;
                    string[] errMsg = (validation.Errors != null && validation.Errors.Count > 0)
                        ? validation.Errors.Select(x => x.ErrorMessage).ToArray()
                        : new string[] { "" };

                    ViewBag.errMessages = errMsg;
                    return View(errorViewModel);
                }

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

                if (!String.IsNullOrWhiteSpace(product.mSizeChart))
                {
                    sizeChart = sizeChartContext.Find(product.mSizeChart, true);
                }

                // Get custom lists that may be linked to product
                Dictionary<string, List<string>> productCustomLists = new Dictionary<string, List<string>>();
                List<string> customListNames = new List<string>();

                if (!String.IsNullOrWhiteSpace(product.mCustomLists))
                {
                    foreach(string listId in product.mCustomLists.Split(','))
                    {
                        CustomOptionList customList = customListContext.Find(listId);

                        if(customList == null) { continue; }

                        // Add name to list, and init dictionary using custom list Id
                        customListNames.Add(customList.mName);
                        productCustomLists.Add(listId, new List<string>());

                        // Go through each custom list option, and add it to dictionary
                        foreach(string listOpt in customList.options.Split(','))
                        {
                            productCustomLists[listId].Add(listOpt);
                        }
                    }
                }

                string[] productIds = !String.IsNullOrWhiteSpace(product.mImageIDs)
                    ? product.mImageIDs.Split(',')
                    : new string[] { };

                viewModel = new BasketItemViewModel()
                {
                    productID = product.mID,
                    product = product,
                    basketItem = basketItem,
                    basketItemID = basketItem.mID,
                    sizeChart = sizeChart,
                    listOptions = productCustomLists,
                    customListNames = customListNames,
                    productImages = productImageContext.GetCollection()
                        .Where(proImg => productIds.Contains(proImg.mID)).ToList(),
                    selectedCustomListOptions = 
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(basketItem.mCustomListOptions)
                };
            }

           return viewModel;
        }
    }
}