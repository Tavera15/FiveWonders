using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;

namespace FiveWonders.WebUI.Controllers
{
    public class CategoryManagerController : Controller
    {
        IRepository<Category> categoryContext;
        IRepository<Product> productsContext;

        public CategoryManagerController(IRepository<Category> categoryRepository, IRepository<Product> productsRepository)
        {
            categoryContext = categoryRepository;
            productsContext = productsRepository;
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
                if (!ModelState.IsValid || imageFile == null)
                {
                    return View(cat);
                }

                // Save img, and store its name to category obj.
                string newImgUrl;
                AddImages(cat.mID, imageFile, out newImgUrl);

                cat.mImgUrL = newImgUrl;

                // Save to memory
                categoryContext.Insert(cat);
                categoryContext.Commit();

                return RedirectToAction("Index", "CategoryManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return View(cat);
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                Category categoryToEdit = categoryContext.Find(Id);

                return View(categoryToEdit);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category c, string Id, HttpPostedFileBase imageFile)
        {
            try
            {
                Category categoryToEdit = categoryContext.Find(Id);

                if(!ModelState.IsValid || (String.IsNullOrWhiteSpace(categoryToEdit.mImgUrL) && imageFile == null))
                {
                    return View(c);
                }

                if(imageFile != null)
                {
                    string newImgUrl;
                    DeleteImages(categoryToEdit.mImgUrL);
                    AddImages(Id, imageFile, out newImgUrl);

                   categoryToEdit.mImgUrL = newImgUrl;
                }

                categoryToEdit.mCategoryName = c.mCategoryName;
                categoryToEdit.mImgShaderAmount = c.mImgShaderAmount;
                
                categoryContext.Commit();

                return RedirectToAction("Index", "CategoryManager");
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
                Category categoryToDelete = categoryContext.Find(Id);
                
                Product[] productsWithCategory = productsContext.GetCollection()
                    .Where(x => x.mCategory == categoryToDelete.mID).ToArray();

                ViewBag.productsWithCategory = productsWithCategory;
                return View(categoryToDelete);
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
                Category categoryToDelete = categoryContext.Find(Id);

                bool bItemsWithCat = productsContext.GetCollection().Any(p => p.mCategory == Id);

                if(bItemsWithCat)
                {
                    return RedirectToAction("Delete", "CategoryManager");
                }

                DeleteImages(categoryToDelete.mImgUrL);

                categoryContext.Delete(categoryToDelete);
                categoryContext.Commit();

                return RedirectToAction("Index", "CategoryManager");
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
            imageFile.SaveAs(Server.MapPath("//Content//CategoryImages//") + newImageURL);
        }

        private void DeleteImages(string currentImageURL)
        {
            string path = Server.MapPath("//Content//CategoryImages//") + currentImageURL;

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}