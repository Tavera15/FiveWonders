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

        [Required(ErrorMessage = "A price is required")]
        [Display(Name = "Price")]
        public decimal mPrice { get; set; }

        [Display(Name = "Sub-Categories")]
        public List<string> mSubCategories { get; set; }

        public Product()
        {
            mSubCategories = new List<string>();
        }
    }
}
