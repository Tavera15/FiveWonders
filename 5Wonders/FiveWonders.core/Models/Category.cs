using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class Category : BaseEntity
    {
        [Display(Name = "Category Name")]
        [Required(ErrorMessage = "A category name is required")]
        public string mCategoryName { get; set; }

        [Display(Name = "Image")]
        public string mImgUrL { get; set; }

        [Display(Name = "Banner Image Shade Amount")]
        [Range(0, 1)]
        public float mImgShaderAmount { get; set; }

        [Display(Name = "Banner Text Color")]
        public string bannerTextColor { get; set; }

        public Category()
        {
            mImgShaderAmount = 0.4f;
        }
    }
}

/*
    Balloons: Male Female Kids
    Clothing: Male Female Kids | XS | S | M | L | XL | XXL |
    Giveaway: Create GiveAway, Past GiveAways   ID, Name, Empty List of Customers, Winner, Price, End date
    
*/