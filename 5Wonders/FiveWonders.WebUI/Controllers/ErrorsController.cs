using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class ErrorsController : Controller
    {
        IRepository<HomePage> homePageContext;

        public ErrorsController(IRepository<HomePage> homePageRepository)
        {
            homePageContext = homePageRepository;
        }

        [HandleError]
        // GET: Errors
        public ActionResult PageNotFound()
        {
            HomePage homePageData = homePageContext.GetCollection().FirstOrDefault();
            return View(homePageData);
        }
    }
}