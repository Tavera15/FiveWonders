using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class ServicesManagerController : Controller
    {
        IRepository<ServicePage> servicePageContext;

        public ServicesManagerController(IRepository<ServicePage> servicePageRepository)
        {
            servicePageContext = servicePageRepository;
        }

        // GET: ServicesManager
        public ActionResult Index()
        {
            ServicePage servicePageData = servicePageContext.GetCollection().FirstOrDefault() ?? new ServicePage();

            ServicePage servicePageWithDiffId = new ServicePage()
            {
                mBannerMessage = servicePageData.mBannerMessage,
                mEmail = servicePageData.mEmail,
                mName = servicePageData.mName,
                mPhoneNumber = servicePageData.mPhoneNumber,
                mEnableForm = servicePageData.mEnableForm
            };

            return View(servicePageWithDiffId);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(ServicePage updatedPage)
        {
            try
            {
                ServicePage target = servicePageContext.GetCollection().FirstOrDefault() ?? updatedPage;

                target.mBannerMessage = updatedPage.mBannerMessage;
                target.mName = updatedPage.mName;
                target.mEmail = updatedPage.mEmail;
                target.mPhoneNumber = updatedPage.mPhoneNumber;
                target.mEnableForm = updatedPage.mEnableForm;

                if(updatedPage.mID == target.mID)
                {
                    servicePageContext.Insert(target);
                }

                servicePageContext.Commit();

                return RedirectToAction("Index", "Managers");
            }
            catch(Exception e)
            {
                _ = e;
               return View(updatedPage);
            }
        }
    }
}