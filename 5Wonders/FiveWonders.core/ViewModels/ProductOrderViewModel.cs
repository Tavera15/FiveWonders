using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class ProductOrderViewModel
    {
        public Product product { get; set; }
        public BasketItem productOrder { get; set; }
        public SizeChart sizeChart { get; set; }

        public ProductOrderViewModel()
        {
            productOrder = new BasketItem();
        }
    }
}
