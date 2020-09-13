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

        // GET: SubCategoryManager
        public SubCategoryManagerController(IRepository<SubCategory> subcategoryRepository, IRepository<Product> productRepository)
        {
            subCategoryContext = subcategoryRepository;
            productContext = productRepository;
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
                    return View(sub);
                }

                if(sub.isEventOrTheme && imageFile != null)
                {
                    string newImgUrl;
                    AddImages(sub.mID, imageFile, out newImgUrl);

                    sub.mImageUrl = newImgUrl;
                }

                subCategoryContext.Insert(sub);
                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return View(sub);
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                SubCategory subToEdit = subCategoryContext.Find(Id);

                return View(subToEdit);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(SubCategory sub, string Id, HttpPostedFileBase imageFile)
        {
            try
            {
                SubCategory subToEdit = subCategoryContext.Find(Id);

                if(!ModelState.IsValid || 
                    (sub.isEventOrTheme &&
                        (imageFile == null && String.IsNullOrWhiteSpace(subToEdit.mImageUrl))))
                {
                    return View(sub);
                }

                if(!sub.isEventOrTheme && !String.IsNullOrWhiteSpace(subToEdit.mImageUrl))
                {
                    DeleteImages(subToEdit.mImageUrl);
                    subToEdit.mImageUrl = "";
                }
                
                subToEdit.mSubCategoryName = sub.mSubCategoryName;
                subToEdit.isEventOrTheme = sub.isEventOrTheme;
                subToEdit.mImgShaderAmount = sub.mImgShaderAmount;
                subToEdit.bannerTextColor = sub.bannerTextColor;

                if(imageFile != null)
                {
                    DeleteImages(subToEdit.mImageUrl);

                    string newImgUrl;
                    AddImages(Id, imageFile, out newImgUrl);

                    subToEdit.mImageUrl = newImgUrl;
                }

                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                SubCategory subToDelete = subCategoryContext.Find(Id);

                Product[] productsWithSub = productContext.GetCollection()
                    .Where(p => !String.IsNullOrEmpty(p.mSubCategories) 
                    && p.mSubCategories.Contains(subToDelete.mID)).ToArray();

                ViewBag.productsWithSub = productsWithSub.ToArray();
                return View(subToDelete);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [ActionName("Delete")]
        [HttpPost]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                SubCategory subToDelete = subCategoryContext.Find(Id);

                bool bItemsWithSub = productContext.GetCollection()
                    .Any(p => p.mSubCategories.Contains(Id));

                if (bItemsWithSub)
                {
                    return RedirectToAction("Delete", "SubCategoryManager");
                }

                DeleteImages(subToDelete.mImageUrl);

                subCategoryContext.Delete(subToDelete);
                subCategoryContext.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        private void AddImages(string Id, HttpPostedFileBase imageFile, out string newImageURL)
        {
            string fileNameWithoutSpaces = String.Concat(imageFile.FileName.Where(c => !Char.IsWhiteSpace(c)));

            newImageURL = Id + fileNameWithoutSpaces;
            imageFile.SaveAs(Server.MapPath("//Content//SubcategoryImages//") + newImageURL);
        }

        private void DeleteImages(string currentImageURL)
        {
            string path = Server.MapPath("//Content//SubcategoryImages//") + currentImageURL;

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}