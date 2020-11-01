using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class CustomListManagerController : Controller
    {
        IRepository<CustomOptionList> customListContext;
        IRepository<Product> productsContext;
        IRepository<BasketItem> basketItemContext;
        IBasketServices basketService;

        public CustomListManagerController(IRepository<CustomOptionList> customListRepository, IRepository<Product> productsRepository, IRepository<BasketItem> basketItemRepository, IBasketServices basketService)
        {
            customListContext = customListRepository;
            productsContext = productsRepository;
            basketItemContext = basketItemRepository;
            this.basketService = basketService;
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
                CustomOptionList customList = customListContext.Find(Id, true);
                return View(customList);
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
                // Make sure Custom List exist
                CustomOptionList customListToEdit = customListContext.Find(Id, true);

                // Save a copy of the old List options to determine if baskets need to be updated
                string oldListOpts = customListToEdit.options;

                // Temporarily update the actual Custom List object
                customListToEdit.mName = updatedList.mName;
                customListToEdit.options = updatedList.options;

                // Validate inputs
                CustomListValidator categoryValidator = new CustomListValidator(customListContext);
                ValidationResult validation = categoryValidator.Validate(customListToEdit);

                if (!validation.IsValid)
                {
                    customListToEdit.options = oldListOpts;

                    string[] errMsg = (validation.Errors != null && validation.Errors.Count > 0)
                        ? validation.Errors.Select(x => x.ErrorMessage).ToArray()
                        : new string[] { "A name or options list is missing." };

                    ViewBag.errMessages = errMsg;
                    return View(customListToEdit);
                }

                // Save
                customListContext.Commit();

                // If all the old options still exist, don't need to update basket items
                bool bUpdateBaskets = !oldListOpts.Split(',')
                    .All(oldOpt => updatedList.options.Split(',').Contains(oldOpt));

                if(bUpdateBaskets)
                {
                    // Find basket items that contain the Custom List Id
                    BasketItem[] basketItemsWithCustomListId = basketItemContext.GetCollection()
                        .Where(basketItem => basketItem.mCustomListOptions.Contains(Id)).ToArray();

                    if(basketItemsWithCustomListId != null && basketItemsWithCustomListId.Length > 0)
                    {
                        List<string> basketItemsToDelete = new List<string>();

                        // Go through all basket items... 
                        foreach(BasketItem item in basketItemsWithCustomListId)
                        {
                            // Deserialize its Custom List Opts...
                            Dictionary<string, string> deserializedListOpts =
                                JsonConvert.DeserializeObject<Dictionary<string, string>>(item.mCustomListOptions);

                            // and store the basket items that contain values that don't exist anymore
                            if (!updatedList.options.Split(',').Any(opt => opt == deserializedListOpts[Id]))
                            {
                                basketItemsToDelete.Add(item.mID);
                            }
                        }

                        // Delete the basket items selected
                        basketService.RemoveMultipleBasketItems(basketItemsToDelete.ToArray());
                    }
                }

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