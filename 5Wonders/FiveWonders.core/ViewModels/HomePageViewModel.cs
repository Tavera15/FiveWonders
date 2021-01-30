 using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class HomePageViewModel
    {
        public HomePage homePageData { get; set; }
        public List<ProductData> top3Products { get; set; }
        public List<GalleryImg> top3IGPosts { get; set; }
        public List<HomeCarouselImages> carouselImages { get; set; }
        public Promo promo1 { get; set; }
        public Promo promo2 { get; set; }
        public string welcomePageUrl { get; set; }
    }

    public class Promo
    {
        public string promoLink { get; set; }
        public string promoImg { get; set; }
        public string promoName { get; set; }
        public float promoImgShader { get; set; }
        public string promoNameColor { get; set; }
    }

    public class ProductData
    {
        public Product product { get; set; }
        public ProductImage firstImage { get; set; }
        public string productCategoryName { get; set; }
    }
}
