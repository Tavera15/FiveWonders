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
        public IRepository<HomePage> homeContext;
        public IRepository<Product> productsContext;

        public HomeController(IInstagramService IGService, IRepository<HomePage> homeRepository, IRepository<Product> productsRepository)
        {
            InstagramService = IGService;
            homeContext = homeRepository;
            productsContext = productsRepository;
        }

        public async Task<ActionResult> Index()
        {
            HomePage homeData = homeContext.GetCollection().FirstOrDefault();
            homeData.mWelcomeBtnUrl = homeData.mWelcomeBtnUrl ?? "/products";

            string pic = homeData.mWelcomeImgUrl ?? "FWondersDefault.jpg";
            homeData.mWelcomeImgUrl = "../content/home/" + pic;

            List<InstagramPost> instagramPosts = await InstagramService.GetIGMediaAsync();
            InstagramPost[] topThreeIGPosts = instagramPosts.Take(3).ToArray();

            Product[] top3Products = productsContext.GetCollection().Take(3).ToArray();

            ViewBag.top3Products = top3Products;
            ViewBag.instagramPosts = topThreeIGPosts;
            return View(homeData);
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