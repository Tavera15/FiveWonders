using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class OrderItemViewModel
    {
        public OrderItem orderItem { get; set; }
        public string[] productImages { get; set; }
        public string verificationId { get; set; }
    }
}
