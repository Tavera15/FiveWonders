using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class FWonderOrder : BaseEntity
    {
        public virtual ICollection<OrderItem> mOrderItems { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string mCustomerName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string mCustomerEmail { get; set; }

        public string paypalRef { get; set; }

        public FWonderOrder()
        {
            mOrderItems = new List<OrderItem>();
        }
    }
}
