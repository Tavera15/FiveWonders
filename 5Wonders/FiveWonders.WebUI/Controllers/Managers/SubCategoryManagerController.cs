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
        public ActionResult Create(SubCategory sub)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(sub);
                }

                subCategoryContext.Insert(sub);
                subCategoryContext.Commit();

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
        public ActionResult Edit(SubCategory sub, string Id)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(sub);
                }

                SubCategory subToEdit = subCategoryContext.Find(Id);
                subToEdit.mSubCategoryName = sub.mSubCategoryName;

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

                List<Product> productsWithSub = new List<Product>();

                foreach(var product in productContext.GetCollection())
                {
                    if(product.mSubCategories.Split(',').Contains(subToDelete.mID))
                    {
                        productsWithSub.Add(product);
                    }
                }

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

                List<Product> productsWithSub = new List<Product>();

                foreach (var product in productContext.GetCollection())
                {
                    if (product.mSubCategories.Split(',').Contains(subToDelete.mID))
                    {
                        productsWithSub.Add(product);
                    }
                }

                if (productsWithSub.ToArray().Length != 0)
                {
                    return RedirectToAction("Delete", "SubCategoryManager");
                }

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
    }
}