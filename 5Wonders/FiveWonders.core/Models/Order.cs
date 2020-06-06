using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class Order : BaseEntity
    {
        public virtual ICollection<OrderItem> mOrderItems { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string mCustomerName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string mCustomerEmail { get; set; }

        [Required]
        [Display(Name = "Address Line 1")]
        public string mAddress1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string mAddress2 { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public string mZipCode { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string mPhoneNumber { get; set; }

        public Order()
        {
            mOrderItems = new List<OrderItem>();
        }
    }
}
