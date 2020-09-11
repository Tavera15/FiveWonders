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
 
        [Range(0, 1)]
        public float mImgShaderAmount { get; set; }

        public Category()
        {
        }
    }
}

/*
    Balloons: Male Female Kids
    Clothing: Male Female Kids | XS | S | M | L | XL | XXL |
    Giveaway: Create GiveAway, Past GiveAways   ID, Name, Empty List of Customers, Winner, Price, End date
    
*/