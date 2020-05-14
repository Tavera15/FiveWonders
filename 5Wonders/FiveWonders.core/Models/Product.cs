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
        [Display(Name = "Enter full name")]
        public string mName { get; set; }

        [Display(Name = "Enter a description")]
        public string mDesc { get; set; }

        [Display(Name = "Category")]
        public string mCategory { get; set; }

        [Required(ErrorMessage = "A price is required")]
        [Display(Name = "Enter a price")]
        public decimal mPrice { get; set; }

        public Product()
        {

        }
    }
}
