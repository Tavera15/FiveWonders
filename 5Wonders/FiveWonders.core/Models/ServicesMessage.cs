using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FiveWonders.core.Models
{
    public class ServicesMessage
    {
        [Required(ErrorMessage = "A name is required.")]
        [Display(Name = "Full Name")]
        public string mCustomerName { get; set; }

        [Required(ErrorMessage = "An email is required")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "A valid email is required.")]
        public string mEmail { get; set; }

        [Required(ErrorMessage = "A phone number is required")]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string mPhoneNumber { get; set; }

        [Required(ErrorMessage = "A message subject is required.")]
        [Display(Name = "Subject")]
        public string mSubject { get; set; }

        [Required(ErrorMessage = "A message body is required.")]
        [Display(Name = "Body")]
        [AllowHtml]
        public string mContent { get; set; }

        public ServicesMessage()
        {
        }
    }
}
