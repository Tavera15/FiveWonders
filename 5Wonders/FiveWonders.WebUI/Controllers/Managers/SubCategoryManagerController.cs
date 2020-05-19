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
        IRepository<SubCategory> context;

        // GET: SubCategoryManager
        public SubCategoryManagerController(IRepository<SubCategory> repository)
        {
            context = repository;
        }

        public ActionResult Index()
        {
            List<SubCategory> allSubCategories = context.GetCollection().ToList<SubCategory>();

            return View(allSubCategories);
        }

        public ActionResult Create()
        {
            return View(new SubCategory());
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(SubCategory sub)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(sub);
                }

                context.Insert(sub);
                context.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                SubCategory subToEdit = context.Find(Id);

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
        public ActionResult Edit(SubCategory sub, string Id)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(sub);
                }

                SubCategory subToEdit = context.Find(Id);
                subToEdit.mSubCategoryName = sub.mSubCategoryName;

                context.Commit();

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
                SubCategory subToDelete = context.Find(Id);

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
                SubCategory subToDelete = context.Find(Id);
                context.Delete(subToDelete);
                context.Commit();

                return RedirectToAction("Index", "SubCategoryManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }
    }
}