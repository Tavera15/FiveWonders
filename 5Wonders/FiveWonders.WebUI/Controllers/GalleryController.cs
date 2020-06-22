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
            List<InstagramPost> InstagramPosts = await InstagramService.GetIGMediaAsync();

            return View(InstagramPosts);
        }
    }
}