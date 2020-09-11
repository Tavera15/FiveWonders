using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class HomePageViewModel
    {
        public HomePage homePageData { get; set; }
        public List<Product> top3Products { get; set; }
        public List<GalleryImg> top3IGPosts { get; set; }
        public Category clothing { get; set; }
        public Category balloons { get; set; }
    }
}
