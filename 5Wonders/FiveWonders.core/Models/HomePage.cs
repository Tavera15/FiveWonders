using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FiveWonders.core.Models
{
    public class HomePage : BaseEntity
    {
        [Display(Name = "Home Page Welcome Image")]
        public byte[] mWelcomeImgID { get; set; }
        public string mWelcomeImgType { get; set; }


        [Display(Name = "Enable Greeting Image")]
        public bool mEnableWelcomeImg { get; set; }

        [Display(Name = "Greeting Image Shader")]
        [Range(0, 1)]
        public float welcomeGreetingImgShader { get; set; }

        [Display(Name = "Home Page Button Url")]
        public string mWelcomeBtnUrl { get; set; }


        [Display(Name = "Website Logo")]
        public byte[] mWebsiteLogo { get; set; }
        public string mWebsiteLogoImgType { get; set; }


        [Display(Name = "Home Greeting Greeting")]
        [AllowHtml]
        [Required(ErrorMessage = "A Greeting is required.")]
        public string mHomePageGreeting { get; set; }

        [Display(Name = "Category 1 to promote")]
        public string mPromo1 { get; set; }

        [Display(Name = "Category 2 to promote")]
        public string mPromo2 { get; set; }


        [Display(Name = "Carousel Images")]
        public string mCarouselImgIDs { get; set; }


        [Required(ErrorMessage = "A Banner text is required for Product List.")]
        [Display(Name = "Default Banner Text")]
        public string mDefaultProductsBannerText { get; set; }


        [Display(Name = "Default Banner Image")]
        public byte[] mDefaultProductListImg { get; set; }
        public string mDefaultProductListImgType { get; set; }


        [Display(Name = "Default Banner Image Shader")]
        [Range(0, 1)]
        public float defaultBannerImgShader { get; set; }
        
        [Display(Name = "Default Banner Text Color")]
        public string mdefaultBannerTextColor { get; set; }

        public HomePage()
        {
            defaultBannerImgShader = 0.0f;
            mdefaultBannerTextColor = "black";
            mEnableWelcomeImg = true;
        }
    }
}
