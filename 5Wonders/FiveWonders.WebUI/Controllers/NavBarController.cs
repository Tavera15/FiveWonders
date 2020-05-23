using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class NavBarController : Controller
    {
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subCateroryContext;

        public NavBarController(IRepository<Category> categoryRepository, IRepository<SubCategory> subCategoryRepository)
        {
            categoryContext = categoryRepository;
            subCateroryContext = subCategoryRepository;
        }

        // GET: NavBar
        public ActionResult Index()
        {
            //bool isAdmin = false;

            /*if(isAdmin)
            {
            }
            else
            {

            }*/
            var allCategories = categoryContext.GetCollection().Select(x => x.mCategoryName).ToArray();

            ViewBag.AllCategories = allCategories;
            return PartialView(allCategories);
        }
    }
}