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

        public HomeManagerController(IRepository<HomePage> homePageRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subContext)
        {
            homeDataContext = homePageRepository;
            categoryContext = categoryRepository;
            subcategoryContext = subContext;
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
                    DeleteImage(existingData.mHomePageLogoUrl);

                    string newLogoUrl;
                    AddImage(existingData.mID, homeLogo, out newLogoUrl);
                    existingData.mHomePageLogoUrl = newLogoUrl;
                }

                if (homeImg != null)
                {
                    DeleteImage(existingData.mWelcomeImgUrl);

                    string newWelcomeImgUrl;
                    AddImage(existingData.mID, homeImg, out newWelcomeImgUrl);
                    existingData.mWelcomeImgUrl = newWelcomeImgUrl;
                }

                if (default_bannerImg != null)
                {
                    DeleteImage(existingData.mDefaultProductListImgUrl);

                    string newProductListImgUrl;
                    AddImage(existingData.mID, default_bannerImg, out newProductListImgUrl);
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

        private void AddImage(string Id, HttpPostedFileBase imageFile, out string newImageURL)
        {
            string fileNameWithoutSpaces = String.Concat(imageFile.FileName.Where(c => !Char.IsWhiteSpace(c)));

            newImageURL = Id + fileNameWithoutSpaces;
            imageFile.SaveAs(Server.MapPath("//Content//Home//") + newImageURL);
        }

        private void DeleteImage(string currentImageURL)
        {
            string path = Server.MapPath("//Content//Home//") + currentImageURL;

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}