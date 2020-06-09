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
        public ActionResult Create(Category cat)
        {
            try
            {
                if(ModelState == null)
                {
                    return View(cat);
                }

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
        public ActionResult Edit(Category c, string Id)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(c);
                }

                Category categoryToEdit = categoryContext.Find(Id);
                categoryToEdit.mCategoryName = c.mCategoryName;

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
                Product[] productsWithCategory = productsContext.GetCollection().Where(x => x.mCategory == categoryToDelete.mID).ToArray();

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
                Product[] productsWithCategory = productsContext.GetCollection().Where(x => x.mCategory == categoryToDelete.mID).ToArray();

                if(productsWithCategory.Length != 0)
                {
                    return RedirectToAction("Delete", "CategoryManager");
                }

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
    }
}