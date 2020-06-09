using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;

namespace FiveWonders.WebUI.Controllers
{
    public class SizeChartManagerController : Controller
    {
        IRepository<SizeChart> sizeChartContext;
        IRepository<Product> productContext;

        public SizeChartManagerController(IRepository<SizeChart> sizeChartRepository, IRepository<Product> productRepository)
        {
            sizeChartContext = sizeChartRepository;
            productContext = productRepository;
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
        public ActionResult Create(SizeChart chart, string[] selectedSizes)
        {
            if(!ModelState.IsValid)
            {
                return View(chart);
            }

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

                return View(chartToEdit);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(SizeChart newChart, string[] newSelectedSizes, string Id)
        {
            if(!ModelState.IsValid || newSelectedSizes == null)
            {
                return View();
            }

            try
            {
                SizeChart chartToEdit = sizeChartContext.Find(Id);

                chartToEdit.mChartName = newChart.mChartName;
                chartToEdit.mImageChartUrl = newChart.mImageChartUrl;
                chartToEdit.mSizesToDisplay = String.Join(",", newSelectedSizes);

                sizeChartContext.Commit();

                return RedirectToAction("Index", "SizeChartManager");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                SizeChart chartToDelete = sizeChartContext.Find(Id);
                Product[] productsWithSizeChart = productContext.GetCollection().Where(x => x.mSizeChart == chartToDelete.mID).ToArray();

                ViewBag.productsWithSizeChart = productsWithSizeChart;
                return View(chartToDelete);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
                Product[] productsWithSizeChart = productContext.GetCollection().Where(x => x.mSizeChart == chartToDelete.mID).ToArray();

                if(productsWithSizeChart.Length != 0)
                {
                    RedirectToAction("Delete", "SizeChartManager");
                }

                sizeChartContext.Delete(chartToDelete);
                sizeChartContext.Commit();

                return RedirectToAction("Index", "SizeChartManager");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return HttpNotFound();
            }
        }
    }
}
