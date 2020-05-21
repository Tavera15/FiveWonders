using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class Product : BaseEntity
    {
        [Required(ErrorMessage = "A name is required")]
        [Display(Name = "Name")]
        public string mName { get; set; }

        [Display(Name = "Description")]
        public string mDesc { get; set; }

        [Display(Name = "Category")]
        public string mCategory { get; set; }

        [Display(Name = "Image")]
        public string mImage { get; set; }

        [Required(ErrorMessage = "A price is required")]
        [Range(0.01, Double.PositiveInfinity, ErrorMessage = "Price must be greater than $0")]
        [Display(Name = "Price")]
        public decimal mPrice { get; set; }

        [Display(Name = "Sub-Categories")]
        public string mSubCategories { get; set; }

        [Display(Name = "Size Chart")]
        public string mSizeChart { get; set; }

        public Product()
        {
        }
    }
}
