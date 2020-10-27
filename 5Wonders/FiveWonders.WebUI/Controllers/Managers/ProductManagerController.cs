using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System.IO;
using FiveWonders.core.Contracts;

namespace FiveWonders.WebUI.Controllers
{
    //[Authorize(Roles = "FWondersAdmin")]
    public class ProductManagerController : Controller
    {
        IRepository<Product> context;
        IRepository<Category> productCategories;
        IRepository<SubCategory> subCateroryContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<CustomOptionList> customOptionListsContext;
        IBasketServices basketService;
        IImageStorageService imageStorageService;

        public ProductManagerController(IRepository<Product> productContext, IRepository<Category> categoriesContext, IRepository<SubCategory> subCategoryRepository, IRepository<SizeChart> sizeChartRepositories, IRepository<CustomOptionList> customListsRepository, IBasketServices basketServices, IImageStorageService imageStorageService)
        {
            context = productContext;
            productCategories = categoriesContext;
            subCateroryContext = subCategoryRepository;
            sizeChartContext = sizeChartRepositories;
            customOptionListsContext = customListsRepository;
            basketService = basketServices;
            this.imageStorageService = imageStorageService;
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
            try
            {
                ProductManagerViewModel viewModel = GetProductManagerVM();
                viewModel.Product = new Product();
                
                return View(viewModel);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "CategoryManager");
            }
        }

        // Get form information and store to memory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductManagerViewModel p, string[] selectedCategories, string[] selectedCustomLists, HttpPostedFileBase[] imageFiles)
        {
            try
            {
                if(!ModelState.IsValid || imageFiles[0] == null 
                    || !AreOptionsValid(p.Product.mCategory, selectedCategories, selectedCustomLists, p.Product.mSizeChart))
                {
                    throw new Exception("Product Create model no good");
                }

                string newImageURL;
                imageStorageService.AddMultipleImages(EFolderName.Products, Server, imageFiles, p.Product.mID, out newImageURL);

                p.Product.mImage = newImageURL;
                p.Product.mSubCategories = selectedCategories != null ? String.Join(",", selectedCategories) : "" ;
                p.Product.mCustomLists = selectedCustomLists != null ? String.Join(",", selectedCustomLists) : "";

                if(p.Product.isTextCustomizable && String.IsNullOrWhiteSpace(p.Product.mCustomText))
                {
                    p.Product.mCustomText = "Custom Text:";
                }

                context.Insert(p.Product);
                context.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                _ = e;
                ProductManagerViewModel viewModel = GetProductManagerVM();
                viewModel.Product = p.Product;

                return View(viewModel);
            }
        }

        // Display a form to edit product
        public ActionResult Edit(string Id)
        {
            try
            {
                Product target = context.Find(Id, true);

                ProductManagerViewModel viewModel = GetProductManagerVM();
                viewModel.Product = target;

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
        public ActionResult Edit(ProductManagerViewModel p, string[] selectedCategories, string[] selectedCustomLists, string[] existingImages, HttpPostedFileBase[] imageFiles, string Id)
        {
            try
            {
                // Find product to edit
                Product target = context.Find(Id, true);

                if ((existingImages == null && imageFiles[0] == null) 
                    || !AreOptionsValid(p.Product.mCategory, selectedCategories, selectedCustomLists, p.Product.mSizeChart))
                {
                    ProductManagerViewModel viewModel = GetProductManagerVM();
                    viewModel.Product = p.Product;
                    viewModel.Product.mImage = target.mImage;
                    viewModel.Product.mSubCategories = selectedCategories != null ? String.Join(",", selectedCategories) : "";
                    viewModel.Product.mCustomLists = selectedCustomLists != null ? String.Join(",", selectedCustomLists) : "";

                    return View(viewModel);
                }

                string strSelectedSubs = selectedCategories != null ? String.Join(",", selectedCategories) : "";
                string strSelectedLists = selectedCustomLists != null ? String.Join(",", selectedCustomLists) : "";

                bool shouldUpdateBaskets = (target.mPrice != p.Product.mPrice
                                           || target.mSizeChart != p.Product.mSizeChart
                                           || target.isNumberCustomizable != p.Product.isNumberCustomizable
                                           || target.isTextCustomizable != p.Product.isTextCustomizable
                                           || target.isDateCustomizable != p.Product.isDateCustomizable
                                           || target.isTimeCustomizable != p.Product.isTimeCustomizable
                                           || target.mCustomLists != strSelectedLists);

                if(shouldUpdateBaskets)
                {
                    basketService.RemoveItemFromAllBaskets(Id);
                }

                if (p.Product.isTextCustomizable && String.IsNullOrWhiteSpace(p.Product.mCustomText))
                {
                    p.Product.mCustomText = "Custom Text:";
                }

                // Set Target's properties to new values
                target.mName = p.Product.mName;
                target.mPrice = p.Product.mPrice;
                target.mCategory = p.Product.mCategory;
                target.mSizeChart = p.Product.mSizeChart;
                target.isNumberCustomizable = p.Product.isNumberCustomizable;
                target.isTextCustomizable = p.Product.isTextCustomizable;
                target.isDateCustomizable = p.Product.isDateCustomizable;
                target.isTextCustomizable = p.Product.isTextCustomizable;
                target.isTimeCustomizable = p.Product.isTimeCustomizable;
                target.mCustomText = p.Product.mCustomText;
                target.mHtmlDesc = p.Product.mHtmlDesc;
                target.mSubCategories = strSelectedSubs;
                target.mCustomLists = strSelectedLists;

                // If new images were selected, update Target's Image property 
                string[] currentImageFiles = target.mImage.Split(',');
                
                if((imageFiles != null && imageFiles[0] != null) ||
                    existingImages == null ||
                    currentImageFiles.Length != existingImages.Length)
                {
                    string newImageURL;
                    // TODO UPDATE Update images
                    UpdateImages(Id, existingImages, imageFiles, out newImageURL);

                    target.mImage = newImageURL;
                }

                context.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "ProductManager", new { Id = Id});
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                Product target = context.Find(Id, true);

                // Gets all the Subcategories' IDs
                string[] allSubIDs = target.mSubCategories != "" 
                    ? target.mSubCategories.Split(',') 
                    : new string[] { "None" };

                // Gets each Subcategory's name using the ID and stores it in an array...if any
                string[] allSubCategoryNames = (allSubIDs[0] != "None") 
                    ? allSubIDs.Select(x => subCateroryContext.Find(x).mSubCategoryName).ToArray() 
                    : new string[] { "None" };

                // Gets each Custom List's name using the ID and stores it in an array...if any
                string[] allCustomListName = (!String.IsNullOrWhiteSpace(target.mCustomLists))
                    ? target.mCustomLists.Split(',').Select(x => customOptionListsContext.Find(x).mName).ToArray()
                    : new string[] { };
                
                // Find the Main Category's and Size Chart's names
                string mainCategoryName = productCategories.Find(target.mCategory).mCategoryName;
                string sizeChartName = target.mSizeChart != "0" ? sizeChartContext.Find(target.mSizeChart).mChartName : "None";

                // Rather than passing IDs, pass the name property to view
                ViewBag.CategoryName = mainCategoryName;
                ViewBag.SubCategoryNames = allSubCategoryNames;
                ViewBag.ChartName = sizeChartName;
                ViewBag.customListsNames = allCustomListName;

                return View(target);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                Product target = context.Find(Id, true);

                basketService.RemoveItemFromAllBaskets(Id);

                string[] currentImageFiles = target.mImage.Split(',');
                imageStorageService.DeleteMultipleImages(EFolderName.Products, currentImageFiles, Server);

                context.Delete(target);
                context.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "ProductManager", new { Id = Id });
            }
        }

        private void UpdateImages(string Id, string[] existingImages, HttpPostedFileBase[] newImageFiles, out string newImageURL)
        {
            // Get current product images
            string[] productImgs = context.Find(Id).mImage.Split(',');

            // Delete images that were not checkboxed
            string[] imagesToDelete = (existingImages == null) 
                ? productImgs
                : productImgs.Where(img => !existingImages.Contains(img)).ToArray();

            imageStorageService.DeleteMultipleImages(EFolderName.Products, imagesToDelete, Server);

            // Initialize - Either new List or with images that were checkboxed
            List<string> allFileNames = existingImages != null 
                ? existingImages.ToList() 
                : new List<string>();

            // Add new images to folder, and store it's file name to List from above
            if(newImageFiles != null && newImageFiles[0] != null)
            {
                foreach (HttpPostedFileBase file in newImageFiles)
                {
                    string fileNameWithoutSpaces = String.Concat(file.FileName.Where(c => !Char.IsWhiteSpace(c)));

                    string fileName = Id + fileNameWithoutSpaces;
                    file.SaveAs(Server.MapPath("//Content//ProductImages//") + fileName);
                    allFileNames.Add(fileName);
                }
            }

            newImageURL = String.Join(",", allFileNames);
        }

        private ProductManagerViewModel GetProductManagerVM()
        {
            List<SizeChart> allSizeCharts = sizeChartContext.GetCollection().ToList();
            allSizeCharts.Insert(0, new SizeChart() { mID = "0", mChartName = "None" });

            IEnumerable<Category> allCategories = productCategories.GetCollection();

            if(allCategories.Count() <= 0)
            {
                throw new Exception("No categories to list");
            }

            // Popularize the view model with all the lists and product to edit
            ProductManagerViewModel viewModel = new ProductManagerViewModel
            {
                categories = allCategories,
                subCategories = subCateroryContext.GetCollection(),
                customOptionLists = customOptionListsContext.GetCollection(),
                sizeCharts = allSizeCharts
            };

            return viewModel;
        }
    
        private bool AreOptionsValid(string categoryId, string[] selectedCategoriesIds, string[] selectedCustomLists, string sizeChartId)
        {
            try
            {
                Category category = productCategories.Find(categoryId, true);
                SizeChart chart = sizeChartId != "0" ? sizeChartContext.Find(sizeChartId, true) : null;

                bool bAllSubsExist = selectedCategoriesIds == null 
                    || selectedCategoriesIds
                        .All(sel => subCateroryContext.GetCollection().Any(sub => sub.mID == sel));

                bool bAllCustomListsExist = selectedCustomLists == null
                    || selectedCustomLists
                        .All(sel => customOptionListsContext.GetCollection().Any(li => li.mID == sel));

                return bAllSubsExist && bAllCustomListsExist;
            }
            catch(Exception e)
            {
                _ = e;
                return false;
            }
        }
    }
}