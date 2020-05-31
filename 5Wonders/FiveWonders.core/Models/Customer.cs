using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class Customer : BaseEntity
    {
        public string mUserID { get; set; }

        public string mFullName { get; set; }

        public string mEmail { get; set; }

        public string mBasketID { get; set; }
    }
}
