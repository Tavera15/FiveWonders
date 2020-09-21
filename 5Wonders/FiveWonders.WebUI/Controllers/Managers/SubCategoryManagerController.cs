using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class SubCategoryManagerController : Controller
    {
        IRepository<SubCategory> subCategoryContext;
        IRepository<Product> productContext;
        IImageStorageService imageStorageService;

        // GET: SubCategoryManager
        public SubCategoryManagerController(IRepository<SubCategory> subcategoryRepository, IRepository<Product> productRepository, IImageStorageService imageStorageService)
        {
            subCategoryContext = subcategoryRepository;
            productContext = productRepository;
            this.imageStorageService = imageStorageService;
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
                if(!ModelState.IsValid || (sub.isEventOrTheme && imageFile == null))
                {
                    throw new Exception("Subcategory Model no good");
                }

                if(sub.isEventOrTheme && imageFile != null)
                {
                    string newImgUrl;
                    imageStorageService.AddImage(EFolderName.Subcategory, Server, imageFile, sub.mID, out newImgUrl);

                    sub.mImageUrl = newImgUrl;
                }

                subCategoryContext.Insert(sub);
                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                _ = e;
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

                if (!ModelState.IsValid || 
                    (sub.isEventOrTheme &&
                        (imageFile == null && String.IsNullOrWhiteSpace(subToEdit.mImageUrl))))
                {
                    return View(sub);
                }

                if(!sub.isEventOrTheme && !String.IsNullOrWhiteSpace(subToEdit.mImageUrl))
                {
                    imageStorageService.DeleteImage(EFolderName.Subcategory, subToEdit.mImageUrl, Server);
                    subToEdit.mImageUrl = "";
                }
                
                subToEdit.mSubCategoryName = sub.mSubCategoryName;
                subToEdit.isEventOrTheme = sub.isEventOrTheme;
                subToEdit.mImgShaderAmount = sub.mImgShaderAmount;
                subToEdit.bannerTextColor = sub.bannerTextColor;

                if(imageFile != null)
                {
                    imageStorageService.DeleteImage(EFolderName.Subcategory, subToEdit.mImageUrl, Server);

                    string newImgUrl;
                    imageStorageService.AddImage(EFolderName.Subcategory, Server, imageFile, sub.mID, out newImgUrl);
                    subToEdit.mImageUrl = newImgUrl;
                }

                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "SubCategoryManager", new { Id = Id});
            }
        }

        // TODO shouldn't be able to delete promoted subcategory in home page
        public ActionResult Delete(string Id)
        {
            try
            {
                SubCategory subToDelete = subCategoryContext.Find(Id, true);

                Product[] productsWithSub = productContext.GetCollection()
                    .Where(p => !String.IsNullOrEmpty(p.mSubCategories) 
                    && p.mSubCategories.Contains(subToDelete.mID)).ToArray();

                ViewBag.productsWithSub = productsWithSub.ToArray();
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

                bool bItemsWithSub = productContext.GetCollection()
                    .Any(p => p.mSubCategories.Contains(Id));

                if (bItemsWithSub)
                {
                    throw new Exception("Products contain target subcategory.");
                }

                imageStorageService.DeleteImage(EFolderName.Subcategory, subToDelete.mImageUrl, Server);

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