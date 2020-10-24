using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using FluentValidation.Results;

namespace FiveWonders.WebUI.Controllers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class CategoryManagerController : Controller
    {
        IRepository<Category> categoryContext;
        IRepository<Product> productsContext;
        IImageStorageService imageStorageService;

        public CategoryManagerController(IRepository<Category> categoryRepository, IRepository<Product> productsRepository, IImageStorageService imageStorageService)
        {
            categoryContext = categoryRepository;
            productsContext = productsRepository;
            this.imageStorageService = imageStorageService;
        }

        // GET: CategoryManager
        public ActionResult Index()
        {
            List<Category> categories = categoryContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToList();

            return View(categories);
        }

        
        public ActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category cat, HttpPostedFileBase imageFile)
        {
            try
            {
                // Validate inputs
                CategoryValidator categoryValidator = new CategoryValidator(categoryContext, imageFile);
                ValidationResult validation = categoryValidator.Validate(cat);

                if(!validation.IsValid)
                {
                    string errMsg = validation.Errors != null && validation.Errors.Count > 0
                        ? String.Join(",", validation.Errors)
                        : "A category name or image is missing.";

                    throw new Exception(errMsg);
                }

                // Save Img. This should always happen.
                if (imageFile != null)
                {
                    // Save img, and store its name to category obj.
                    string newImgUrl;
                    imageStorageService.AddImage(EFolderName.Category, Server, imageFile, cat.mID, out newImgUrl);
                    cat.mImgUrL = newImgUrl;
                }

                // Save to memory
                categoryContext.Insert(cat);
                categoryContext.Commit();

                return RedirectToAction("Index", "CategoryManager");
            }
            catch(Exception e)
            {
                ViewBag.errMessages = e.Message.Split(',');
                return View(cat);
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                Category categoryToEdit = categoryContext.Find(Id, true);

                return View(categoryToEdit);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category updatedCat, string Id, HttpPostedFileBase imageFile)
        {
            try
            {
                // Find category to edit
                Category categoryToEdit = categoryContext.Find(Id, true);

                // Temporarily Update the category's properties. Will not save if inputs are not valid.
                categoryToEdit.mCategoryName = updatedCat.mCategoryName;
                categoryToEdit.mImgShaderAmount = updatedCat.mImgShaderAmount;
                categoryToEdit.bannerTextColor = updatedCat.bannerTextColor;

                // Validate inputs
                CategoryValidator categoryValidator = new CategoryValidator(categoryContext, imageFile);
                ValidationResult validation = categoryValidator.Validate(categoryToEdit);

                if (!validation.IsValid)
                {
                    string[] errMsg = (validation.Errors != null && validation.Errors.Count > 0)
                        ? validation.Errors.Select(x => x.ErrorMessage).ToArray()
                        : new string[] { "A category name or image is missing." };

                    ViewBag.errMessages = errMsg;
                    return View(categoryToEdit);
                }

                // Save any new image that was selected
                if (imageFile != null)
                {
                    string newImgUrl;
                    imageStorageService.DeleteImage(EFolderName.Category, categoryToEdit.mImgUrL, Server);
                    imageStorageService.AddImage(EFolderName.Category, Server, imageFile, Id, out newImgUrl);

                    // Save Url
                    categoryToEdit.mImgUrL = newImgUrl;
                }
                
                // Save
                categoryContext.Commit();

                return RedirectToAction("Index", "CategoryManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "CategoryManager", new { Id = Id });
            }
        }

        // TODO Shouldn't be able to delete promoted category in home page
        public ActionResult Delete(string Id)
        {
            try
            {
                Category categoryToDelete = categoryContext.Find(Id, true);

                Product[] productsWithCategory = productsContext.GetCollection()
                    .Where(x => x.mCategory == categoryToDelete.mID).ToArray();

                ViewBag.productsWithCategory = productsWithCategory;
                return View(categoryToDelete);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [ActionName("Delete")]
        [HttpPost]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                Category categoryToDelete = categoryContext.Find(Id, true);

                bool bItemsWithCat = productsContext.GetCollection().Any(p => p.mCategory == Id);

                if(bItemsWithCat)
                {
                    throw new Exception("Products contain targeted category");
                }

                imageStorageService.DeleteImage(EFolderName.Category, categoryToDelete.mImgUrL, Server);

                categoryContext.Delete(categoryToDelete);
                categoryContext.Commit();

                return RedirectToAction("Index", "CategoryManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "CategoryManager", new { Id = Id });
            }
        }
    }
}