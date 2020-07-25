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

        public ActionResult Index()
        {
            HomePageViewModel homeViewModel = new HomePageViewModel();

            HomePage homeData = homeContext.GetCollection().FirstOrDefault();
            homeData.mWelcomeBtnUrl = homeData.mWelcomeBtnUrl ?? "/products";

            string pic = homeData.mWelcomeImgUrl ?? "FWondersDefault.jpg";
            homeData.mWelcomeImgUrl = "../content/home/" + pic;

            List<Product> top3Products = productsContext.GetCollection().Take(3).ToList();
            List<InstagramPost> top4IGPosts = new List<InstagramPost>();
            for(int i = 0; i < 4; i++)
            {
                top4IGPosts.Add(new InstagramPost()
                {
                    mImageURL = "https://scontent-dfw5-1.xx.fbcdn.net/v/t1.0-9/110117948_283600756398816_9044309464005316960_n.jpg?_nc_cat=101&_nc_sid=8bfeb9&_nc_ohc=u4Qvkou0IwcAX8FqcOb&_nc_ht=scontent-dfw5-1.xx&oh=80f5d3540500543d53df86977d0981c6&oe=5F3E541F"
                });
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