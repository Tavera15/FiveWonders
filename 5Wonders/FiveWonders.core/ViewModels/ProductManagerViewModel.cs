using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiveWonders.core.Models;

namespace FiveWonders.core.ViewModels
{
    public class ProductManagerViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Category> categories { get; set; }
        public IEnumerable<SubCategory> subCategories { get; set; }
        public IEnumerable<CustomOptionList> customOptionLists { get; set; }
        public List<SizeChart> sizeCharts { get; set; }
    }
}
