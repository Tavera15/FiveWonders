using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;

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

            ServicePageViewModel viewModel = new ServicePageViewModel()
            {
                servicePageData = servicePageData,
                servicesMessage = new ServicesMessage()
            };

            return View(viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(ServicePageViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid || viewModel.servicesMessage == null ||
                    String.IsNullOrWhiteSpace(viewModel.servicesMessage.mSubject) ||
                    String.IsNullOrWhiteSpace(viewModel.servicesMessage.mContent))
                {
                    throw new Exception("Services model no good");
                }

                string fixedCustomerName = "<p>" + viewModel.servicesMessage.mCustomerName + "</p>";
                string fixedCustomerPhone = "<p>" + viewModel.servicesMessage.mPhoneNumber + "</p>";
                string fixedCustomerEmail = "<p>" + viewModel.servicesMessage.mEmail + "</p>";

                MailMessage message = new MailMessage();
                message.To.Add("");
                message.From = new MailAddress("");
                message.Subject = viewModel.servicesMessage.mSubject;
                message.IsBodyHtml = true;
                message.Body = viewModel.servicesMessage.mContent 
                    + "<br />" + fixedCustomerName + fixedCustomerEmail + fixedCustomerPhone;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential("", ""),
                    EnableSsl = true
                };

                smtpClient.Send(message);

                return RedirectToAction("Index", "Products");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                return RedirectToAction("Index", "Services");
            }
        }
    }
}