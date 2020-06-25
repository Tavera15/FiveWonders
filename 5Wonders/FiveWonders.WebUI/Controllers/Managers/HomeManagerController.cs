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
        public ActionResult Index(HomePage homeData, HttpPostedFileBase homeImg)
        {
            try
            {
                HomePage singularHome = homeDataContext.GetCollection().FirstOrDefault() ?? homeData;
                singularHome.mWelcomeBtnUrl = homeData.mWelcomeBtnUrl;
                
                DeleteHomeImage(singularHome.mWelcomeImgUrl);
                
                string newImageURL;
                AddNewHomeImage(homeImg, out newImageURL);
                singularHome.mWelcomeImgUrl = newImageURL;

                if(singularHome.mID == homeData.mID)
                {
                    homeDataContext.Insert(singularHome);
                }

                homeDataContext.Commit();
            }
            catch(Exception e)
            {
                return View(homeData);
            }
            return RedirectToAction("Index", "Home");
        }

        public void DeleteHomeImage(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                return;

            try
            {
                string path = Server.MapPath("//Content//Home//") + fileName;

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                else
                {
                    throw new Exception("Error: " + fileName + " not found");
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        private void AddNewHomeImage(HttpPostedFileBase imageFile, out string newImageURL)
        {
            newImageURL = imageFile != null ? "Home_" + imageFile.FileName : null;

            if (imageFile == null) { return; }
            imageFile.SaveAs(Server.MapPath("//Content//Home//") + newImageURL);
        }
    }
}