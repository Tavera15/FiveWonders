using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
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
                if(!ModelState.IsValid || newIcon == null)
                {
                    throw new Exception("Social media Create model no good");
                }

                string newIconUrl;
                imageStorageService.AddImage(EFolderName.Icons, Server, newIcon, newMedia.mID, out newIconUrl);
                newMedia.m64x64Icon = newIconUrl;

                socialMediaContext.Insert(newMedia);
                socialMediaContext.Commit();

                return RedirectToAction("Index", "SocialMediaManager");
            }
            catch(Exception e)
            {
                _ = e;
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
                SocialMedia sm = socialMediaContext.Find(Id, true);

                if(!ModelState.IsValid || (String.IsNullOrWhiteSpace(sm.m64x64Icon) && newIcon == null))
                {
                    return View(updatedMedia);
                }

                if(newIcon != null)
                {
                    imageStorageService.DeleteImage(EFolderName.Icons, sm.m64x64Icon, Server);

                    string newIconUrl;
                    imageStorageService.AddImage(EFolderName.Icons, Server, newIcon, Id, out newIconUrl);
                    sm.m64x64Icon = newIconUrl;
                }

                sm.mUrl = updatedMedia.mUrl;

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

                imageStorageService.DeleteImage(EFolderName.Icons, sm.m64x64Icon, Server);
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