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
using FluentValidation.Results;

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
        IRepository<ProductImage> productImageContext;
        IBasketServices basketServices;

        const int CARDS_PER_PAGE = 12;

        public ProductsController(IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subCategoryRepository, IRepository<HomePage> homePageRepository, IRepository<CustomOptionList> customListRepository, IRepository<ProductImage> productImageRepository, IBasketServices basketServices)
        {
            productsContext = productsRepository;
            sizeChartContext = sizeChartsRepository;
            categoryContext = categoryRepository;
            subCategoryContext = subCategoryRepository;
            homePageContext = homePageRepository;
            customListContext = customListRepository;
            productImageContext = productImageRepository;
            this.basketServices = basketServices;
        }

        // GET: Product - Displays every products in the store
        [Route(Name = "/{category?}{subcategory?}{productName?}{page}")]
        public ActionResult Index(string category, string[] subcategory, string productName, int? page)
        {
            HomePage homePageDefaults = homePageContext.GetCollection().FirstOrDefault();

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
                if(subcategory.Length == 1 && subcategory[0].Contains(","))
                {
                    subcategory = subcategory[0].Split(',');
                }

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
                int pageNumbers;
                ProductsListViewModel viewModel = GetPopularizedProductsViewModel(categoryData, subcategoriesData, homePageDefaults);
                viewModel.products = GetProducts(categoryData, subcategoriesData, productName, page, out pageNumbers);

                ViewBag.CurrentPage = page ?? 1;
                ViewBag.PageNumbers = pageNumbers;
                ViewBag.subs = subcategory != null ? String.Join(",", subcategory) : "";

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

                if(!viewModel.product.isDisplayed)
                {
                    throw new Exception("Product is not available.");
                }

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

                if(!productToBuy.isDisplayed)
                {
                    throw new Exception("Product is not available.");
                }

                // If an input is not valid, return to the item page with warnings and customer selections
                BasketItemValidator basketItemValidator = new BasketItemValidator(productsContext, sizeChartContext, customListContext, item.selectedCustomListOptions);
                ValidationResult validation = basketItemValidator.Validate(item.productOrder);

                if (!ModelState.IsValid || !validation.IsValid || productToBuy == null)
                {
                    ProductOrderViewModel failedViewModel = GetProductOrderViewModel(Id);
                    failedViewModel.productOrder = item.productOrder;

                    string[] errMsg = (validation.Errors != null && validation.Errors.Count > 0)
                        ? validation.Errors.Select(x => x.ErrorMessage).ToArray()
                        : new string[] { "" };

                    ViewBag.errMessages = errMsg;
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

                if (!product.isDisplayed)
                {
                    throw new Exception("Product is not available.");
                }

                SizeChart chart = sizeChartContext.Find(product.mSizeChart, true);

                return View(chart);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
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

        private ProductData[] GetProducts(Category categoryData, List<SubCategory> subcategoriesData, string productName, int? page, out int pageNumbers)
        {
            Product[] allResults = productsContext.GetCollection().Where(p => p.isDisplayed).ToArray();

            try
            {
                if(categoryData != null || subcategoriesData.Count > 0 || !String.IsNullOrWhiteSpace(productName))
                {
                    // Filter by Category
                    if (categoryData != null)
                    {
                        allResults = allResults.Where(p => p.mCategory == categoryData.mID).ToArray();
                    }

                    // Filter by Subs
                    if(subcategoriesData.Count > 0)
                    {
                        allResults = allResults.Where(p => p.mSubCategories.Split(',')
                            .Any(productSub => subcategoriesData.Any(sub => sub.mID == productSub))).ToArray();
                    }

                    // Filter by Product Name Search
                    if(!String.IsNullOrWhiteSpace(productName))
                    {
                        allResults = allResults.Where(p => p.mName.ToLower().Contains(productName.ToLower())).ToArray();
                    }
                }

                // Calculate page number using input from url
                int pageNumber = page ?? 1;
                pageNumber = pageNumber <= 1 ? 1 : pageNumber;

                // Generate the products per page using 'page number' variable
                Product[] results = (pageNumber) <= 1
                    ? allResults.Take(CARDS_PER_PAGE).ToArray()
                    : (allResults.Skip(CARDS_PER_PAGE * (pageNumber - 1)).Take(CARDS_PER_PAGE)).ToArray();

                double rawPageNumbers = ((double)allResults.Length / (double)CARDS_PER_PAGE);
                pageNumbers = (int)(Math.Ceiling(rawPageNumbers));

                List<ProductData> productData = new List<ProductData>();

                foreach(Product p in results)
                {
                    Category cat = categoryContext.Find(p.mCategory, true);
                    ProductData pd = new ProductData();
                    pd.product = p;
                    pd.productCategoryName = cat.mCategoryName;
                    pd.firstImage = productImageContext.Find(p.mImageIDs.Split(',')[0]);

                    productData.Add(pd);
                }

                return productData.ToArray();
            }
            catch(Exception e)
            {
                _ = e;
                pageNumbers = 1;
                return new ProductData[] { };
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

        private ProductsListViewModel GetPopularizedProductsViewModel(Category category, List<SubCategory> subCategories, HomePage homePageDefaults)
        {
            ProductsListViewModel viewModel = null;

            try
            {
                if(homePageDefaults == null)
                {
                    throw new Exception("Home Page defaults null.");
                }

                string productsListbannerImgType = homePageDefaults.mDefaultProductListImgType;
                byte[] productsListbannerImg = homePageDefaults.mDefaultProductListImg;
                float productListImgShaderAmount = homePageDefaults.defaultBannerImgShader;
                string productListPageTitleColor = homePageDefaults.mdefaultBannerTextColor;
                string productsListPageTitle = GetPageTitle(category, subCategories, homePageDefaults.mDefaultProductsBannerText);

                if (subCategories.Count == 1 && subCategories[0].isEventOrTheme)
                {
                    productsListbannerImgType = subCategories[0].mImageType;
                    productsListbannerImg = subCategories[0].mImage;
                    productListImgShaderAmount = subCategories[0].mImgShaderAmount;
                    productListPageTitleColor = subCategories[0].bannerTextColor;
                }
                else if(category != null)
                {
                    productsListbannerImgType = category.mImageType;
                    productsListbannerImg = category.mImage;
                    productListImgShaderAmount = category.mImgShaderAmount;
                    productListPageTitleColor = category.bannerTextColor;
                }

                viewModel = new ProductsListViewModel()
                {
                    imageType = productsListbannerImgType,
                    image = productsListbannerImg,
                    mImgShaderAmount = productListImgShaderAmount,
                    pageTitle = productsListPageTitle,
                    pageTitleColor = productListPageTitleColor,
                };
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                viewModel = new ProductsListViewModel()
                {
                    pageTitle = "Welcome",
                    pageTitleColor = "#34495E"
                };
            }

            return viewModel;
        }
    
        private ProductOrderViewModel GetProductOrderViewModel(string Id)
        {
            Product p = productsContext.Find(Id, true);
            SizeChart chart = sizeChartContext.Find(p.mSizeChart);
            string[] imgIDs = p.mImageIDs != null
                ? p.mImageIDs.Split(',')
                : new string[] { };

            ProductOrderViewModel viewModel = new ProductOrderViewModel();
            viewModel.product = p;
            viewModel.productOrder.mProductID = Id;
            viewModel.sizeChart = chart;
            viewModel.productImages = productImageContext.GetCollection()
                .Where(productImg => imgIDs.Contains(productImg.mID)).ToList();

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
    }
}