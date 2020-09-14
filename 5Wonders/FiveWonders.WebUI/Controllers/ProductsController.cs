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

namespace FiveWonders.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        IRepository<Product> productsContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subCategoryContext;
        IRepository<HomePage> homePageContext;
        IBasketServices basketServices;

        public ProductsController(IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subCategoryRepository, IRepository<HomePage> homePageRepository, IBasketServices basketServices)
        {
            productsContext = productsRepository;
            sizeChartContext = sizeChartsRepository;
            categoryContext = categoryRepository;
            subCategoryContext = subCategoryRepository;
            homePageContext = homePageRepository;
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

                // TODO Fix this. Solo Subcategories display nothing
                return View(viewModel);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Products");
            }
        }

        // GET: [/Products/Item/Id] - Returns a page with only one item to buy
        public ActionResult Item(string Id)
        {
            try
            {
                Product p = productsContext.Find(Id);

                if(p == null)
                {
                    throw new Exception("Product not found");
                }

                SizeChart chart = sizeChartContext.Find(p.mSizeChart);

                ProductOrderViewModel viewModel = new ProductOrderViewModel();
                viewModel.product = p;
                viewModel.productOrder.mProductID = Id;
                viewModel.sizeChart = chart;

                ViewBag.Title = p.mName;
                return View(viewModel);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
                Product productToBuy = productsContext.Find(Id);
                item.product = productToBuy;
                item.productOrder.mProductID = Id;
                
                int tempCustom;

                if (!ModelState.IsValid || item.productOrder.mQuantity <= 0 || (item.product.isNumberCustomizable && !int.TryParse(item.productOrder.mCustomNum, out tempCustom)))
                {
                    return View(item);
                }

                // Add Product Order to Shopping Cart
                basketServices.AddToBasket(HttpContext, item.productOrder);

                return RedirectToAction("Index", "Products");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                ViewBag.itemQuantity = 0;
                return RedirectToAction("Index", "Products");
            }
        }

        public ActionResult SizeChart(string Id)
        {
            try
            {
                Product product = productsContext.Find(Id);

                if (product == null || String.IsNullOrWhiteSpace(product.mSizeChart))
                    throw new Exception("No Size Chart");

                SizeChart chart = sizeChartContext.Find(product.mSizeChart);

                return View(chart);
            }
            catch(Exception e)
            {
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
    }
}