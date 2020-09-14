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
        public string mWelcomeImgUrl { get; set; }
        
        [Display(Name = "Home Page Button Url")]
        public string mWelcomeBtnUrl { get; set; }

        [Display(Name = "Home Page Logo")]
        public string mHomePageLogoUrl { get; set; }

        [Display(Name = "Home Greeting Greeting")]
        [AllowHtml]
        [Required(ErrorMessage = "A Greeting is required.")]
        public string mHomePageGreeting { get; set; }





        [Required(ErrorMessage = "A Banner text is required for Product List.")]
        [Display(Name = "Default Banner Text")]
        public string mDefaultProductsBannerText { get; set; }

        [Display(Name = "Default Banner Image")]
        public string mDefaultProductListImgUrl { get; set; }
        
        [Display(Name = "Default Banner Image Shader")]
        [Range(0, 1)]
        public float defaultBannerImgShader { get; set; }
        
        [Display(Name = "Default Banner Text Color")]
        public string mdefaultBannerTextColor { get; set; }

        public HomePage()
        {
            defaultBannerImgShader = 0.4f;
            mdefaultBannerTextColor = "white";
        }
    }
}
