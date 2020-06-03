using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class OrderItem
    {
        public string mBaseOrderID { get; set; }
        public string mProductID { get; set; }
        public decimal mPrice { get; set; }
        public string mProductName { get; set; }
    }
}
