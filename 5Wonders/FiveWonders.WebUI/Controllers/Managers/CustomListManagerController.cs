using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    public class CustomListManagerController : Controller
    {
        IRepository<CustomOptionList> customListContext;
        IRepository<Product> productsContext;

        public CustomListManagerController(IRepository<CustomOptionList> customListRepository, IRepository<Product> productsRepository)
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
        public ActionResult Create(CustomOptionList customList)
        {
            try
            {
                // Validate inputs
                CustomListValidator categoryValidator = new CustomListValidator(customListContext);
                ValidationResult validation = categoryValidator.Validate(customList);

                if (!ModelState.IsValid || !validation.IsValid)
                {
                    string errMsg = validation.Errors != null && validation.Errors.Count > 0
                        ? String.Join(",", validation.Errors)
                        : "A name or options list is missing.";

                    throw new Exception(errMsg);
                }

                customListContext.Insert(customList);
                customListContext.Commit();

                return RedirectToAction("Index", "CustomListManager");
            }
            catch(Exception e)
            {
                ViewBag.errMessages = e.Message.Split(',');
                return View(customList);
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                CustomOptionList colorSet = customListContext.Find(Id, true);
                return View(colorSet);
            }
            catch (Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string Id, CustomOptionList updatedList)
        {
            try
            {
                CustomOptionList customListToEdit = customListContext.Find(Id, true);

                customListToEdit.mName = updatedList.mName;
                customListToEdit.options = updatedList.options;

                // Validate inputs
                CustomListValidator categoryValidator = new CustomListValidator(customListContext);
                ValidationResult validation = categoryValidator.Validate(customListToEdit);

                if (!validation.IsValid)
                {
                    string[] errMsg = (validation.Errors != null && validation.Errors.Count > 0)
                        ? validation.Errors.Select(x => x.ErrorMessage).ToArray()
                        : new string[] { "A name or options list is missing." };

                    ViewBag.errMessages = errMsg;
                    return View(customListToEdit);
                }

                customListContext.Commit();

                return RedirectToAction("Index", "CustomListManager");
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "CustomListManager", new { Id = Id });
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
            catch (Exception e)
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

                return RedirectToAction("Index", "CustomListManager");
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "CustomListManager", new { Id = Id });
            }
        }
    }
}