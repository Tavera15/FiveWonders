using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FiveWonders.WebUI.Controllers
{
    public class SearchController : Controller
    {
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subcategoryContext;
        
        public SearchController(IRepository<Category> categoryRepository, IRepository<SubCategory> subcategoryRepository)
        {
            categoryContext = categoryRepository;
            subcategoryContext = subcategoryRepository;
        }

        // GET: Search
        public ActionResult Index()
        {
            return View(GetSearchViewModel());
        }
        
        [HttpPost]
        public ActionResult Index(SearchViewModel searchViewModel)
        {
            RouteValueDictionary parameters = new RouteValueDictionary
            {
                ["Category"] = searchViewModel.categoryInput,
                ["productName"] = searchViewModel.productNameinput,
                ["page"] = 1
            };

            if (searchViewModel.subCategories != null)
            {
                for (int i = 0; i < searchViewModel.subCategories.Length; i++)
                {
                    parameters["Subcategory[" + i + "]"] = searchViewModel.subCategories[i];
                }
            }

            return RedirectToAction("Index", "Products", parameters);
        }

        private SearchViewModel GetSearchViewModel()
        {
            SearchViewModel newSearchViewModel = new SearchViewModel
            {
                allCategories = categoryContext.GetCollection()
                    .OrderByDescending(x => x.mTimeEntered)
                    .Select(cat => cat.mCategoryName).ToList(),

                allSubcategories = subcategoryContext.GetCollection()
                    .OrderByDescending(x => x.mTimeEntered)
                    .Select(sub => sub.mSubCategoryName).ToList()
            };

            newSearchViewModel.allCategories.Insert(0, "");

            return newSearchViewModel;
        }
    }
}