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
        public List<InstagramPost> top3IGPosts { get; set; }
    }
}
