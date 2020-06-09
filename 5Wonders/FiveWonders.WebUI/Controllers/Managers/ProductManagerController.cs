﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System.IO;

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
            List<Product> allProducts = context.GetCollection().OrderByDescending(x => x.mTimeEntered).ToList();

            return View(allProducts);
        }

        // Form to create a new product
        public ActionResult Create()
        {
            ProductManagerViewModel viewModel = new ProductManagerViewModel();
            var allSizeCharts = sizeChartContext.GetCollection().ToList();

            // TODO Fix the "None" option mID
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
        public ActionResult Create(ProductManagerViewModel p, string[] selectedCategories, HttpPostedFileBase[] imageFiles)
        {
            if(!ModelState.IsValid || imageFiles[0] == null)
            {
                var allSizeCharts = sizeChartContext.GetCollection().ToList();
                allSizeCharts.Insert(0, new SizeChart() { mID = "0", mChartName = "None" });

                p.categories = productCategories.GetCollection();
                p.subCategories = subCateroryContext.GetCollection();
                p.sizeCharts = allSizeCharts;

                return View(p);
            }

            try
            {
                string newImageURL;
                AddImages(p.Product.mID, imageFiles, out newImageURL);

                p.Product.mImage = newImageURL;
                p.Product.mSubCategories = selectedCategories != null ? String.Join(",", selectedCategories) : "" ;

                context.Insert(p.Product);
                context.Commit();
       
                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        // Display a form to edit product
        public ActionResult Edit(string Id)
        {
            try
            {
                Product productToEdit = context.Find(Id);

                // Finds the correct Size Chart and inserts a "None" option
                var allSizeCharts = sizeChartContext.GetCollection().ToList();
                allSizeCharts.Insert(0, new SizeChart() { mID = "0", mChartName = "None" });

                // Popularize the view model with all the lists and product to edit
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
        public ActionResult Edit(ProductManagerViewModel p, string[] selectedCategories, HttpPostedFileBase[] imageFiles, string Id)
        {
            try
            {
                // Find product to edit
                Product target = context.Find(Id);

                // Set Target's properties to new values
                target.mName = p.Product.mName;
                target.mDesc = p.Product.mDesc;
                target.mPrice = p.Product.mPrice;
                target.mCategory = p.Product.mCategory;
                target.mSizeChart = p.Product.mSizeChart;
                target.mSubCategories = (selectedCategories != null ? String.Join(",", selectedCategories) : "");

                // If new images were selected, update Target's Image property 
                if(imageFiles != null && imageFiles[0] != null)
                {
                    var currentImageFiles = target.mImage.Split(',');
                    string newImageURL;

                    DeleteImages(currentImageFiles);
                    AddImages(Id, imageFiles, out newImageURL);

                    target.mImage = newImageURL;
                }

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

                // Gets all the Subcategories' IDs
                string[] allSubIDs = target.mSubCategories != "" ? target.mSubCategories.Split(',') : new string[] { "None" };

                // Gets each Subcategory's name using the ID and stores it in an array...if any
                string[] allSubCategoryNames = (allSubIDs[0] != "None") ? 
                    allSubIDs.Select(x => subCateroryContext.Find(x).mSubCategoryName).ToArray() : new string[] { "None" };
                
                // Find the Main Category's and Size Chart's names
                string mainCategoryName = productCategories.Find(target.mCategory).mCategoryName;
                string sizeChartName = target.mSizeChart != "0" ? sizeChartContext.Find(target.mSizeChart).mChartName : "None";

                // Rather than passing IDs, pass the name property to view
                ViewBag.CategoryName = mainCategoryName;
                ViewBag.SubCategoryNames = allSubCategoryNames;
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
                
                var currentImageFiles = target.mImage.Split(',');
                DeleteImages(currentImageFiles);

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

        private void DeleteImages(string[] currentImageFiles)
        {
            foreach (var file in currentImageFiles)
            {
                string path = Server.MapPath("//Content//ProductImages//") + file;

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(file + " not found");
                }
            }
        }

        private void AddImages(string Id, HttpPostedFileBase[] imageFiles, out string newImageURL)
        {
            var allFileNames = new List<string>();

            foreach (var file in imageFiles)
            {
                string fileName = Id + file.FileName;
                file.SaveAs(Server.MapPath("//Content//ProductImages//") + fileName);
                allFileNames.Add(fileName);
            }

            newImageURL = String.Join(",", allFileNames);
        }
    }
}