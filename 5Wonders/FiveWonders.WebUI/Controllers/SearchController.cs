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


    public class SearchController : Controller
    {
        IRepository<Product> productsContext;
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subcategoryContext;
        
        public SearchController(IRepository<Product> productsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subcategoryRepository)
        {
            productsContext = productsRepository;
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
            SearchViewModel newSearchViewModel = GetSearchViewModel();

            try
            {
                if(String.IsNullOrWhiteSpace(searchViewModel.productNameinput))
                {
                    throw new Exception("Enter the name for product to search for.");
                }

                Product[] productsWithName = productsContext.GetCollection()
                    .Where(p => p.mName.ToLower().Contains(searchViewModel.productNameinput.ToLower())).ToArray();

                if(productsWithName.Length <= 0)
                {
                    throw new Exception("No products were found that match criteria.");
                }

                newSearchViewModel.results = productsWithName.ToList();
            }
            catch(Exception e)
            {
                newSearchViewModel.message = e.Message;
            }

            return View(newSearchViewModel);
        }

        private SearchViewModel GetSearchViewModel()
        {
            SearchViewModel newSearchViewModel = new SearchViewModel();
            
            newSearchViewModel.allCategories = categoryContext.GetCollection()
                .OrderByDescending(x => x.mTimeEntered).ToArray();

            newSearchViewModel.allSubcategories = subcategoryContext.GetCollection()
                .OrderByDescending(x => x.mTimeEntered).ToArray();

            return newSearchViewModel;
        }
    }
}