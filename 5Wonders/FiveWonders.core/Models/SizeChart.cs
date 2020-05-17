using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class SizeChart : BaseEntity
    {
        [Required(ErrorMessage = "A chart name is required")]
        [Display(Name = "Chart Name")]
        public string mChartName { get; set; }

        [Display(Name = "Sizes")]
        public Dictionary<int, List<string>> mChartEntries { get; set; }

        public SizeChart()
        {
            mChartEntries = new Dictionary<int, List<string>>();
        }
    }
}
