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
    public class GalleryManagerController : Controller
    {
        public IInstagramService InstagramService;
        public IRepository<GalleryImg> galleryContext;

        public GalleryManagerController(IInstagramService IgService, IRepository<GalleryImg> galleryRepository)
        {
            InstagramService = IgService;
            galleryContext = galleryRepository;
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
                    string fileNameWithoutSpaces = String.Concat(file.FileName.Where(c => !Char.IsWhiteSpace(c)));

                    file.SaveAs(Server.MapPath("//Content//GalleryImages//") + fileNameWithoutSpaces);

                    GalleryImg newImg = new GalleryImg
                    {
                        mImageFile = fileNameWithoutSpaces
                    };

                    galleryContext.Insert(newImg);
                }

                galleryContext.Commit();

                return RedirectToAction("Index", "GalleryManager");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "GalleryManager");
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                GalleryImg target = galleryContext.Find(Id);

                return View(target);
            }
            catch(Exception e)
            {
                return RedirectToAction("Index", "GalleryManager");
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                GalleryImg target = galleryContext.Find(Id);

                string path = Server.MapPath("//Content//GalleryImages//") + target.mImageFile;

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                galleryContext.Delete(target);
                galleryContext.Commit();

                return RedirectToAction("Index", "GalleryManager");
            }
            catch (Exception e)
            {
                return RedirectToAction("Delete", "GalleryManager", new { Id = Id});
            }
        }
    }
}