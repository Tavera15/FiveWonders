using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;

namespace FiveWonders.WebUI.Controllers
{
    public class ProductManagerController : Controller
    {
        IRepository<Product> context;
        IRepository<Category> productCategories;

        public ProductManagerController(IRepository<Product> productContext, IRepository<Category> categoriesContext)
        {
            context = productContext;
            productCategories = categoriesContext;
        }

        // Should display all products
        public ActionResult Index()
        {
            List<Product> allProducts = context.GetCollection().ToList<Product>();

            return View(allProducts);
        }

        // Form to create a new product
        public ActionResult Create()
        {
            ProductManagerViewModel viewModel = new ProductManagerViewModel();
            viewModel.Product = new Product();
            viewModel.categories = productCategories.GetCollection();

            return View(viewModel);
        }

        // Get form information and store to memory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductManagerViewModel p)
        {
            if(!ModelState.IsValid)
            {
                return View(p);
            }

            // Store into memory. Later in Database
            context.Insert(p.Product);
            context.Commit();

            return RedirectToAction("Index", "ProductManager");
        }

        // Display a form to edit product
        public ActionResult Edit(string Id)
        {
            try
            {
                Product productToEdit = context.Find(Id);

                ProductManagerViewModel viewModel = new ProductManagerViewModel();
                viewModel.Product = productToEdit;
                viewModel.categories = productCategories.GetCollection();

                return View(viewModel);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult Edit(ProductManagerViewModel p, string Id)
        {
            try
            {
                Product target = context.Find(Id);

                if (!ModelState.IsValid)
                    return View(p);

                target.mName = p.Product.mName;
                target.mDesc = p.Product.mDesc;
                target.mPrice = p.Product.mPrice;
                target.mCategory = p.Product.mCategory;

                context.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                Product target = context.Find(Id);

                return View(target);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                Product target = context.Find(Id);

                context.Delete(target);
                context.Commit();

                return RedirectToAction("Index", "ProductManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }
    }
}