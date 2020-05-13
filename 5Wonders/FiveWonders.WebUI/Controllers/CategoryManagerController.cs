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
        CategoryRepository context;

        public CategoryManagerController()
        {
            context = new CategoryRepository();
        }

        // GET: CategoryManager
        public ActionResult Index()
        {
            List<Category> categories = context.GetCollection().ToList<Category>();

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
                context.Insert(cat);
                context.Commit();

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
                Category categoryToEdit = context.Find(Id);

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

                Category categoryToEdit = context.Find(Id);
                categoryToEdit.mCategoryName = c.mCategoryName;

                context.Commit();

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
                Category categoryToDelete = context.Find(Id);

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
                Category categoryToDelete = context.Find(Id);

                context.Delete(categoryToDelete);
                context.Commit();

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