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

        [Required(ErrorMessage = "An image of a size chart is required")]
        [Display(Name = "Image")]
        public string mImageChartUrl { get; set; }

        [Required(ErrorMessage = "At Least one size letter is required")]
        [Display(Name = "Sizes to Display")]
        public List<string> mSizesToDisplay { get; set; }


        public static readonly string[] ALL_AVAILABLE_SIZES = { "XXXS", "XXS", "XS", "S", "M", "L", "XL", "XXL", "XXXL" };

        public SizeChart()
        {
            mSizesToDisplay = new List<string>();
        }

        public string[] GetAllAvailableSizes()
        {
            return ALL_AVAILABLE_SIZES;
        }
    }
}
