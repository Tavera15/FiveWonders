using Newtonsoft.Json;
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

        [Display(Name = "Custom Text")]
        public string mCustomText { get; set; }

        [Display(Name = "Custom Number")]
        [MaxLength(99)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Number must be numeric")]
        public string mCustomNum { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Custom Date")]
        public string customDate { get; set; }

        //[RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid Time.")]
        [RegularExpression(@"^([0]?[0-9]|1[0-2]):[0-5][0-9]$", ErrorMessage = "Invalid Time.")]
        public string customTime { get; set; }

        [Display(Name = "Custom List Options")]
        public string mCustomListOptions { get; set; }

        public BasketItem()
        {
            mQuantity = 1;
            mSize = "";
            mCustomText = "";
            mCustomNum = "";
            customTime = "";
            customDate = "";
        }
    }
}
