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

        [Required(ErrorMessage = "A name is required.")]
        [Display(Name = "Full Name")]
        public string mCustomerName { get; set; }

        [Required(ErrorMessage = "An Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email address.")]
        [Display(Name = "Email")]
        public string mCustomerEmail { get; set; }

        public bool isCompleted { get; set; }

        public string mVerificationId { get; set; }

        public string mCustomerId { get; set; }

        public FWonderOrder()
        {
            mOrderItems = new List<OrderItem>();
            isCompleted = false;
            mCustomerId = "";
        }
    }
}
