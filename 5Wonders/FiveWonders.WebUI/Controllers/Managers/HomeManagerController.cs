using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class HomeManagerController : Controller
    {
        public IRepository<HomePage> homeDataContext;
        public IRepository<Category> categoryContext;
        public IRepository<SubCategory> subcategoryContext;
        public IImageStorageService imageStorageSystem;

        public HomeManagerController(IRepository<HomePage> homePageRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subContext, IImageStorageService imageStorageSystem)
        {
            homeDataContext = homePageRepository;
            categoryContext = categoryRepository;
            subcategoryContext = subContext;
            this.imageStorageSystem = imageStorageSystem;
        }

        // GET: HomeManager
        public ActionResult Index()
        {
            try
            {
                HomePageManagerViewModel viewModel = GetFilledViewModel();
                viewModel.homePagedata = homeDataContext.GetCollection().FirstOrDefault() ?? new HomePage();
                
                return View(viewModel);
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Index", "Home");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(HomePageManagerViewModel updatedDataViewModel, HttpPostedFileBase homeLogo, HttpPostedFileBase homeImg, HttpPostedFileBase default_bannerImg, string[] checkedCarouselImgs, HttpPostedFileBase[] newCarouselImgs)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("Home Manager model not good");

                HomePage existingData = homeDataContext.GetCollection().FirstOrDefault() ?? updatedDataViewModel.homePagedata;
                
                existingData.mWelcomeBtnUrl = updatedDataViewModel.homePagedata.mWelcomeBtnUrl;
                existingData.mdefaultBannerTextColor = updatedDataViewModel.homePagedata.mdefaultBannerTextColor;
                existingData.mDefaultProductsBannerText = updatedDataViewModel.homePagedata.mDefaultProductsBannerText;
                existingData.mHomePageGreeting = updatedDataViewModel.homePagedata.mHomePageGreeting;
                existingData.defaultBannerImgShader = updatedDataViewModel.homePagedata.defaultBannerImgShader;
                existingData.mPromo1 = updatedDataViewModel.homePagedata.mPromo1;
                existingData.mPromo2 = updatedDataViewModel.homePagedata.mPromo2;
                existingData.welcomeGreetingImgShader = updatedDataViewModel.homePagedata.welcomeGreetingImgShader;
                existingData.mEnableWelcomeImg = updatedDataViewModel.homePagedata.mEnableWelcomeImg;
                
                if(homeLogo != null)
                {
                    imageStorageSystem.DeleteImage(EFolderName.Home, existingData.mHomePageLogoUrl, Server);

                    string newLogoUrl;
                    imageStorageSystem.AddImage(EFolderName.Home, Server, homeLogo, existingData.mID, out newLogoUrl, "Logo");
                    existingData.mHomePageLogoUrl = newLogoUrl;
                }

                if (homeImg != null)
                {
                    imageStorageSystem.DeleteImage(EFolderName.Home, existingData.mWelcomeImgUrl, Server);

                    string newWelcomeImgUrl;
                    imageStorageSystem.AddImage(EFolderName.Home, Server, homeImg, existingData.mID, out newWelcomeImgUrl, "DefaultHomeBanner");
                    existingData.mWelcomeImgUrl = newWelcomeImgUrl;
                }

                if (default_bannerImg != null)
                {
                    imageStorageSystem.DeleteImage(EFolderName.Home, existingData.mDefaultProductListImgUrl, Server);

                    string newProductListImgUrl;
                    imageStorageSystem.AddImage(EFolderName.Home, Server, default_bannerImg, existingData.mID, out newProductListImgUrl, "DefaultProductsBanner");
                    existingData.mDefaultProductListImgUrl = newProductListImgUrl;
                }

                // Check for new images
                string[] savedCarouselImgs = !String.IsNullOrWhiteSpace(existingData.mCarouselImgs)
                    ? existingData.mCarouselImgs.Split(',')
                    : null;
                
                // Update Images if new ones are selected, or old ones were checked off
                if((newCarouselImgs != null && newCarouselImgs[0] != null) 
                    || (checkedCarouselImgs == null && savedCarouselImgs != null)
                    || (savedCarouselImgs != null && savedCarouselImgs.Length != checkedCarouselImgs.Length))
                {
                    string newCarouselImgUrl;
                    imageStorageSystem.UpdateImages(Server, EFolderName.Home, savedCarouselImgs, checkedCarouselImgs, newCarouselImgs, out newCarouselImgUrl, "carousel-");
                    existingData.mCarouselImgs = newCarouselImgUrl;
                }

                if (existingData.mID == updatedDataViewModel.homePagedata.mID)
                {
                    homeDataContext.Insert(existingData);
                }

                homeDataContext.Commit();

                return RedirectToAction("Index", "Managers");
            }
            catch(Exception e)
            {
                _ = e;
                HomePageManagerViewModel errorViewModel = GetFilledViewModel();
                errorViewModel.homePagedata = updatedDataViewModel.homePagedata;

                return View(errorViewModel);
            }
        }

        private HomePageManagerViewModel GetFilledViewModel()
        {

            Dictionary<string, string> links = new Dictionary<string, string>();
            Dictionary<string, string> promoLinks = new Dictionary<string, string>();

            links.Add("0", "All Products");
            promoLinks.Add("0", "Empty");

            foreach (Category cat in categoryContext.GetCollection())
            {
                links.Add(cat.mID, "/products/?category=" + cat.mCategoryName);
                promoLinks.Add(cat.mID, "/products/?category=" + cat.mCategoryName);
            }

            foreach (SubCategory sub in subcategoryContext.GetCollection())
            {
                links.Add(sub.mID, "/products/?subcategory=" + sub.mSubCategoryName);

                if (sub.isEventOrTheme)
                {
                    promoLinks.Add(sub.mID, "/products/?subcategory=" + sub.mSubCategoryName);
                }
            }

            HomePageManagerViewModel viewModel = new HomePageManagerViewModel()
            {
                btnRediLinks = links,
                promoLinks = promoLinks
            };

            return viewModel;
        }
    }
}