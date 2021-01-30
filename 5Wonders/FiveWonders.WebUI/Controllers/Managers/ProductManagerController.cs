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
using FiveWonders.Services;
using FluentValidation.Results;


namespace FiveWonders.WebUI.Controllers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class ProductManagerController : Controller
    {
        IRepository<Product> productsContext;
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subCategoryContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<CustomOptionList> customOptionListsContext;
        IRepository<ProductImage> productImageContext;
        IBasketServices basketService;

        public ProductManagerController(IRepository<Product> productRepository, IRepository<Category> categoriesRepository, IRepository<SubCategory> subCategoryRepository, IRepository<SizeChart> sizeChartRepositories, IRepository<CustomOptionList> customListsRepository, IRepository<ProductImage> productImageRepository, IBasketServices basketServices)
        {
            productsContext = productRepository;
            categoryContext = categoriesRepository;
            subCategoryContext = subCategoryRepository;
            sizeChartContext = sizeChartRepositories;
            customOptionListsContext = customListsRepository;
            productImageContext = productImageRepository;
            basketService = basketServices;
        }

        // Should display all products
        public ActionResult Index()
        {
            List<Product> allProducts = productsContext.GetCollection().ToList();

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
                p.Product.mSubCategories = selectedCategories != null ? String.Join(",", selectedCategories) : "";
                p.Product.mCustomLists = selectedCustomLists != null ? String.Join(",", selectedCustomLists) : "";
                
                ProductsValidator productValidator = new ProductsValidator(categoryContext, subCategoryContext, sizeChartContext, customOptionListsContext, imageFiles);
                ValidationResult validation = productValidator.Validate(p.Product);

                if (!ModelState.IsValid || !validation.IsValid)
                {
                    throw new Exception(String.Join(",", validation.Errors));
                }

                List<string> imgIDs = new List<string>();

                foreach(HttpPostedFileBase image in imageFiles)
                {
                    ProductImage newProductImage = new ProductImage();
                    newProductImage.mImage = ImageStorageService.GetImageBytes(image);
                    newProductImage.mImageType = ImageStorageService.GetImageExtension(image);

                    imgIDs.Add(newProductImage.mID);
                    productImageContext.Insert(newProductImage);
                }

                productImageContext.Commit();
                p.Product.mImageIDs = String.Join(",", imgIDs);

                if(p.Product.isTextCustomizable && String.IsNullOrWhiteSpace(p.Product.mCustomText))
                {
                    p.Product.mCustomText = "Custom Text:";
                }

                productsContext.Insert(p.Product);
                productsContext.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                ViewBag.errMessages = e.Message.Split(',');
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
                Product target = productsContext.Find(Id, true);

                ProductManagerViewModel viewModel = GetProductManagerVM();
                viewModel.Product = target;

                if(!String.IsNullOrWhiteSpace(target.mImageIDs))
                {
                    viewModel.productImages = GetProductImages(target.mImageIDs);
                }

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
                Product target = productsContext.Find(Id, true);

                // Validate new inputs
                p.Product.mSubCategories = selectedCategories != null ? String.Join(",", selectedCategories) : "";
                p.Product.mCustomLists = selectedCustomLists != null ? String.Join(",", selectedCustomLists) : "";
                p.Product.mImageIDs = existingImages != null ? String.Join(",", existingImages) : "";

                ProductsValidator productValidator = new ProductsValidator(categoryContext, subCategoryContext, sizeChartContext, customOptionListsContext, imageFiles);
                ValidationResult validation = productValidator.Validate(p.Product);

                if(!ModelState.IsValid || !validation.IsValid)
                {
                    ProductManagerViewModel viewModel = GetProductManagerVM();
                    viewModel.productImages = GetProductImages(target.mImageIDs);
                    viewModel.Product = p.Product;
                    viewModel.Product.mImageIDs = target.mImageIDs;
                    viewModel.Product.mSubCategories = selectedCategories != null ? String.Join(",", selectedCategories) : "";
                    viewModel.Product.mCustomLists = selectedCustomLists != null ? String.Join(",", selectedCustomLists) : "";

                    ViewBag.errMessages = String.Join(",", validation.Errors).Split(',');
                    return View(viewModel);
                }

                bool shouldUpdateBaskets = (target.mPrice != p.Product.mPrice
                                           || target.isDisplayed != p.Product.isDisplayed
                                           || target.mSizeChart != p.Product.mSizeChart
                                           || target.isNumberCustomizable != p.Product.isNumberCustomizable
                                           || target.isTextCustomizable != p.Product.isTextCustomizable
                                           || target.isDateCustomizable != p.Product.isDateCustomizable
                                           || target.isTimeCustomizable != p.Product.isTimeCustomizable
                                           || target.mCustomLists != p.Product.mCustomLists);

                if(shouldUpdateBaskets)
                {
                    basketService.RemoveItemFromAllBaskets(Id);
                }

                if (p.Product.isTextCustomizable && String.IsNullOrWhiteSpace(p.Product.mCustomText))
                {
                    p.Product.mCustomText =  "Custom Text:";
                }

                // Update Images
                if((imageFiles != null && imageFiles[0] != null) ||
                    existingImages == null ||
                    target.mImageIDs.Split(',').Count() != existingImages.Length)
                {
                    List<string> res = new List<string>();
                    
                    // Keep existing images
                    if (existingImages != null)
                    {
                        foreach (string oldImg in existingImages)
                        {
                            res.Add(oldImg);
                        }
                    }

                    // Delete images that were not selected to keep
                    if (existingImages == null || target.mImageIDs.Split(',').Length != existingImages.Length)
                    {
                        foreach(string imgID in target.mImageIDs.Split(','))
                        {
                            if(res.Contains(imgID)) { continue; }

                            ProductImage productImageToDelete = productImageContext.Find(imgID);

                            if(productImageToDelete != null)
                            {
                                productImageContext.Delete(productImageToDelete);
                            }
                        }
                    }

                    // Add new images that were uploaded
                    if(imageFiles != null && imageFiles[0] != null)
                    {
                        foreach(HttpPostedFileBase newImg in imageFiles)
                        {
                            ProductImage newProductImg = new ProductImage();
                            newProductImg.mImage = ImageStorageService.GetImageBytes(newImg);
                            newProductImg.mImageType = ImageStorageService.GetImageExtension(newImg);
                            productImageContext.Insert(newProductImg);

                            res.Add(newProductImg.mID);
                        }
                    }

                    // Save images to DB and save ref to current product
                    productImageContext.Commit();
                    target.mImageIDs = res.Count > 0 
                        ? String.Join(",", res)
                        : "";
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
                target.mSubCategories = p.Product.mSubCategories;
                target.mCustomLists = p.Product.mCustomLists;
                target.isDisplayed = p.Product.isDisplayed;

                productsContext.Commit();

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
                Product target = productsContext.Find(Id, true);

                // Gets all the Subcategories' IDs
                string[] allSubIDs = target.mSubCategories != "" 
                    ? target.mSubCategories.Split(',') 
                    : new string[] { "None" };

                // Gets each Subcategory's name using the ID and stores it in an array...if any
                string[] allSubCategoryNames = (allSubIDs[0] != "None") 
                    ? allSubIDs.Select(x => subCategoryContext.Find(x).mSubCategoryName).ToArray() 
                    : new string[] { "None" };

                // Gets each Custom List's name using the ID and stores it in an array...if any
                string[] allCustomListName = (!String.IsNullOrWhiteSpace(target.mCustomLists))
                    ? target.mCustomLists.Split(',').Select(x => customOptionListsContext.Find(x).mName).ToArray()
                    : new string[] { };
                
                // Find the Main Category's and Size Chart's names
                string mainCategoryName = categoryContext.Find(target.mCategory).mCategoryName;
                string sizeChartName = !String.IsNullOrWhiteSpace(target.mSizeChart) 
                    ? sizeChartContext.Find(target.mSizeChart).mChartName 
                    : "None";

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
                Product target = productsContext.Find(Id, true);

                basketService.RemoveItemFromAllBaskets(Id);

                // Delete images from product
                if(!String.IsNullOrWhiteSpace(target.mImageIDs))
                {
                    foreach(string imgId in target.mImageIDs.Split(','))
                    {
                        ProductImage productImage = productImageContext.Find(imgId);

                        if(productImage == null)
                        {
                            continue;
                        }

                        productImageContext.Delete(productImage);
                    }

                    productImageContext.Commit();
                }

                productsContext.Delete(target);
                productsContext.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "ProductManager", new { Id = Id });
            }
        }

        private ProductManagerViewModel GetProductManagerVM()
        {
            List<SizeChart> allSizeCharts = sizeChartContext.GetCollection().ToList();
            allSizeCharts.Insert(0, new SizeChart() { mID = "", mChartName = "None" });

            IEnumerable<Category> allCategories = categoryContext.GetCollection();

            if(allCategories.Count() <= 0)
            {
                throw new Exception("No categories to list");
            }

            // Popularize the view model with all the lists and product to edit
            ProductManagerViewModel viewModel = new ProductManagerViewModel
            {
                categories = allCategories,
                subCategories = subCategoryContext.GetCollection(),
                customOptionLists = customOptionListsContext.GetCollection(),
                sizeCharts = allSizeCharts
            };

            return viewModel;
        }
    
        private ProductImage[] GetProductImages(string imgIDs)
        {
            string[] productIds = !String.IsNullOrWhiteSpace(imgIDs)
                ? imgIDs.Split(',')
                : new string[] { };

            return productImageContext.GetCollection()
                .Where(productImg => productIds.Contains(productImg.mID)).ToArray();
        }
    }
}