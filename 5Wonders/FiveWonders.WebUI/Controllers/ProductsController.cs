using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using FiveWonders.core.Contracts;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;

namespace FiveWonders.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        IRepository<Product> productsContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subCategoryContext;
        IRepository<HomePage> homePageContext;
        IRepository<CustomOptionList> customListContext;
        IBasketServices basketServices;

        public ProductsController(IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subCategoryRepository, IRepository<HomePage> homePageRepository, IRepository<CustomOptionList> customListRepository, IBasketServices basketServices)
        {
            productsContext = productsRepository;
            sizeChartContext = sizeChartsRepository;
            categoryContext = categoryRepository;
            subCategoryContext = subCategoryRepository;
            homePageContext = homePageRepository;
            customListContext = customListRepository;
            this.basketServices = basketServices;
        }

        // GET: Product - Displays every products in the store
        [Route(Name = "/{category?}{subcategory?}")]
        public ActionResult Index(string category, string subcategory)
        {
            HomePage homePageDefaults = homePageContext.GetCollection().FirstOrDefault();

            if (homePageDefaults == null)
                return HttpNotFound();

            const string defaultFolder = "Home";
            string defaultImg = homePageDefaults.mDefaultProductListImgUrl;
            float defaultImgShaderAmount = homePageDefaults.defaultBannerImgShader;
            string defaultPageTitleColor = homePageDefaults.mdefaultBannerTextColor;
            string pageTitle = homePageDefaults.mDefaultProductsBannerText;

            string fixedCategory = GetProductsListPageTitle(category);
            string fixedSubcategory = GetProductsListPageTitle(subcategory);

            if (!String.IsNullOrWhiteSpace(fixedCategory) && !String.IsNullOrWhiteSpace(fixedSubcategory))
            {
                pageTitle = fixedCategory + " - " + fixedSubcategory;
            }
            else if(!String.IsNullOrWhiteSpace(fixedCategory))
            {
                pageTitle = fixedCategory;
            }
            else if(!String.IsNullOrWhiteSpace(fixedSubcategory))
            {
                pageTitle = fixedSubcategory;
            }

            try
            {
                Product[] products = new Product[] { };

                if(category == null && subcategory == null)
                {
                    products = productsContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToArray();
                }
                else
                {
                    Product[] catProd = !String.IsNullOrEmpty(category)
                                    ? GetProductsWithCategory(category)
                                    : new Product[] { };

                    Product[] subProd = !String.IsNullOrEmpty(subcategory)
                                    ? GetProductsWithSub(subcategory)
                                    : new Product[] { };

                    if(!String.IsNullOrEmpty(category) && !String.IsNullOrEmpty(subcategory))
                    {
                        products = catProd.Intersect(subProd)
                            .OrderByDescending(p => p.mTimeEntered).ToArray();
                    }
                    else
                    {
                        products = catProd.Union(subProd)
                            .OrderByDescending(p => p.mTimeEntered).ToArray();
                    }
                }

                Category categoryObj = categoryContext.GetCollection()
                    .Where(c => c.mCategoryName == category.ToLower()).FirstOrDefault();

                SubCategory subcategoryObj = subCategoryContext.GetCollection()
                    .Where(s => s.mSubCategoryName == subcategory.ToLower()).FirstOrDefault();

                ProductsListViewModel viewModel = new ProductsListViewModel
                {
                    pageTitle = pageTitle,
                    products = products,
                    imgUrl = defaultImg,
                    folderName = defaultFolder,
                    mImgShaderAmount = defaultImgShaderAmount,
                    pageTitleColor = defaultPageTitleColor
                };

                if(categoryObj != null && subcategoryObj != null)
                {
                    viewModel.imgUrl = subcategoryObj.isEventOrTheme
                        ? subcategoryObj.mImageUrl
                        : categoryObj.mImgUrL;

                    viewModel.folderName = subcategoryObj.isEventOrTheme
                        ? "SubcategoryImages"
                        : "CategoryImages";

                    viewModel.mImgShaderAmount = subcategoryObj.isEventOrTheme
                        ? subcategoryObj.mImgShaderAmount
                        : categoryObj.mImgShaderAmount;

                    viewModel.pageTitleColor = subcategoryObj.isEventOrTheme
                        ? subcategoryObj.bannerTextColor
                        : categoryObj.bannerTextColor;
                }
                else if (categoryObj != null)
                {
                    viewModel.imgUrl = categoryObj.mImgUrL;
                    viewModel.folderName = "CategoryImages";
                    viewModel.mImgShaderAmount = categoryObj.mImgShaderAmount;
                    viewModel.pageTitleColor = categoryObj.bannerTextColor ?? viewModel.pageTitleColor;
                }
                else if(subcategoryObj != null)
                {
                    viewModel.imgUrl = subcategoryObj.isEventOrTheme 
                        ? subcategoryObj.mImageUrl
                        : viewModel.imgUrl;

                    viewModel.folderName = subcategoryObj.isEventOrTheme
                        ? "SubcategoryImages"
                        : viewModel.folderName;

                    viewModel.mImgShaderAmount = subcategoryObj.isEventOrTheme
                        ? subcategoryObj.mImgShaderAmount
                        : viewModel.mImgShaderAmount;

                    viewModel.pageTitleColor = subcategoryObj.isEventOrTheme
                        ? subcategoryObj.bannerTextColor
                        : viewModel.pageTitleColor;
                }

                return View(viewModel);
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Index", "Products");
            }
        }

        // GET: [/Products/Item/Id] - Returns a page with only one item to buy
        public ActionResult Item(string Id)
        {
            try
            {
                ProductOrderViewModel viewModel = GetProductOrderViewModel(Id);

                ViewBag.Title = viewModel.product.mName;
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
        public ActionResult Item(ProductOrderViewModel item, string Id)
        {
            try
            {
                // Gets the product that the customer wants to buy - along with size and quantity
                Product productToBuy = productsContext.Find(Id, true);
                item.product = productToBuy;
                item.productOrder.mProductID = Id;
                
                // If an input is not valid, return to the item page with warnings and customer selections
                if (!ModelState.IsValid || productToBuy == null || item.productOrder.mQuantity <= 0
                    || !IsValidToAddToCart(productToBuy, item))
                {
                    ProductOrderViewModel failedViewModel = GetProductOrderViewModel(Id);
                    failedViewModel.productOrder = item.productOrder;

                    return View(failedViewModel);
                }

                item.productOrder.mCustomListOptions = JsonConvert.SerializeObject(item.selectedCustomListOptions);

                // Add Product Order to Shopping Cart
                basketServices.AddToBasket(HttpContext, item.productOrder);

                return RedirectToAction("Index", "Basket");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Item", "Products", new { Id = Id});
            }
        }

        public ActionResult SizeChart(string Id)
        {
            try
            {
                Product product = productsContext.Find(Id, true);
                SizeChart chart = sizeChartContext.Find(product.mSizeChart, true);

                return View(chart);
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Item", "Products", new { Id = Id });
            }
        }

        private Product[] GetProductsWithCategory(string categoryName)
        {
            Category category = categoryContext.GetCollection()
                .FirstOrDefault(x => x.mCategoryName.ToLower() == categoryName.ToLower());

            if (category == null)
            {
                throw new Exception("No products contain the category: " + categoryName);
            }

            Product[] productsWithCategory = productsContext.GetCollection()
                .Where(x => x.mCategory == category.mID)
                .OrderByDescending(w => w.mTimeEntered)
                .ToArray();

            return productsWithCategory;
        }

        private Product[] GetProductsWithSub(string subName)
        {
            SubCategory sub = subCategoryContext.GetCollection()
                .FirstOrDefault(x => x.mSubCategoryName.ToLower() == subName.ToLower());

            if (sub == null)
            {
                throw new Exception("No products contain the subcategory: " + subName);
            }

            Product[] productsWithSub = productsContext.GetCollection()
                .Where(prod => prod.mSubCategories.Contains(sub.mID))
                .OrderByDescending(p => p.mTimeEntered)
                .ToArray();

            return productsWithSub;
        }

        private string GetProductsListPageTitle(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) { return ""; }
            
            string fixedTitle = "";
            string[] words = input.Split(' ');
            
            for(int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                fixedTitle += (Char.ToUpper(word[0]) + word.Substring(1).ToLower());
                fixedTitle += (i + 1) == words.Length ? "" : " ";
            }

            return fixedTitle;
        }
    
        private ProductOrderViewModel GetProductOrderViewModel(string Id)
        {
            Product p = productsContext.Find(Id, true);
            SizeChart chart = sizeChartContext.Find(p.mSizeChart);

            ProductOrderViewModel viewModel = new ProductOrderViewModel();
            viewModel.product = p;
            viewModel.productOrder.mProductID = Id;
            viewModel.sizeChart = chart;

            if (!String.IsNullOrWhiteSpace(p.mCustomLists))
            {
                foreach (string listId in p.mCustomLists.Split(','))
                {
                    CustomOptionList customOptionList = customListContext.Find(listId);
                    if (customOptionList == null) { continue; }

                    viewModel.customListNames.Add(customOptionList.mName);
                    viewModel.listOptions.Add(listId, new List<string>());

                    foreach (string customListOption in customOptionList.options.Split(','))
                    {
                        viewModel.listOptions[listId].Add(customListOption);
                    }
                }
            }

            return viewModel;
        }
    
        private bool IsValidToAddToCart(Product product, ProductOrderViewModel possibleItem)
        {
            try
            {
                bool isCustomDateValid = (!product.isDateCustomizable && String.IsNullOrWhiteSpace(possibleItem.productOrder.customDate))
                    ||(product.isDateCustomizable && !String.IsNullOrWhiteSpace(possibleItem.productOrder.customDate));

                bool isCustomTimeValid = (!product.isTimeCustomizable && String.IsNullOrWhiteSpace(possibleItem.productOrder.customTime))
                    ||(product.isTimeCustomizable && !String.IsNullOrWhiteSpace(possibleItem.productOrder.customTime));

                // Checks if a custom number is being submitted on a product that it is not number customizable
                if (!product.isNumberCustomizable && !String.IsNullOrWhiteSpace(possibleItem.productOrder.mCustomNum))
                {
                    throw new Exception("Custom number input not valid");
                }
                else if(product.isNumberCustomizable && !String.IsNullOrWhiteSpace(possibleItem.productOrder.mCustomNum))
                {
                    int customNum;

                    // Checks if the custom number input is a valid integer
                    if(!int.TryParse(possibleItem.productOrder.mCustomNum, out customNum) || customNum < 0)
                    {
                        throw new Exception("Custom number input not valid");
                    }
                }
                
                // Checks if a custom text is being submitted on a product that it is not text customizable
                if(!product.isTextCustomizable && !String.IsNullOrWhiteSpace(possibleItem.productOrder.mCustomText))
                {
                    throw new Exception("Text customization is not allowed.");
                }

                // Makes sure that the product has the custom list still linked, and if the options exist as well
                if(!String.IsNullOrWhiteSpace(product.mCustomLists))
                {
                    // Get list Ids
                    string[] productListIDs = product.mCustomLists.Split(',');
                    
                    foreach(string listID in productListIDs)
                    {
                        // Go through each list, and ensures it exists
                        CustomOptionList customOptionList = customListContext.Find(listID, true);

                        // Check if the customer's selected option exists in the list's options
                        if(!possibleItem.selectedCustomListOptions.ContainsKey(listID)
                            || !customOptionList.options.Contains(possibleItem.selectedCustomListOptions[listID]))
                        {
                            throw new Exception("Option not valid for: " + customOptionList.mName);
                        }
                    }
                }

                return isCustomDateValid && isCustomTimeValid;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }
    }
}