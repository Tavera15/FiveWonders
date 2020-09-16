﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FiveWonders.core.Models
{
    public class ServicePage : BaseEntity
    {
        [AllowHtml]
        [Display(Name = "Banner Message")]
        public string mBannerMessage { get; set; }

        [Required]
        [Display(Name = "Owner Name")]
        public string mName { get; set; }
        
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string mPhoneNumber { get; set; }
        
        [EmailAddress]
        [Display(Name = "Email")]
        public string mEmail { get; set; }
        
        // Todo Remove these Social Medias and put the new version.
        // Also, only list the cummunicated ones.

        [Display(Name = "Facebook URL")]
        public string mFacebookUrl { get; set; }
        
        [Display(Name = "Instagram URL")]
        public string mInstagramUrl { get; set; }
    }
}
