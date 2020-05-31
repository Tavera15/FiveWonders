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
        public int quantity { get; set; }
        public string productID { get; set; }
        public decimal price { get; set; }
        public string image { get; set; }
        public string size { get; set; }

        [Display(Name = "Product")]
        public string productName { get; set; }
        public string sizeChart { get; set; }

    }
}
