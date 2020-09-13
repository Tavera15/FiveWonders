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

                string customerSection = "<h4>Customer Info</h4>";
                string fixedCustomerName = "<p>Name: " + viewModel.servicesMessage.mCustomerName + "</p>";
                string fixedCustomerPhone = "<p>Phone Number: " + viewModel.servicesMessage.mPhoneNumber + "</p>";
                string fixedCustomerEmail = "<p>Email: " + viewModel.servicesMessage.mEmail + "</p>";

                MailMessage message = new MailMessage();
                message.To.Add("");
                message.From = new MailAddress("");
                message.Subject = viewModel.servicesMessage.mSubject;
                message.IsBodyHtml = true;
                message.Body = viewModel.servicesMessage.mContent 
                    + "<br />" + customerSection + fixedCustomerName + fixedCustomerEmail + fixedCustomerPhone;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential("", ""),
                    EnableSsl = true
                };

                throw new Exception("stop");
                smtpClient.Send(message);

                return RedirectToAction("Index", "Products");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                ServicePage servicePageData = servicePageContext.GetCollection().FirstOrDefault() ?? new ServicePage();
                viewModel.servicePageData = servicePageData;

                return View(viewModel);
            }
        }
    }
}