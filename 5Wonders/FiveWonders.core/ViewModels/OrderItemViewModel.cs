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
        public FWonderOrder order { get; set; }
        public OrderItem orderItem { get; set; }
        public ProductImage[] productImages { get; set; }
        public Dictionary<string, string> customLists { get; set; }
    }
}
