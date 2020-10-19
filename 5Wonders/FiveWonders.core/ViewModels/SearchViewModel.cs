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
        public string categoryIdinput { get; set; }
        
        [Display(Name = "Sub-Category Name:")]
        public string[] subCategoryIdinput { get; set; }

        public string message { get; set; }
        
        public List<Product> results { get; set; }
        
        public List<Product> similarResults { get; set; }

        public Category[] allCategories { get; set; }
        
        public SubCategory[] allSubcategories { get; set; }
        
        
        public SearchViewModel()
        {
            results = new List<Product>();
            similarResults = new List<Product>();
            productNameinput = "";
        }
    }
}
