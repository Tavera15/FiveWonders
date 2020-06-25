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
        public async Task<ActionResult> Index()
        {
            try
            {
                throw new Exception("No");
                List<InstagramPost> InstagramPosts = await InstagramService.GetIGMediaAsync();

                return View(InstagramPosts);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                List<InstagramPost> errorPost = new List<InstagramPost>();
                errorPost.Add(new InstagramPost()
                {
                    mImageURL = "https://www.bargainballoons.com/products/Betallic-Balloons/Everyday-2015-Balloons/Large-Balloons/36029-18-inches-Sad-Smiley-balloons.jpg"
                });

                return View(errorPost);
            }
        }
    }
}