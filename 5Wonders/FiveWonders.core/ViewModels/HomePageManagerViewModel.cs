using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class HomePageManagerViewModel
    {
        public HomePage homePagedata { get; set; }
        public Dictionary<string, string> btnRediLinks { get; set; }
        public Dictionary<string, string> promoLinks { get; set; }
        public List<HomeCarouselImages> carouselImages { get; set; }
    }
}
