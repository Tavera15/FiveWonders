using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    public class ServicesManagerController : Controller
    {
        IRepository<ServicesMessage> serviceContext;

        public ServicesManagerController(IRepository<ServicesMessage> serviceMessagesRepository)
        {
            serviceContext = serviceMessagesRepository;
        }

        // GET: ServicesManager
        public ActionResult Index()
        {
            List<ServicesMessage> allServiceMessages = serviceContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToList();
            
            return View(allServiceMessages);
        }

        public ActionResult Details(string Id)
        {
            try
            {
                ServicesMessage message = serviceContext.Find(Id);

                return View(message);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                return HttpNotFound();
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                ServicesMessage message = serviceContext.Find(Id);

                return View(message);
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
                ServicesMessage message = serviceContext.Find(Id);

                serviceContext.Delete(message);
                serviceContext.Commit();

                return RedirectToAction("Index", "ServicesManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                return HttpNotFound();
            }
        }
    }
}