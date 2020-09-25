using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers.Managers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class ManagersController : Controller
    {
        // GET: Managers
        public ActionResult Index()
        {
            return View();
        }
    }
}