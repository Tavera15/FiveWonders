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
    public class ServicesController : Controller
    {
        IRepository<ServicePage> servicePageContext;

        public ServicesController(IRepository<ServicePage> servicePageRepository)
        {
            servicePageContext = servicePageRepository;
        }

        // GET: Services
        public ActionResult Index()
        {
            ServicePage servicePageData = servicePageContext.GetCollection().FirstOrDefault() ?? new ServicePage();
            ServicesMessage servicesMessage = new ServicesMessage();

            ServicePageViewModel viewModel = new ServicePageViewModel()
            {
                servicePageData = servicePageData,
                servicesMessage = servicesMessage
            };

            return View(viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(ServicePageViewModel viewModel)
        {
            try
            {
                return RedirectToAction("Index", "Products");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                return HttpNotFound();
            }
        }
    }
}