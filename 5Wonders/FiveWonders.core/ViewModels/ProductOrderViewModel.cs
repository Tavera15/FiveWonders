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
        public ProductOrder productOrder { get; set; }
        public SizeChart sizeChart { get; set; }


        private static readonly int[] mAllQuantities = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        public ProductOrderViewModel()
        {
            productOrder = new ProductOrder();
        }

        public int[] GetAllQuantites()
        {
            return mAllQuantities;
        }
    }
}
