using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
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
            HomePageViewModel homeViewModel = new HomePageViewModel();

            HomePage homeData = homeContext.GetCollection().FirstOrDefault();
            homeData.mWelcomeBtnUrl = homeData.mWelcomeBtnUrl ?? "/products";

            string pic = homeData.mWelcomeImgUrl ?? "FWondersDefault.jpg";
            homeData.mWelcomeImgUrl = "../content/home/" + pic;

            List<Product> top3Products = productsContext.GetCollection().Take(3).ToList();
            List<InstagramPost> top4IGPosts = new List<InstagramPost>();

            try
            {
                List<InstagramPost> IGPostsFromAPI = await InstagramService.GetIGMediaAsync();
                top4IGPosts = IGPostsFromAPI.Take(4).ToList();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                top4IGPosts.Clear();

                for(int i = 0; i < 4; i++)
                {
                    top4IGPosts.Add(new InstagramPost()
                    {
                        mImageURL = "https://www.bargainballoons.com/products/Betallic-Balloons/Everyday-2015-Balloons/Large-Balloons/36029-18-inches-Sad-Smiley-balloons.jpg"
                    });
                }
            }

            homeViewModel.homePageData = homeData;
            homeViewModel.top3Products = top3Products;
            homeViewModel.top3IGPosts = top4IGPosts;

            return View(homeViewModel);
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