using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class GalleryController : Controller
    {
        const int IMGS_PER_PAGE = 4;

        public IInstagramService InstagramService; 

        public GalleryController(IInstagramService service)
        {
            InstagramService = service;
        }

        [Route(Name = "/{page?}")]

        // GET: Gallery
        public ActionResult Index(int? page)
        {
            ViewBag.Title = "Gallery";

            try
            {
                GalleryImg[] allGalleryImgs = InstagramService.GetGalleryImgs();

                int pageNumber = page ?? 1;
                pageNumber = pageNumber <= 1 ? 1 : pageNumber;

                GalleryImg[] galleryImgs = (pageNumber) <= 1
                    ? allGalleryImgs.Take(IMGS_PER_PAGE).ToArray()
                    : (allGalleryImgs.Skip(IMGS_PER_PAGE * (pageNumber - 1)).Take(IMGS_PER_PAGE)).ToArray();

                double rawPageNumbers = ((double)allGalleryImgs.Length / (double)IMGS_PER_PAGE);
                ViewBag.PageNumbers = (int)(Math.Ceiling(rawPageNumbers));
                ViewBag.CurrentPage = pageNumber;
                
                return View(galleryImgs);
            }
            catch(Exception e)
            {
                _ = e;
                GalleryImg[] galleryImgs = new GalleryImg[] 
                    { new GalleryImg { mImageFile = "https://www.bargainballoons.com/products/Betallic-Balloons/Everyday-2015-Balloons/Large-Balloons/36029-18-inches-Sad-Smiley-balloons.jpg" } };
                
                ViewBag.PageNumbers = 1;
                return View(galleryImgs);
            }
        }
    }
}