using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using FiveWonders.Services;
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
        public IRepository<HomeCarouselImages> homeCarouselImageContext;
        public IImageStorageService imageStorageSystem;

        public HomeManagerController(IRepository<HomePage> homePageRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subContext, IRepository<HomeCarouselImages> homeCarouselImageRepository, IImageStorageService imageStorageSystem)
        {
            homeDataContext = homePageRepository;
            categoryContext = categoryRepository;
            subcategoryContext = subContext;
            homeCarouselImageContext = homeCarouselImageRepository;
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
                
                // Update Website Logo
                if(homeLogo != null)
                {
                    existingData.mWebsiteLogo = ImageStorageService.GetImageBytes(homeLogo);
                    existingData.mWebsiteLogoImgType = ImageStorageService.GetImageExtension(homeLogo);
                }

                // Update Home Page Greeting Img
                if(homeImg != null)
                {
                    existingData.mWelcomeImgID = ImageStorageService.GetImageBytes(homeImg);
                    existingData.mWelcomeImgType = ImageStorageService.GetImageExtension(homeImg);
                }

                // Update Default Products List Page Banner Img
                if(default_bannerImg != null)
                {
                    existingData.mDefaultProductListImg = ImageStorageService.GetImageBytes(default_bannerImg);
                    existingData.mDefaultProductListImgType = ImageStorageService.GetImageExtension(default_bannerImg);
                }

                // Update Home Page Carousel Imgs
                if(checkedCarouselImgs == null 
                    || checkedCarouselImgs.Length != existingData.mCarouselImgIDs.Split(',').Length
                    || (newCarouselImgs != null && newCarouselImgs[0] != null))
                {
                    List<string> newCarouselIDs = new List<string>();

                    // Keep existing images
                    if(checkedCarouselImgs != null)
                    {
                        foreach(string img in checkedCarouselImgs)
                        {
                            newCarouselIDs.Add(img);
                        }
                    }

                    // Delete images not selected to keep
                    if(checkedCarouselImgs == null || 
                        (!String.IsNullOrWhiteSpace(existingData.mCarouselImgIDs) && checkedCarouselImgs.Length != existingData.mCarouselImgIDs.Split(',').Length))
                    {
                        if(!String.IsNullOrWhiteSpace(existingData.mCarouselImgIDs))
                        {
                            foreach(string img in existingData.mCarouselImgIDs.Split(','))
                            {
                                if (newCarouselIDs.Contains(img)) { continue; }

                                HomeCarouselImages carouselImage = homeCarouselImageContext.Find(img);

                                if(carouselImage != null)
                                {
                                    homeCarouselImageContext.Delete(carouselImage);
                                }
                            }
                        }
                    }

                    // Add new images that were uploaded
                    if(newCarouselImgs != null && newCarouselImgs[0] != null)
                    {
                        foreach(HttpPostedFileBase newImg in newCarouselImgs)
                        {
                            HomeCarouselImages carouselImage = new HomeCarouselImages();
                            carouselImage.mImage = ImageStorageService.GetImageBytes(newImg);
                            carouselImage.mImageType = ImageStorageService.GetImageExtension(newImg);
                            homeCarouselImageContext.Insert(carouselImage);

                            newCarouselIDs.Add(carouselImage.mID);
                        }
                    }

                    homeCarouselImageContext.Commit();
                    existingData.mCarouselImgIDs = String.Join(",", newCarouselIDs);
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
                promoLinks = promoLinks,
                carouselImages = homeCarouselImageContext.GetCollection().ToList()
            };

            return viewModel;
        }
    }
}