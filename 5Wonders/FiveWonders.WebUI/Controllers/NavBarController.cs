using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
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
        IRepository<SubCategory> subCategoryContext;
        IRepository<Product> productsContext;

        public NavBarController(IRepository<Category> categoryRepository, IRepository<SubCategory> subCategoryRepository, IRepository<Product> productsRepository)
        {
            categoryContext = categoryRepository;
            subCategoryContext = subCategoryRepository;
            productsContext = productsRepository;
        }

        // GET: NavBar
        public ActionResult Index()
        {
            // Key is the category - Values are subcategories.
            // Categories will have a dropdown menu of subcategories
            Dictionary<string, HashSet<string>> navLinks = new Dictionary<string, HashSet<string>>();

            foreach (var x in categoryContext.GetCollection())
                navLinks.Add(x.mCategoryName, new HashSet<string>());

            foreach(var x in productsContext.GetCollection())
            {
                Category category = categoryContext.Find(x.mCategory);

                foreach (var y in x.mSubCategories.Split(','))
                {
                    SubCategory sub = subCategoryContext.Find(y);

                    navLinks[category.mCategoryName].Add(sub.mSubCategoryName);
                }
            }

            var allCategories = categoryContext.GetCollection().Select(x => x.mCategoryName).ToArray();

            string[] subsWithThemes = (from x in subCategoryContext.GetCollection()
                                            where x.isEventOrTheme
                                            select x.mSubCategoryName).ToArray();

            ViewBag.SubsWithThemes = subsWithThemes;
            return PartialView(navLinks);
        }
    }
}