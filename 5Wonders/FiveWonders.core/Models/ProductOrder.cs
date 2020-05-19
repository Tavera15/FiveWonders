using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class ProductOrder
    {
        public string mProductID { get; set; }

        [Range(1, Int32.MaxValue)]
        [Display(Name = "Quantity")]
        public int mQuantity { get; set; }

        [Display(Name = "Size")]
        public string mSize { get; set; }

        public ProductOrder()
        {
            mQuantity = 1;
            mSize = "";
        }
    }
}
