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

        public ActionResult Create(int newRows = 3, int newCols = 4)
        {
            SizeChart chart = new SizeChart();
            
            for(int i = 0; i < newRows; i++)
            {
                if(!chart.mChartEntries.ContainsKey(i))
                {
                    var x = new List<string>();
                    chart.mChartEntries.Add(i, x);
                }

                for (int j = 0; j < newCols; j++)
                {
                    var y = "[" + i + ", " + j + "]";
                    chart.mChartEntries[i].Add(y);
                }
            }

            ViewBag.Rows = newRows;
            ViewBag.Cols = newCols;
            return View(chart);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(SizeChart chart, string[][] chartValues)
        {
            foreach(var x in chartValues)
            {
                foreach(var y in x)
                {
                    System.Diagnostics.Debug.WriteLine(x + ", " + y);
                }
            }

            return RedirectToAction("Index", "SizeChartManager");
        }

        public ActionResult UpdateRowsAndCols(int rows, int cols)
        {
            return RedirectToAction("Create", "SizeChartManager", new { newRows = rows, newCols = cols });
        }
    }
}

//https://stackoverflow.com/questions/12316441/asp-net-mvc-binding-to-a-dictionary-in-view