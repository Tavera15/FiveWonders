using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    public class ProductColorsManagerController : Controller
    {
        IRepository<CustomOptionList> customListContext;
        IRepository<Product> productsContext;

        // TODO Investigate if it is possible to create multiple custom sets. (Color sets, team sets, etc)

        public ProductColorsManagerController(IRepository<CustomOptionList> customListRepository, IRepository<Product> productsRepository)
        {
            customListContext = customListRepository;
            productsContext = productsRepository;
        }

        // GET: ProductColorsManager
        public ActionResult Index()
        {
            CustomOptionList[] colorSets = customListContext.GetCollection().ToArray();
            return View(colorSets);
        }

        public ActionResult Create()
        {
            return View(new CustomOptionList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomOptionList colorSet)
        {
            if(!ModelState.IsValid)
            {
                return View(colorSet);
            }

            customListContext.Insert(colorSet);
            customListContext.Commit();

            return RedirectToAction("Index", "ProductColorsManager");
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                CustomOptionList colorSet = customListContext.Find(Id, true);
                return View(colorSet);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string Id, CustomOptionList updatedColor)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(updatedColor);
                }

                CustomOptionList colorSet = customListContext.Find(Id, true);

                colorSet.mName = updatedColor.mName;
                colorSet.options = updatedColor.options;

                customListContext.Commit();

                return RedirectToAction("Index", "ProductColorsManager");
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "ProductColorsManager", new { Id = Id });
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                CustomOptionList customList = customListContext.Find(Id, true);

                Product[] productsWithCustList = productsContext.GetCollection()
                    .Where(p => !String.IsNullOrEmpty(p.mCustomLists)
                    && p.mCustomLists.Contains(Id)).ToArray();

                ViewBag.productsWithCustList = productsWithCustList;
                return View(customList);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                CustomOptionList customList = customListContext.Find(Id, true);

                bool bItemsContainList = productsContext.GetCollection()
                    .Any(p => p.mCustomLists.Contains(Id));

                if (bItemsContainList) { throw new Exception("Products contain custom list"); }

                customListContext.Delete(customList);
                customListContext.Commit();

                return RedirectToAction("Index", "ProductColorsManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "ProductColorsManager", new { Id = Id });
            }
        }

    }
}