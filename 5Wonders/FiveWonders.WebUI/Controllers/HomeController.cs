using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class HomeController : Controller
    {
        IInstagramService InstagramService;
        IRepository<HomePage> homeContext;
        IRepository<Product> productsContext;
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subcategoryContext;
        IRepository<ServicePage> servicePageContext;
        IRepository<SocialMedia> socialMediaContext;

        public HomeController(IInstagramService IGService, IRepository<HomePage> homeRepository, IRepository<Product> productsRepository, IRepository<Category> categoryRepository, IRepository<SubCategory> subcategoryRepository, IRepository<ServicePage> servicePageRepository, IRepository<SocialMedia> socialMediaRepository)
        {
            InstagramService = IGService;
            homeContext = homeRepository;
            productsContext = productsRepository;
            categoryContext = categoryRepository;
            subcategoryContext = subcategoryRepository;
            servicePageContext = servicePageRepository;
            socialMediaContext = socialMediaRepository;
        }

        public ActionResult Index()
        {
            HomePageViewModel homeViewModel = new HomePageViewModel();

            HomePage homeData = homeContext.GetCollection().FirstOrDefault() ?? new HomePage();
            
            homeViewModel.welcomePageUrl = "/Products/";

            if (categoryContext.Find(homeData.mWelcomeBtnUrl) != null)
            {
                homeViewModel.welcomePageUrl = "/Products/?Category=" 
                    + categoryContext.Find(homeData.mWelcomeBtnUrl).mCategoryName;
            }
            else if(subcategoryContext.Find(homeData.mWelcomeBtnUrl) != null)
            {
                homeViewModel.welcomePageUrl = "/Products/?Subcategory="
                    + subcategoryContext.Find(homeData.mWelcomeBtnUrl).mSubCategoryName;
            }

            string pic = homeData.mWelcomeImgUrl ?? "";
            homeData.mWelcomeImgUrl = !String.IsNullOrEmpty(pic) 
                ? "../content/home/" + pic
                : "";

            Product[] allProductsSorted = productsContext.GetCollection().Where(p => p.isDisplayed).OrderByDescending(x => x.mTimeEntered).ToArray();
            List<GalleryImg> top4GalleryImgs = InstagramService.GetGalleryImgs().Take(5).ToList();
            List<Product> top3Products = allProductsSorted.Take(3).ToList();
            List<ProductData> top3ProductsData = new List<ProductData>();

            foreach(Product p in top3Products)
            {
                try
                {
                    Category cat = categoryContext.Find(p.mCategory, true);

                    ProductData productData = new ProductData();
                    productData.product = p;
                    productData.productCategoryName = cat.mCategoryName;

                    top3ProductsData.Add(productData);
                }
                catch(Exception e)
                {
                    top3ProductsData = new List<ProductData>();
                    break;
                }
                
            }

            Promo promo1 = new Promo();
            Promo promo2 = new Promo();

            // Find data for promo 1
            if (categoryContext.Find(homeData.mPromo1) != null)
            {
                Category category = categoryContext.Find(homeData.mPromo1);

                promo1.promoName = category.mCategoryName;
                promo1.promoLink = "/Products/?Category=" + category.mCategoryName;
                promo1.promoImg = "/CategoryImages/" + category.mImgUrL;
                promo1.promoImgShader = category.mImgShaderAmount;
                promo1.promoNameColor = category.bannerTextColor;
            }
            else if(subcategoryContext.Find(homeData.mPromo1) != null)
            {
                SubCategory sub = subcategoryContext.Find(homeData.mPromo1);

                promo1.promoName = sub.mSubCategoryName;
                promo1.promoLink = "/Products/?Subcategory=" + sub.mSubCategoryName;
                promo1.promoImg = "/SubcategoryImages/" + sub.mImageUrl;
                promo1.promoImgShader = sub.mImgShaderAmount;
                promo1.promoNameColor = sub.bannerTextColor;
            }

            // Find data for promo 2
            if (categoryContext.Find(homeData.mPromo2) != null)
            {
                Category category = categoryContext.Find(homeData.mPromo2);

                promo2.promoName = category.mCategoryName;
                promo2.promoLink = "/Products/?Category=" + category.mCategoryName;
                promo2.promoImg = "/CategoryImages/" + category.mImgUrL;
                promo2.promoImgShader = category.mImgShaderAmount;
                promo2.promoNameColor = category.bannerTextColor;
            }
            else if (subcategoryContext.Find(homeData.mPromo2) != null)
            {
                SubCategory sub = subcategoryContext.Find(homeData.mPromo2);

                promo2.promoName = sub.mSubCategoryName;
                promo2.promoLink = "/Products/?Subcategory=" + sub.mSubCategoryName;
                promo2.promoImg = "/SubcategoryImages/" + sub.mImageUrl;
                promo2.promoImgShader = sub.mImgShaderAmount;
                promo2.promoNameColor = sub.bannerTextColor;
            }

            homeViewModel.homePageData = homeData;
            homeViewModel.top3Products = top3ProductsData;
            homeViewModel.top3IGPosts = top4GalleryImgs;
            homeViewModel.promo1 = promo1;
            homeViewModel.promo2 = promo2;

            return View(homeViewModel);
        }

        public ActionResult Contact()
        {
            ServicePageViewModel viewModel = GetContactPageViewModel();

            return View(viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Contact(ServicePageViewModel viewModel)
        {
            try
            {
                ServicePage servicePageData = servicePageContext.GetCollection().FirstOrDefault();

                if(servicePageData == null || !servicePageData.mEnableForm)
                {
                    throw new Exception("Message Submission is not allowed.");
                }

                if (!ModelState.IsValid || viewModel.servicesMessage == null ||
                    String.IsNullOrWhiteSpace(viewModel.servicesMessage.mSubject) ||
                    String.IsNullOrWhiteSpace(viewModel.servicesMessage.mContent))
                {
                    throw new Exception("Services model no good");
                }

                string customerSection = "<h4>Customer Info</h4>";
                string fixedCustomerName = "<p>Name: " + viewModel.servicesMessage.mCustomerName.Trim() + "</p>";
                string fixedCustomerPhone = "<p>Phone Number: " + viewModel.servicesMessage.mPhoneNumber.Trim() + "</p>";
                string fixedCustomerEmail = "<p>Email: " + viewModel.servicesMessage.mEmail.Trim() + "</p>";

                MailMessage message = new MailMessage();
                message.To.Add("");
                message.From = new MailAddress("");
                message.Subject = viewModel.servicesMessage.mSubject;
                message.IsBodyHtml = true;
                message.Body = viewModel.servicesMessage.mContent
                    + "<br />" + customerSection + fixedCustomerName + fixedCustomerEmail + fixedCustomerPhone;

                //throw new Exception("stop");

                SmtpClient smtp = new SmtpClient();
                smtp.Send(message);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                _ = e;
                ServicePage servicePageData = servicePageContext.GetCollection().FirstOrDefault() ?? new ServicePage();
                viewModel.servicePageData = servicePageData;

                return View(viewModel);
            }
        }

        private ServicePageViewModel GetContactPageViewModel()
        {
            ServicePage servicePageData = servicePageContext.GetCollection().FirstOrDefault()
                ?? new ServicePage();

            HomePage homePageData = homeContext.GetCollection().FirstOrDefault() ?? new HomePage();

            ServicePageViewModel viewModel = new ServicePageViewModel()
            {
                servicePageData = servicePageData,
                servicesMessage = new ServicesMessage(),
                logo = String.IsNullOrEmpty(homePageData.mHomePageLogoUrl) ? "" : homePageData.mHomePageLogoUrl,
                communicativeSocialMedias = socialMediaContext.GetCollection().Where(sm => sm.isCommunicative).ToArray()
            };

            return viewModel;
        }
    }
}

//https://www.practicalecommerce.com/4-Simple-Product-Page-Layouts-That-Work