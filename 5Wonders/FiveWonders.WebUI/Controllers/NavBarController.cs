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
            Dictionary<string, HashSet<string>> navLinks = new Dictionary<string, HashSet<string>>();

            // Create a new hash set for each category key
            foreach (Category x in categoryContext.GetCollection().OrderBy(x => x.mTimeEntered).ToArray())
                navLinks.Add(x.mCategoryName, new HashSet<string>());

            // Categories will have a dropdown menu popularized of subcategories
            foreach(Product product in productsContext.GetCollection())
            {
                Category category = categoryContext.Find(product.mCategory);

                // If a product of a certain category has subcategories, add the subcategory name into hashset
                
                if(!String.IsNullOrWhiteSpace(product.mSubCategories))
                {
                    foreach (string subCat in product.mSubCategories.Split(','))
                    {
                        SubCategory sub = subCategoryContext.Find(subCat);

                        navLinks[category.mCategoryName].Add(sub.mSubCategoryName);
                    }
                }
            }

            string[] subsWithThemes = (from sub in subCategoryContext.GetCollection().OrderBy(x => x.mSubCategoryName).ToArray()
                                            where sub.isEventOrTheme
                                            select sub.mSubCategoryName).ToArray();

            ViewBag.SubsWithThemes = subsWithThemes;
            return PartialView(navLinks);
        }
    }
}