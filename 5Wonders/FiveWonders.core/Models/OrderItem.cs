using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class OrderItem : BaseEntity
    {
        [Display(Name = "Base Order ID")]
        public string mBaseOrderID { get; set; }

        [Display(Name = "Product ID")]
        public string mProductID { get; set; }

        [Display(Name = "Product Name")]
        public string mProductName { get; set; }

        [Display(Name = "Product Unit Price")]
        public decimal mPrice { get; set; }

        [Display(Name = "Quantity")]
        public int mQuantity { get; set; }

        [Display(Name = "Size")]
        public string mSize { get; set; }

        [Display(Name = "Custom Text")]
        public string mCustomText { get; set; }

        [Display(Name = "Custom Number")]
        public string mCustomNumber { get; set; }

        [Display(Name = "Custom Date")]
        public string mCustomDate { get; set; }

        [Display(Name = "Custom Time")]
        public string mCustomTime { get; set; }

        [Display(Name = "Additional Customization Options")]
        public string mCustomListOpts { get; set; }
    }
}
