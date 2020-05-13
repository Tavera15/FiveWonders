using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class CategoryManagerController : Controller
    {
        // GET: CategoryManager
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

    }
}