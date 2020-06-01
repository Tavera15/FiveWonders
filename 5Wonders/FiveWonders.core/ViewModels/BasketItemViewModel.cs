using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class BasketItemViewModel
    {
        public string basketItemID { get; set; }
        
        public string productID { get; set; }
        
        [Display(Name = "Product")]
        public string productName { get; set; }
        
        [Display(Name = "Quantity")]
        public int quantity { get; set; }
        
        [Display(Name = "Price")]
        public decimal price { get; set; }
        
        [Display(Name = "Size")]
        public string size { get; set; }

        public string image { get; set; }
        
        public string sizeChart { get; set; }

    }
}
