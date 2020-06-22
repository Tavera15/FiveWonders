using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public IInstagramService InstagramService;

        public HomeController(IInstagramService IGService)
        {
            InstagramService = IGService;
        }

        public async Task<ActionResult> Index()
        {
            List<InstagramPost> instagramPosts = await InstagramService.GetIGMediaAsync();
            InstagramPost[] topThreeIGPosts = instagramPosts.Take(3).ToArray();


            ViewBag.instagramPosts = topThreeIGPosts;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}