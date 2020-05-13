using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{

    public class Product
    {
        public string mID { private set; get; }

        [Required(ErrorMessage = "A name is required")]
        [Display(Name = "Enter full name")]
        public string mName { get; set; }

        [Display(Name = "Enter a description")]
        public string mDesc { get; set; }

        [Required(ErrorMessage = "A price is required")]
        [Display(Name = "Enter a price")]
        public decimal mPrice { get; set; }

        public Product()
        {
            mID = Guid.NewGuid().ToString();
            System.Diagnostics.Debug.WriteLine("ID: " + mID);
        }
    }
}
