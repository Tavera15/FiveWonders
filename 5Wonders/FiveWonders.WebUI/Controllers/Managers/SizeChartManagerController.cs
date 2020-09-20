using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;

namespace FiveWonders.WebUI.Controllers
{
    public class SizeChartManagerController : Controller
    {
        IRepository<SizeChart> sizeChartContext;
        IRepository<Product> productContext;
        IImageStorageService imageStorageService;

        public SizeChartManagerController(IRepository<SizeChart> sizeChartRepository, IRepository<Product> productRepository, IImageStorageService imageStorageService)
        {
            sizeChartContext = sizeChartRepository;
            productContext = productRepository;
            this.imageStorageService = imageStorageService;
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
            if(!ModelState.IsValid || imageFile == null)
            {
                return View(chart);
            }

            string newImageURL;
            imageStorageService.AddImage(EFolderName.SizeCharts, Server, imageFile, chart.mID, out newImageURL);

            chart.mImageChartUrl = newImageURL;
            chart.mSizesToDisplay = String.Join(",", selectedSizes);

            sizeChartContext.Insert(chart);
            sizeChartContext.Commit();

            return RedirectToAction("Index", "SizeChartManager");
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                SizeChart chartToEdit = sizeChartContext.Find(Id);

                if (chartToEdit == null)
                    throw new Exception(Id + " not found");

                return View(chartToEdit);
            }
            catch(Exception e)
            {
                return HttpNotFound();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(SizeChart newChart, string[] newSelectedSizes, HttpPostedFileBase newImage, string Id)
        {

            try
            {
                SizeChart chartToEdit = sizeChartContext.Find(Id);

                if (chartToEdit == null)
                    throw new Exception(Id + " not found");
                
                if(!ModelState.IsValid || newSelectedSizes == null)
                {
                    throw new Exception("Size Chart Edit Model no good");
                }

                chartToEdit.mChartName = newChart.mChartName;
                chartToEdit.mSizesToDisplay = String.Join(",", newSelectedSizes);

                if(newImage != null)
                {
                    string newImageURL;

                    imageStorageService.DeleteImage(EFolderName.SizeCharts, chartToEdit.mImageChartUrl, Server);
                    imageStorageService.AddImage(EFolderName.SizeCharts, Server, newImage, Id, out newImageURL);
                    chartToEdit.mImageChartUrl = newImageURL;
                }

                sizeChartContext.Commit();

                return RedirectToAction("Index", "SizeChartManager");
            }
            catch(Exception e)
            {
                return RedirectToAction("Edit", "SizeChartManager", new { Id = Id});
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                SizeChart chartToDelete = sizeChartContext.Find(Id);

                if (chartToDelete == null)
                    throw new Exception(Id + " not found");

                Product[] productsWithSizeChart = productContext.GetCollection().Where(x => x.mSizeChart == chartToDelete.mID).ToArray();

                ViewBag.productsWithSizeChart = productsWithSizeChart;
                return View(chartToDelete);
            }
            catch(Exception e)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            try
            {
                SizeChart chartToDelete = sizeChartContext.Find(Id);

                if (chartToDelete == null)
                    throw new Exception(Id + " not found");

                bool bItemsWithChart = productContext.GetCollection().Any(p => p.mSizeChart == Id);

                if (bItemsWithChart)
                {
                    throw new Exception("Products contain targeted size chart");
                }

                imageStorageService.DeleteImage(EFolderName.SizeCharts, chartToDelete.mImageChartUrl, Server);

                sizeChartContext.Delete(chartToDelete);
                sizeChartContext.Commit();

                return RedirectToAction("Index", "SizeChartManager");
            }
            catch (Exception e)
            {
                return RedirectToAction("Delete", "SizeChartManager", new { Id = Id });
            }
        }

    }
}
