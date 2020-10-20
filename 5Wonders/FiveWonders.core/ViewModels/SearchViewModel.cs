using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class SearchViewModel
    {
        [Display(Name = "Product Name:")]
        public string productNameinput { get; set; }
        
        [Display(Name = "Category Name:")]
        public string categoryInput { get; set; }
        
        [Display(Name = "Sub-Category Names:")]
        public string[] subCategories { get; set; }


        public List<string> allCategories { get; set; }
        
        public List<string> allSubcategories { get; set; }
        
        public SearchViewModel()
        {
            productNameinput = "";
        }
    }
}
