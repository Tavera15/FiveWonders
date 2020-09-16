using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
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
                HomePage home = homeDataContext.GetCollection().FirstOrDefault() ?? new HomePage();
                Dictionary<string, string> links = new Dictionary<string, string>();

                foreach(var cat in categoryContext.GetCollection())
                {
                    links.Add(cat.mCategoryName, "/products/?category=" + cat.mCategoryName);
                }

                foreach (var sub in subcategoryContext.GetCollection())
                {
                    links.Add(sub.mSubCategoryName, "/products/?subcategory=" + sub.mSubCategoryName);
                }

                ViewBag.links = links;
                return View(home);
            }
            catch(Exception e)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(HomePage updatedData, HttpPostedFileBase homeLogo, HttpPostedFileBase homeImg, HttpPostedFileBase default_bannerImg)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("Home Manager model not good");

                HomePage existingData = homeDataContext.GetCollection().FirstOrDefault() ?? updatedData;
                
                existingData.mWelcomeBtnUrl = updatedData.mWelcomeBtnUrl;
                existingData.mdefaultBannerTextColor = updatedData.mdefaultBannerTextColor;
                existingData.mDefaultProductsBannerText = updatedData.mDefaultProductsBannerText;
                existingData.mHomePageGreeting = updatedData.mHomePageGreeting;
                existingData.defaultBannerImgShader = updatedData.defaultBannerImgShader;
                
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

                if (existingData.mID == updatedData.mID)
                {
                    homeDataContext.Insert(existingData);
                }

                homeDataContext.Commit();

                return RedirectToAction("Index", "Home");
            }
            catch(Exception e)
            {
                return View(updatedData);
            }
        }
    }
}