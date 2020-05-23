using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class ProductsController : Controller
    {

        IRepository<Product> productsContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subCateroryContext;

        public ProductsController(IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subCategoryRepository)
        {
            productsContext = productsRepository;
            sizeChartContext = sizeChartsRepository;
            categoryContext = categoryRepository;
            subCateroryContext = subCategoryRepository;
        }

        // GET: Product
        public ActionResult Index()
        {
            List<Product> allProducts = productsContext.GetCollection().ToList<Product>();
            
            return View(allProducts);
        }

        // GET: [/Products/Item/Id]
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
                Product x = productsContext.Find(Id);
                item.product = x;
                item.productOrder.mProductID = Id;

                // Add Product Order to Shopping Cart

                System.Diagnostics.Debug.WriteLine("Added: " + item.productOrder.mProductID + ": " + item.product.mName);
                System.Diagnostics.Debug.WriteLine("Added: " + item.productOrder.mQuantity + " of them at $" + item.product.mPrice + " each");
                System.Diagnostics.Debug.WriteLine("Size: " + item.productOrder.mSize);
                System.Diagnostics.Debug.WriteLine("Total: $" + (item.productOrder.mQuantity * item.product.mPrice));

                return RedirectToAction("Index", "Products");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Products");
            }
        }

        public ActionResult Category(string Id)
        {
            try
            {
                Category category = categoryContext.GetCollection().FirstOrDefault(x => x.mCategoryName.ToLower() == Id.ToLower());

                if (category == null)
                    throw new Exception("No products contain the category: " + Id);

                Product[] hhh = productsContext.GetCollection().Where(x => x.mCategory == category.mID).ToArray();

                return View(hhh);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }
    }
}