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
        public IInstagramService InstagramService; 

        public GalleryController(IInstagramService service)
        {
            InstagramService = service;
        }

        // GET: Gallery
        public ActionResult Index()
        {
            ViewBag.Title = "Gallery";

            try
            {
                GalleryImg[] galleryImgs = InstagramService.GetGalleryImgs();

                return View(galleryImgs);
            }
            catch(Exception e)
            {
                GalleryImg[] galleryImgs = new GalleryImg[] 
                    { new GalleryImg { mImageFile = "https://www.bargainballoons.com/products/Betallic-Balloons/Everyday-2015-Balloons/Large-Balloons/36029-18-inches-Sad-Smiley-balloons.jpg" } };

                return View(galleryImgs);
            }
        }
    }
}