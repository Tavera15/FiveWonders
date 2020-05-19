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
        IRepository<SizeChart> context;

        public SizeChartManagerController(IRepository<SizeChart> sizeChartRepository)
        {
            context = sizeChartRepository;
        }

        // GET: SizeChartManager
        public ActionResult Index()
        {
            List<SizeChart> allCharts = context.GetCollection().ToList<SizeChart>();
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

            chart.mSizesToDisplay = selectedSizes.ToList();

            context.Insert(chart);
            context.Commit();

            return RedirectToAction("Index", "SizeChartManager");
        }

        public ActionResult Edit(string Id)
        {
            try
            {
                SizeChart chartToEdit = context.Find(Id);

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
                SizeChart chartToEdit = context.Find(Id);

                chartToEdit.mChartName = newChart.mChartName;
                chartToEdit.mImageChartUrl = newChart.mImageChartUrl;
                chartToEdit.mSizesToDisplay = newSelectedSizes.ToList();

                context.Commit();

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
                SizeChart chartToDelete = context.Find(Id);

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
                SizeChart chartToDelete = context.Find(Id);

                context.Delete(chartToDelete);
                context.Commit();

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

//https://stackoverflow.com/questions/12316441/asp-net-mvc-binding-to-a-dictionary-in-view