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
    public class GalleryManagerController : Controller
    {
        public IInstagramService InstagramService;
        public IRepository<GalleryImg> galleryContext;
        public IImageStorageService imageStorageService;

        public GalleryManagerController(IInstagramService IgService, IRepository<GalleryImg> galleryRepository, IImageStorageService imageStorageService)
        {
            InstagramService = IgService;
            galleryContext = galleryRepository;
            this.imageStorageService = imageStorageService;
        }

        // GET: GalleryManager - All images 
        public ActionResult Index()
        {
            GalleryImg[] galleryImgs = InstagramService.GetGalleryImgs();

            return View(galleryImgs);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase[] imageFiles)
        {
            try
            {
                // If no new images were added, return to Gallery home page
                if (imageFiles == null || imageFiles[0] == null)
                {
                    return RedirectToAction("Index", "GalleryManager");
                }

                // Else add new images and store them to Db.
                foreach (HttpPostedFileBase file in imageFiles)
                {
                    GalleryImg newImg = new GalleryImg();
                    string newImgUrl;
                    imageStorageService.AddImage(EFolderName.Gallery, Server, file, newImg.mID, out newImgUrl);

                    newImg.mImageFile = newImgUrl;

                    galleryContext.Insert(newImg);
                }

                galleryContext.Commit();

                return RedirectToAction("Index", "GalleryManager");
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Index", "GalleryManager");
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                GalleryImg target = galleryContext.Find(Id, true);

                return View(target);
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
                GalleryImg target = galleryContext.Find(Id, true);

                imageStorageService.DeleteImage(EFolderName.Gallery, target.mImageFile, Server);

                galleryContext.Delete(target);
                galleryContext.Commit();

                return RedirectToAction("Index", "GalleryManager");
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "GalleryManager", new { Id = Id});
            }
        }
    }
}