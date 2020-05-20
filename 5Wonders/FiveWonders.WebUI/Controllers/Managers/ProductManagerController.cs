using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;

namespace FiveWonders.WebUI.Controllers
{
    public class ProductManagerController : Controller
    {
        IRepository<Product> context;
        IRepository<Category> productCategories;
        IRepository<SubCategory> subCateroryContext;
        IRepository<SizeChart> sizeChartContext;

        public ProductManagerController(IRepository<Product> productContext, IRepository<Category> categoriesContext, IRepository<SubCategory> subCategoryRepository, IRepository<SizeChart> sizeChartRepositories)
        {
            context = productContext;
            productCategories = categoriesContext;
            subCateroryContext = subCategoryRepository;
            sizeChartContext = sizeChartRepositories;
        }

        // Should display all products
        public ActionResult Index()
        {
            List<Product> allProducts = context.GetCollection().ToList<Product>();

            return View(allProducts);
        }

        // Form to create a new product
        public ActionResult Create()
        {
            ProductManagerViewModel viewModel = new ProductManagerViewModel();
            var allSizeCharts = sizeChartContext.GetCollection().ToList();
            allSizeCharts.Insert(0, new SizeChart() { mID = "0", mChartName = "None" });

            viewModel.Product = new Product();
            viewModel.categories = productCategories.GetCollection();
            viewModel.subCategories = subCateroryContext.GetCollection();
            viewModel.sizeCharts = allSizeCharts;

            return View(viewModel);
        }

        // Get form information and store to memory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductManagerViewModel p, string[] selectedCategories)
        {
            if(!ModelState.IsValid)
            {
                return View(p);
            }

            p.Product.mSubCategories = selectedCategories != null ? String.Join(",", selectedCategories) : "" ;

            // Store into memory. Later in Database
            context.Insert(p.Product);
            context.Commit();

            return RedirectToAction("Index", "ProductManager");
        }

        // Display a form to edit product
        public ActionResult Edit(string Id)
        {
            try
            {
                Product productToEdit = context.Find(Id);

                var allSizeCharts = sizeChartContext.GetCollection().ToList();
                allSizeCharts.Insert(0, new SizeChart() { mID = "0", mChartName = "None" });

                ProductManagerViewModel viewModel = new ProductManagerViewModel();
                viewModel.Product = productToEdit;
                viewModel.categories = productCategories.GetCollection();
                viewModel.subCategories = subCateroryContext.GetCollection();
                viewModel.sizeCharts = allSizeCharts;

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
        public ActionResult Edit(ProductManagerViewModel p, string[] selectedCategories, string Id)
        {
            try
            {
                Product target = context.Find(Id);

                if (!ModelState.IsValid)
                    return View(p);

                target.mName = p.Product.mName;
                target.mDesc = p.Product.mDesc;
                target.mPrice = p.Product.mPrice;
                target.mCategory = p.Product.mCategory;
                target.mSizeChart = p.Product.mSizeChart;
                target.mSubCategories = (selectedCategories != null ? String.Join(",", selectedCategories) : "");

                context.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                Product target = context.Find(Id);

                string[] allSubIDs = target.mSubCategories != "" ? target.mSubCategories.Split(',') : new string[] { "None" };
                List<string> allSubNames = new List<string>();

                if(allSubIDs[0] != "None")
                {
                    foreach (var subID in allSubIDs)
                        allSubNames.Add(subCateroryContext.Find(subID).mSubCategoryName);
                }
                else
                {
                    allSubNames.Add("None");
                }

                string mainCategoryName = productCategories.Find(target.mCategory).mCategoryName;
                string sizeChartName = target.mSizeChart != "0" ? sizeChartContext.Find(target.mSizeChart).mChartName : "None";

                ViewBag.CategoryName = mainCategoryName;
                ViewBag.SubCategoryNames = allSubNames.ToArray();
                ViewBag.ChartName = sizeChartName;
                return View(target);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                Product target = context.Find(Id);

                context.Delete(target);
                context.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }
    }
}