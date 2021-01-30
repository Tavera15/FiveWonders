using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using FiveWonders.Services;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class SubCategoryManagerController : Controller
    {
        IRepository<SubCategory> subCategoryContext;
        IRepository<Product> productContext;
        IRepository<HomePage> homePageContext;

        // GET: SubCategoryManager
        public SubCategoryManagerController(IRepository<SubCategory> subcategoryRepository, IRepository<Product> productRepository, IRepository<HomePage> homePageRepository)
        {
            subCategoryContext = subcategoryRepository;
            productContext = productRepository;
            homePageContext = homePageRepository;
        }

        public ActionResult Index()
        {
            List<SubCategory> allSubCategories = subCategoryContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToList();

            return View(allSubCategories);
        }

        public ActionResult Create()
        {
            return View(new SubCategory());
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(SubCategory sub, HttpPostedFileBase imageFile)
        {
            try
            {
                // Validate inputs
                SubcategoryValidator subcategoryValidator = new SubcategoryValidator(subCategoryContext, imageFile);
                ValidationResult validation = subcategoryValidator.Validate(sub);

                if (!validation.IsValid)
                {
                    string errMsg = validation.Errors != null && validation.Errors.Count > 0
                        ? String.Join(",", validation.Errors)
                        : "A subcategory name or image is missing.";

                    throw new Exception(errMsg);
                }

                sub.mImage = sub.isEventOrTheme && imageFile != null
                    ? ImageStorageService.GetImageBytes(imageFile)
                    : null;

                sub.mImageType = sub.isEventOrTheme && imageFile != null
                    ? ImageStorageService.GetImageExtension(imageFile)
                    : null;

                subCategoryContext.Insert(sub);
                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                ViewBag.errMessages = e.Message.Split(',');
                return View(sub);
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                SubCategory subToEdit = subCategoryContext.Find(Id, true);

                return View(subToEdit);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(SubCategory sub, string Id, HttpPostedFileBase imageFile)
        {
            try
            {
                SubCategory subToEdit = subCategoryContext.Find(Id, true);

                // Temporarily Update the subcategory's properties. Will not save if inputs are not valid.
                subToEdit.mSubCategoryName = sub.mSubCategoryName;
                subToEdit.isEventOrTheme = sub.isEventOrTheme;
                subToEdit.mImgShaderAmount = sub.mImgShaderAmount;
                subToEdit.bannerTextColor = sub.bannerTextColor;
                
                // Validate inputs
                SubcategoryValidator subcategoryValidator = new SubcategoryValidator(subCategoryContext, imageFile);
                ValidationResult validation = subcategoryValidator.Validate(subToEdit);

                if (!validation.IsValid)
                {
                    string[] errMsg = (validation.Errors != null && validation.Errors.Count > 0)
                        ? validation.Errors.Select(x => x.ErrorMessage).ToArray()
                        : new string[] { "A subcategory name or image is missing." };

                    ViewBag.errMessages = errMsg;
                    return View(subToEdit);
                }

                // Save img if sub is now an event or theme, and if a new one is being imported
                if (subToEdit.isEventOrTheme && imageFile != null)
                {
                    subToEdit.mImage = ImageStorageService.GetImageBytes(imageFile);
                    subToEdit.mImageType = ImageStorageService.GetImageExtension(imageFile);
                }
                else if(!subToEdit.isEventOrTheme)
                {
                    subToEdit.mImage = null;
                    subToEdit.mImageType = null;
                }

                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "SubCategoryManager", new { Id = Id });
            }
        }

        // TODO shouldn't be able to delete promoted subcategory in home page
        public ActionResult Delete(string Id)
        {
            try
            {
                SubCategory subToDelete = subCategoryContext.Find(Id, true);
                HomePage homePageData = homePageContext.GetCollection().FirstOrDefault();

                Product[] productsWithSub = productContext.GetCollection()
                    .Where(p => !String.IsNullOrEmpty(p.mSubCategories) 
                    && p.mSubCategories.Contains(subToDelete.mID)).ToArray();

                bool isAPromoOnHomePage = homePageData != null && (homePageData.mPromo1 == Id || homePageData.mPromo2 == Id);
                bool isHomePageRedirectBtn = homePageData != null && homePageData.mWelcomeBtnUrl == Id;

                ViewBag.productsWithSub = productsWithSub.ToArray();
                ViewBag.isAPromoOnHomePage = isAPromoOnHomePage;
                ViewBag.isHomePageRedirectBtn = isHomePageRedirectBtn;
                return View(subToDelete);
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
                SubCategory subToDelete = subCategoryContext.Find(Id, true);
                HomePage homePageData = homePageContext.GetCollection().FirstOrDefault();

                bool bItemsWithSub = productContext.GetCollection()
                    .Any(p => p.mSubCategories.Contains(Id));

                bool isAPromoOnHomePage = homePageData != null && (homePageData.mPromo1 == Id || homePageData.mPromo2 == Id);
                bool isHomePageRedirectBtn = homePageData != null && homePageData.mWelcomeBtnUrl == Id;

                if (bItemsWithSub || isAPromoOnHomePage || isHomePageRedirectBtn)
                {
                    throw new Exception("Products contain target subcategory, and/or category is currently promoted on the Home Page.");
                }

                subCategoryContext.Delete(subToDelete);
                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "SubCategoryManager", new { Id = Id});
            }
        }
    }
}