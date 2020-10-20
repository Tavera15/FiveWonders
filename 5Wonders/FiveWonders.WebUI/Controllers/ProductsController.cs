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
        [Route(Name = "/{category?}{subcategory?}{productName?}")]
        public ActionResult Index(string category, string[] subcategory, string productName)
        {
            HomePage homePageDefaults = homePageContext.GetCollection().FirstOrDefault();

            if (homePageDefaults == null)
                return HttpNotFound();

            // Read all input
            List<SubCategory> subcategoriesData = new List<SubCategory>();
            Category categoryData = null;

            if (!String.IsNullOrWhiteSpace(category))
            {
                categoryData = categoryContext.GetCollection()
                    .FirstOrDefault(c => c.mCategoryName.ToLower() == category.ToLower());
            }

            if (subcategory != null && subcategory[0] != null)
            {
                foreach (string possibleSubName in subcategory)
                {
                    SubCategory sub = subCategoryContext.GetCollection()
                        .FirstOrDefault(s => s.mSubCategoryName.ToLower() == possibleSubName.ToLower());

                    if (sub == null) { continue; }

                    subcategoriesData.Add(sub);
                }
            }

            try
            {
                ProductsListViewModel viewModel = GetPopularizedProductsViewModel(categoryData, subcategoriesData, productName, homePageDefaults);
                return View(viewModel);
            }
            catch (Exception e)
            {
                _ = e;
                return HttpNotFound();
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

        private Product[] GetProducts(Category categoryData, List<SubCategory> subcategoriesData, string productName)
        {
            Product[] results = productsContext.GetCollection().ToArray();

            try
            {
                if(categoryData != null || subcategoriesData.Count > 0 || !String.IsNullOrWhiteSpace(productName))
                {
                    // Filter by Category
                    if (categoryData != null)
                    {
                        results = results.Where(p => p.mCategory == categoryData.mID).ToArray();
                    }

                    // Filter by Subs
                    if(subcategoriesData.Count > 0)
                    {
                        results = results.Where(p => p.mSubCategories.Split(',')
                            .Any(productSub => subcategoriesData.Any(sub => sub.mID == productSub))).ToArray();
                    }

                    // Filter by Product Name Search
                    if(!String.IsNullOrWhiteSpace(productName))
                    {
                        results = results.Where(p => p.mName.ToLower().Contains(productName.ToLower())).ToArray();
                    }
                }

                return results.OrderByDescending(p => p.mTimeEntered).ToArray();
            }
            catch(Exception e)
            {
                _ = e;
                return new Product[] { };
            }
        }

        private string GetPageTitle(Category category, List<SubCategory> subCategories, string defaultName)
        {
            string pageTitle = defaultName;

            string subTitleName = subCategories.Count == 1
                ? GetProductsListPageTitle(subCategories[0].mSubCategoryName)
                : "";

            string categoryTitleName = category != null
                ? GetProductsListPageTitle(category.mCategoryName)
                : "";

            if (!String.IsNullOrWhiteSpace(categoryTitleName) && !String.IsNullOrWhiteSpace(subTitleName))
            {
                return (categoryTitleName + " - " + subTitleName);
            }
            else if (!String.IsNullOrWhiteSpace(categoryTitleName))
            {
                pageTitle = categoryTitleName;
            }
            else if (!String.IsNullOrWhiteSpace(subTitleName))
            {
                pageTitle = subTitleName;
            }

            return pageTitle;
        }

        private ProductsListViewModel GetPopularizedProductsViewModel(Category category, List<SubCategory> subCategories, string productNameSearch, HomePage homePageDefaults)
        {
            string folderName = "Home";
            string productsListbannerImg = homePageDefaults.mDefaultProductListImgUrl;
            float productListImgShaderAmount = homePageDefaults.defaultBannerImgShader;
            string productListPageTitleColor = homePageDefaults.mdefaultBannerTextColor;
            string productsListPageTitle = GetPageTitle(category, subCategories, homePageDefaults.mDefaultProductsBannerText);

            if (subCategories.Count == 1 && subCategories[0].isEventOrTheme)
            {
                folderName = "SubcategoryImages";
                productsListbannerImg = subCategories[0].mImageUrl;
                productListImgShaderAmount = subCategories[0].mImgShaderAmount;
                productListPageTitleColor = subCategories[0].bannerTextColor;
            }
            else if(category != null)
            {
                folderName = "CategoryImages";
                productsListbannerImg = category.mImgUrL;
                productListImgShaderAmount = category.mImgShaderAmount;
                productListPageTitleColor = category.bannerTextColor;
            }

            ProductsListViewModel viewModel = new ProductsListViewModel()
            {
                folderName = folderName,
                imgUrl = productsListbannerImg,
                mImgShaderAmount = productListImgShaderAmount,
                pageTitle = productsListPageTitle,
                pageTitleColor = productListPageTitleColor,
                products = GetProducts(category, subCategories, productNameSearch)
            };

            return viewModel;
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