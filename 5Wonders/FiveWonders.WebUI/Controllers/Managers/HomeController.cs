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
        public IRepository<Category> categoryContext;
        public IRepository<SubCategory> subcategoryContext;

        public HomeController(IInstagramService IGService, IRepository<HomePage> homeRepository, IRepository<Product> productsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subcategoryRepository)
        {
            InstagramService = IGService;
            homeContext = homeRepository;
            productsContext = productsRepository;
            categoryContext = categoryRepository;
            subcategoryContext = subcategoryRepository;
        }

        public ActionResult Index()
        {
            HomePageViewModel homeViewModel = new HomePageViewModel();

            HomePage homeData = homeContext.GetCollection().FirstOrDefault() ?? new HomePage();
            
            homeViewModel.welcomePageUrl = "/Products/";

            if (categoryContext.Find(homeData.mWelcomeBtnUrl) != null)
            {
                homeViewModel.welcomePageUrl = "/Products/?Category=" 
                    + categoryContext.Find(homeData.mWelcomeBtnUrl).mCategoryName;
            }
            else if(subcategoryContext.Find(homeData.mWelcomeBtnUrl) != null)
            {
                homeViewModel.welcomePageUrl = "/Products/?Subcategory="
                    + subcategoryContext.Find(homeData.mWelcomeBtnUrl).mSubCategoryName;
            }

            string pic = homeData.mWelcomeImgUrl ?? "";
            homeData.mWelcomeImgUrl = !String.IsNullOrEmpty(pic) 
                ? "../content/home/" + pic
                : "";

            Product[] allProductsSorted = productsContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToArray();
            List<Product> top3Products = allProductsSorted.Take(3).ToList();
            List<GalleryImg> top4GalleryImgs = InstagramService.GetGalleryImgs().Take(4).ToList();

            Promo promo1 = new Promo();
            Promo promo2 = new Promo();

            // Find data for promo 1
            if (categoryContext.Find(homeData.mPromo1) != null)
            {
                Category category = categoryContext.Find(homeData.mPromo1);

                promo1.promoName = category.mCategoryName;
                promo1.promoLink = "/Products/?Category=" + category.mCategoryName;
                promo1.promoImg = "/CategoryImages/" + category.mImgUrL;
                promo1.promoImgShader = category.mImgShaderAmount;
                promo1.promoNameColor = category.bannerTextColor;
            }
            else if(subcategoryContext.Find(homeData.mPromo1) != null)
            {
                SubCategory sub = subcategoryContext.Find(homeData.mPromo1);

                promo1.promoName = sub.mSubCategoryName;
                promo1.promoLink = "/Products/?Subcategory=" + sub.mSubCategoryName;
                promo1.promoImg = "/SubcategoryImages/" + sub.mImageUrl;
                promo1.promoImgShader = sub.mImgShaderAmount;
                promo1.promoNameColor = sub.bannerTextColor;
            }

            // Find data for promo 2
            if (categoryContext.Find(homeData.mPromo2) != null)
            {
                Category category = categoryContext.Find(homeData.mPromo2);

                promo2.promoName = category.mCategoryName;
                promo2.promoLink = "/Products/?Category=" + category.mCategoryName;
                promo2.promoImg = "/CategoryImages/" + category.mImgUrL;
                promo2.promoImgShader = category.mImgShaderAmount;
                promo2.promoNameColor = category.bannerTextColor;
            }
            else if (subcategoryContext.Find(homeData.mPromo2) != null)
            {
                SubCategory sub = subcategoryContext.Find(homeData.mPromo2);

                promo2.promoName = sub.mSubCategoryName;
                promo2.promoLink = "/Products/?Subcategory=" + sub.mSubCategoryName;
                promo2.promoImg = "/SubcategoryImages/" + sub.mImageUrl;
                promo2.promoImgShader = sub.mImgShaderAmount;
                promo2.promoNameColor = sub.bannerTextColor;
            }

            homeViewModel.homePageData = homeData;
            homeViewModel.top3Products = top3Products;
            homeViewModel.top3IGPosts = top4GalleryImgs;
            homeViewModel.promo1 = promo1;
            homeViewModel.promo2 = promo2;

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

//https://www.practicalecommerce.com/4-Simple-Product-Page-Layouts-That-Work