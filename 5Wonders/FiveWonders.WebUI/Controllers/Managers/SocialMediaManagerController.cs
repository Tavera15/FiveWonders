using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using FiveWonders.Services;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class SocialMediaManagerController : Controller
    {
        public IRepository<SocialMedia> socialMediaContext;
        public IImageStorageService imageStorageService;

        public SocialMediaManagerController(IRepository<SocialMedia> socialMediaRepository, IImageStorageService imageStorageService)
        {
            socialMediaContext = socialMediaRepository;
            this.imageStorageService = imageStorageService;
        }

        // GET: SocialMediaManager
        public ActionResult Index()
        {
            SocialMedia[] allMedias = socialMediaContext.GetCollection().ToArray();

            return View(allMedias);
        }

        public ActionResult Create()
        {
            return View(new SocialMedia());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SocialMedia newMedia, HttpPostedFileBase newIcon)
        {
            try
            {
                newMedia.mUrl = newMedia.mUrl.Trim();

                SocialMediaValidator socialMediaValidator = new SocialMediaValidator(socialMediaContext, newIcon);
                ValidationResult validation = socialMediaValidator.Validate(newMedia);

                if(!validation.IsValid)
                {
                    string errMsg = validation.Errors != null && validation.Errors.Count > 0
                        ? String.Join(",", validation.Errors)
                        : "A url or a 64x64 pixels icon is missing.";

                    throw new Exception(errMsg);
                }

                newMedia.mIcon = ImageStorageService.GetImageBytes(newIcon);
                newMedia.mIconType = ImageStorageService.GetImageExtension(newIcon);

                socialMediaContext.Insert(newMedia);
                socialMediaContext.Commit();

                return RedirectToAction("Index", "SocialMediaManager");
            }
            catch(Exception e)
            {
                ViewBag.errMessages = e.Message.Split(',');
                return View(newMedia);
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                SocialMedia sm = socialMediaContext.Find(Id, true);

                return View(sm);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string Id, SocialMedia updatedMedia, HttpPostedFileBase newIcon)
        {
            try
            {
                SocialMedia mediaToEdit = socialMediaContext.Find(Id, true);

                mediaToEdit.mUrl = updatedMedia.mUrl.Trim();
                mediaToEdit.isCommunicative = updatedMedia.isCommunicative;

                SocialMediaValidator socialMediaValidator = new SocialMediaValidator(socialMediaContext, newIcon);
                ValidationResult validation = socialMediaValidator.Validate(mediaToEdit);

                if (!validation.IsValid)
                {
                    string[] errMsg = validation.Errors != null && validation.Errors.Count > 0
                        ? validation.Errors.Select(err => err.ErrorMessage).ToArray()
                        : new string[] { "A url or a 64x64 pixels icon is missing." };

                    ViewBag.errMessages = errMsg;
                    return View(mediaToEdit);
                }

                if(newIcon != null)
                {
                    mediaToEdit.mIcon = ImageStorageService.GetImageBytes(newIcon);
                    mediaToEdit.mIconType = ImageStorageService.GetImageExtension(newIcon);
                }

                socialMediaContext.Commit();

                return RedirectToAction("Index", "SocialMediaManager");
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "SocialMediaManager", new { Id = Id });
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                SocialMedia sm = socialMediaContext.Find(Id, true);

                return View(sm);
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
                SocialMedia sm = socialMediaContext.Find(Id, true);

                socialMediaContext.Delete(sm);
                socialMediaContext.Commit();

                return RedirectToAction("Index", "SocialMediaManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "SocialMediaManager", new { Id = Id});
            }
        }
    }
}