﻿using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public IInstagramService InstagramService;
        public IRepository<HomePage> homeContext;
        public IRepository<Product> productsContext;

        public HomeController(IInstagramService IGService, IRepository<HomePage> homeRepository, IRepository<Product> productsRepository)
        {
            InstagramService = IGService;
            homeContext = homeRepository;
            productsContext = productsRepository;
        }

        public ActionResult Index()
        {
            HomePageViewModel homeViewModel = new HomePageViewModel();

            HomePage homeData = homeContext.GetCollection().FirstOrDefault();
            homeData.mWelcomeBtnUrl = homeData.mWelcomeBtnUrl ?? "/products";

            string pic = homeData.mWelcomeImgUrl ?? "FWondersDefault.jpg";
            homeData.mWelcomeImgUrl = "../content/home/" + pic;

            Product[] allProductsSorted = productsContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToArray();
            List<Product> top3Products = allProductsSorted.Take(3).ToList();
            List<GalleryImg> top4GalleryImgs = InstagramService.GetGalleryImgs().Take(4).ToList();
            

            homeViewModel.homePageData = homeData;
            homeViewModel.top3Products = top3Products;
            homeViewModel.top3IGPosts = top4GalleryImgs;

            return View(homeViewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}

//https://www.practicalecommerce.com/4-Simple-Product-Page-Layouts-That-Work