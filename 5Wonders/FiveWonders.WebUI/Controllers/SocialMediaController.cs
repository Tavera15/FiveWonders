using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class SocialMediaController : Controller
    {
        IRepository<SocialMedia> socialMediaContext;

        public SocialMediaController(IRepository<SocialMedia> socialMediaRepository)
        {
            socialMediaContext = socialMediaRepository;
        }

        // GET: SocialMedia
        public ActionResult Index(string Id)
        {
            SocialMedia[] allMedias = socialMediaContext.GetCollection().ToArray();

            ViewBag.iconSize = Id + "vh";
            return PartialView(allMedias);
        }
    }
}