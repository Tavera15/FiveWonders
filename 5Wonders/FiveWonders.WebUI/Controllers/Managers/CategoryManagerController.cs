using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;

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
                // Todo Shouln't create a new category with same name
                if (!ModelState.IsValid || imageFile == null)
                {
                    throw new Exception("Category Create model no good");
                }

                // Save img, and store its name to category obj.
                string newImgUrl;
                imageStorageService.AddImage(EFolderName.Category, Server, imageFile, cat.mID, out newImgUrl);
                cat.mImgUrL = newImgUrl;

                // Save to memory
                categoryContext.Insert(cat);
                categoryContext.Commit();

                return RedirectToAction("Index", "CategoryManager");
            }
            catch(Exception e)
            {
                _ = e;
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
        public ActionResult Edit(Category c, string Id, HttpPostedFileBase imageFile)
        {
            try
            {
                Category categoryToEdit = categoryContext.Find(Id, true);

                if (!ModelState.IsValid || (String.IsNullOrWhiteSpace(categoryToEdit.mImgUrL) && imageFile == null))
                {
                    return View(c);
                }

                if(imageFile != null)
                {
                    string newImgUrl;
                    imageStorageService.DeleteImage(EFolderName.Category, categoryToEdit.mImgUrL, Server);
                    imageStorageService.AddImage(EFolderName.Category, Server, imageFile, Id, out newImgUrl);

                    categoryToEdit.mImgUrL = newImgUrl;
                }

                categoryToEdit.mCategoryName = c.mCategoryName;
                categoryToEdit.mImgShaderAmount = c.mImgShaderAmount;
                categoryToEdit.bannerTextColor = c.bannerTextColor;
                
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