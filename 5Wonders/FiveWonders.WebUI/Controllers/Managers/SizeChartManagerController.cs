using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using FiveWonders.Services;
using FluentValidation.Results;

namespace FiveWonders.WebUI.Controllers
{
    [Authorize(Roles = "FWondersAdmin")]
    public class SizeChartManagerController : Controller
    {
        IRepository<SizeChart> sizeChartContext;
        IRepository<Product> productContext;
        IRepository<BasketItem> basketItemContext;
        IImageStorageService imageStorageService;
        IBasketServices basketServices;

        public SizeChartManagerController(IRepository<SizeChart> sizeChartRepository, IRepository<Product> productRepository, IRepository<BasketItem> basketItemRepository, IImageStorageService imageStorageService, IBasketServices basketServices)
        {
            sizeChartContext = sizeChartRepository;
            productContext = productRepository;
            basketItemContext = basketItemRepository;
            this.imageStorageService = imageStorageService;
            this.basketServices = basketServices;
        }

        // GET: SizeChartManager
        public ActionResult Index()
        {
            List<SizeChart> allCharts = sizeChartContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToList();
            return View(allCharts);
        }

        public ActionResult Create()
        {
            return View(new SizeChart());
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(SizeChart chart, string[] selectedSizes, HttpPostedFileBase imageFile)
        {
            try
            {
                // Format chart sizes
                chart.mSizesToDisplay = selectedSizes != null ? String.Join(",", selectedSizes) : "";
                
                // Validate inputs
                SizeChartValidator sizeChartValidator = new SizeChartValidator(sizeChartContext, imageFile);
                ValidationResult validation = sizeChartValidator.Validate(chart);

                if (!validation.IsValid)
                {
                    string errMsg = validation.Errors != null && validation.Errors.Count > 0
                            ? String.Join(",", validation.Errors)
                            : "A category name or image is missing.";

                    throw new Exception(errMsg);
                }

                chart.mImage = ImageStorageService.GetImageBytes(imageFile);
                chart.mImageType = ImageStorageService.GetImageExtension(imageFile);
            
                sizeChartContext.Insert(chart);
                sizeChartContext.Commit();

                return RedirectToAction("Index", "SizeChartManager");
            }
            catch(Exception e)
            {
                ViewBag.errMessages = e.Message.Split(',');
                return View(chart);
            }
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                SizeChart chartToEdit = sizeChartContext.Find(Id, true);

                return View(chartToEdit);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(SizeChart newChart, string[] newSelectedSizes, HttpPostedFileBase newImage, string Id)
        {

            try
            {
                SizeChart chartToEdit = sizeChartContext.Find(Id, true);

                // Save copy of old size chart options
                string[] oldSizes = chartToEdit.mSizesToDisplay.Split(',');

                // Temporarily update Size Chart Object
                chartToEdit.mChartName = newChart.mChartName;
                chartToEdit.mSizesToDisplay = newSelectedSizes != null ? String.Join(",", newSelectedSizes) : "";

                // Validate inputs
                SizeChartValidator sizeChartValidator = new SizeChartValidator(sizeChartContext, newImage);
                ValidationResult validation = sizeChartValidator.Validate(chartToEdit);

                if(!validation.IsValid)
                {
                    string[] errMsg = (validation.Errors != null && validation.Errors.Count > 0)
                        ? validation.Errors.Select(x => x.ErrorMessage).ToArray()
                        : new string[] { "A name, size options, or image is missing." };

                    ViewBag.errMessages = errMsg;
                    return View(chartToEdit);
                }

                if(newImage != null)
                {
                    chartToEdit.mImage = ImageStorageService.GetImageBytes(newImage);
                    chartToEdit.mImageType = ImageStorageService.GetImageExtension(newImage);
                }

                sizeChartContext.Commit();

                // Update basket items if necessary
                bool bUpdateBasketItems = !oldSizes
                    .All(oldSize => newSelectedSizes.Any(size => size == oldSize));

                if(bUpdateBasketItems)
                {
                    // Grab product ids that contain the size chart id
                    string[] productsWithSizeChart = productContext.GetCollection()
                        .Where(product => product.mSizeChart == Id).Select(p => p.mID).ToArray();

                    // Grab basket items that contains any product ids selected from above
                    BasketItem[] basketItemsWithSizeChart = basketItemContext.GetCollection()
                        .Where(basketItem => productsWithSizeChart.Contains(basketItem.mProductID))
                        .ToArray();

                    if(basketItemsWithSizeChart != null && basketItemsWithSizeChart.Length > 0)
                    {
                        // Grab basket items whose 'size' value does not exist anymore
                        string[] basketItemsToDelete = basketItemsWithSizeChart
                            .Where(basketItem => newSelectedSizes.All(size => size != basketItem.mSize))
                            .Select(b => b.mID).ToArray();

                        basketServices.RemoveMultipleBasketItems(basketItemsToDelete);
                    }
                }

                return RedirectToAction("Index", "SizeChartManager");
            }
            catch(Exception e)
            {
                _ = e;
                return RedirectToAction("Edit", "SizeChartManager", new { Id = Id});
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                SizeChart chartToDelete = sizeChartContext.Find(Id, true);

                Product[] productsWithSizeChart = productContext.GetCollection().Where(x => x.mSizeChart == chartToDelete.mID).ToArray();

                ViewBag.productsWithSizeChart = productsWithSizeChart;
                return View(chartToDelete);
            }
            catch(Exception e)
            {
                _ = e;
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                SizeChart chartToDelete = sizeChartContext.Find(Id, true);

                bool bItemsWithChart = productContext.GetCollection().Any(p => p.mSizeChart == Id);

                if (bItemsWithChart)
                {
                    throw new Exception("Products contain targeted size chart");
                }

                sizeChartContext.Delete(chartToDelete);
                sizeChartContext.Commit();

                return RedirectToAction("Index", "SizeChartManager");
            }
            catch (Exception e)
            {
                _ = e;
                return RedirectToAction("Delete", "SizeChartManager", new { Id = Id });
            }
        }
    }
}
