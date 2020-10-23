using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class SocialMedia : BaseEntity
    {
        [Required(ErrorMessage = "A Url to your Social Media Acount is required.")]
        [Display(Name = "URL")]
        public string mUrl { get; set; }

        [Display(Name = "Icon")]
        public string m64x64Icon { get; set; }

        public bool isCommunicative { get; set; }

        public SocialMedia()
        {
            isCommunicative = true;
        }
    }
}
