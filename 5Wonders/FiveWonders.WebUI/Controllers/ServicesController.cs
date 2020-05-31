using FiveWonders.core.Models;
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
        IRepository<ServicesMessage> serviceContext;

        public ServicesController(IRepository<ServicesMessage> serviceMessagesRepository)
        {
            serviceContext = serviceMessagesRepository;
        }

        // GET: Services
        public ActionResult Index()
        {
            return View(new ServicesMessage());
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(ServicesMessage message)
        {
            if(!ModelState.IsValid)
            {
                return View(message);
            }

            try
            {
                serviceContext.Insert(message);
                serviceContext.Commit();

                System.Diagnostics.Debug.WriteLine(message.mCustomerName);
                System.Diagnostics.Debug.WriteLine(message.mEmail);
                System.Diagnostics.Debug.WriteLine(message.mSubject);
                System.Diagnostics.Debug.WriteLine(message.mContent);

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