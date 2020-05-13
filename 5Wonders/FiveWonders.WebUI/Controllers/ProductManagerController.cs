using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;

namespace FiveWonders.WebUI.Controllers
{
    public class ProductManagerController : Controller
    {
        ProductRepository context;

        public ProductManagerController()
        {
            context = new ProductRepository();
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
            return View(new Product());
        }

        // Get form information and store to memory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product p)
        {
            if(!ModelState.IsValid)
            {
                return View(p);
            }

            // Store into memory. Later in Database
            context.Insert(p);
            context.Commit();

            return RedirectToAction("Index", "ProductManager");
        }

        // Display a form to edit product
        public ActionResult Edit(string Id)
        {
            try
            {
                Product product = context.Find(Id);
                
                return View(product);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult Edit(Product p, string Id)
        {
            try
            {
                Product target = context.Find(Id);

                if (!ModelState.IsValid)
                    return View(p);

                target.mName = p.mName;
                target.mDesc = p.mDesc;
                target.mPrice = p.mPrice;

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