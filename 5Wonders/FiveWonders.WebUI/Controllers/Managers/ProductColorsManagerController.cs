using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    public class ProductColorsManagerController : Controller
    {
        IRepository<ColorSet> colorSetContext;

        public ProductColorsManagerController(IRepository<ColorSet> colorSetRepository)
        {
            colorSetContext = colorSetRepository;
        }

        // GET: ProductColorsManager
        public ActionResult Index()
        {
            ColorSet[] colorSets = colorSetContext.GetCollection().ToArray();
            return View(colorSets);
        }

        public ActionResult Create()
        {
            return View(new ColorSet());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ColorSet colorSet)
        {
            if(!ModelState.IsValid)
            {
                return View(colorSet);
            }

            colorSetContext.Insert(colorSet);
            colorSetContext.Commit();

            return RedirectToAction("Index", "ProductColorsManager");
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                ColorSet colorSet = colorSetContext.Find(Id, true);
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
        public ActionResult Edit(string Id, ColorSet updatedColor)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(updatedColor);
                }

                ColorSet colorSet = colorSetContext.Find(Id, true);

                colorSet.mName = updatedColor.mName;
                colorSet.colors = updatedColor.colors;

                colorSetContext.Commit();

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
                ColorSet colorSet = colorSetContext.Find(Id, true);
                return View(colorSet);
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
                ColorSet colorSet = colorSetContext.Find(Id, true);

                colorSetContext.Delete(colorSet);
                colorSetContext.Commit();

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