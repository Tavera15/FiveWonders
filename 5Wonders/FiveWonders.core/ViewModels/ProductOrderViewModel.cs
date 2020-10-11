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
        public Dictionary<string,List<string>> listOptions { get; set; }
        public List<string> customListNames { get; set; }
        public Dictionary<string, string> selectedCustomListOptions { get; set; }


        public ProductOrderViewModel()
        {
            productOrder = new BasketItem();
            listOptions = new Dictionary<string, List<string>>();
            customListNames = new List<string>();
            selectedCustomListOptions = new Dictionary<string, string>();
        }
    }
}
