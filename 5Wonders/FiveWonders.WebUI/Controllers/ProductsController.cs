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

namespace FiveWonders.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        IRepository<Product> productsContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subCategoryContext;
        IBasketServices basketServices;

        public ProductsController(IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subCategoryRepository, IBasketServices basketServices)
        {
            productsContext = productsRepository;
            sizeChartContext = sizeChartsRepository;
            categoryContext = categoryRepository;
            subCategoryContext = subCategoryRepository;
            this.basketServices = basketServices;
        }

        // GET: Product - Displays every products in the store
        [Route(Name = "/{category?}{subcategory?}")]
        public ActionResult Index(string category, string subcategory)
        {
            try
            {
                if(category == null && subcategory == null)
                {
                    Product[] allProducts = productsContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToArray();

                    ViewBag.pageName = "Welcome to 5Wonders!";
                    return View(allProducts);
                }
                else if (category != null && subcategory != null)
                {
                    Product[] productsWithCat = GetProductsWithCategory(category);
                    Product[] productsWithSub = GetProductsWithSub(subcategory);
                    Product[] commonProducts = productsWithCat.Intersect(productsWithSub).ToArray();

                    if (commonProducts.Length == 0)
                        throw new Exception("Products with category [" + category + "] and subcategory [" + subcategory + "] don't exist.");

                    ViewBag.pageName = category + "/" + subcategory;
                    return View(commonProducts);
                }
                else if (category != null)
                {
                    Product[] productsWithCat = GetProductsWithCategory(category);

                    ViewBag.pageName = category;
                    return View(productsWithCat.ToArray());
                }
                else if (subcategory != null)
                {
                    Product[] productsWithSub = GetProductsWithSub(subcategory);

                    ViewBag.pageName = subcategory;
                    return View(productsWithSub.ToArray());
                }
                else
                    throw new Exception("Redirecting to Products page");

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }


        }

        // GET: [/Products/Item/Id] - Returns a page with only one item to buy
        public ActionResult Item(string Id)
        {
            try
            {
                Product p = productsContext.Find(Id);
                SizeChart chart = sizeChartContext.Find(p.mSizeChart);

                ProductOrderViewModel viewModel = new ProductOrderViewModel();
                viewModel.product = p;
                viewModel.productOrder.mProductID = Id;
                viewModel.sizeChart = chart;

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
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            try
            {
                // Gets the product that the customer wants to buy - along with size and quantity
                Product productToBuy = productsContext.Find(Id);
                item.product = productToBuy;
                item.productOrder.mProductID = Id;

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

        public Product[] GetProductsWithCategory(string categoryName)
        {
            Category category = categoryContext.GetCollection().FirstOrDefault(x => x.mCategoryName.ToLower() == categoryName.ToLower());

            if (category == null)
                throw new Exception("No products contain the category: " + categoryName);

            Product[] productsWithCategory = productsContext.GetCollection().Where(x => x.mCategory == category.mID).ToArray();

            return productsWithCategory;
        }

        public Product[] GetProductsWithSub(string subName)
        {
            SubCategory sub = subCategoryContext.GetCollection().FirstOrDefault(x => x.mSubCategoryName.ToLower() == subName.ToLower());

            if (sub == null)
                throw new Exception("No products contain the subcategory: " + subName);

            List<Product> productsWithSub = new List<Product>();

            foreach (var product in productsContext.GetCollection())
            {
                if(product.mSubCategories.Split(',').Contains(sub.mID))
                {
                    productsWithSub.Add(product);
                }
            }

            return productsWithSub.ToArray();
        }
    }
}