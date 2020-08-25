using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class BasketItem : BaseEntity
    {
        public string mProductID { get; set; }
        public string basketID { get; set; }

        [Range(1, Int32.MaxValue)]
        [Display(Name = "Quantity")]
        public int mQuantity { get; set; }

        [Display(Name = "Size")]
        public string mSize { get; set; }

        [Display(Name = "Product Text")]
        public string mProductText { get; set; }

        [Display(Name = "Custom Number")]
        [MaxLength(99)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Number must be numeric")]
        public string mCustomNum { get; set; }

        public BasketItem()
        {
            mQuantity = 1;
            mSize = "";
            mProductText = "";
            mCustomNum = "";
        }
    }
}
