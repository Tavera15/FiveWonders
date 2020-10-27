using FiveWonders.DataAccess.InMemory;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiveWonders.core.Models
{
    public class SizeChart : BaseEntity
    {
        [Required(ErrorMessage = "A chart name is required")]
        [Display(Name = "Chart Name")]
        public string mChartName { get; set; }

        [Display(Name = "Image")]
        public string mImageChartUrl { get; set; }

        [Display(Name = "Sizes to Display")]
        public string mSizesToDisplay { get; set; }

        public readonly string[] ALL_AVAILABLE_SIZES = { "XXXS", "XXS", "XS", "S", "M", "L", "XL", "XXL", "XXXL" };

        public SizeChart()
        {
            mChartName = "";
            mImageChartUrl = "";
            mSizesToDisplay = "";
        }
    }

    public class SizeChartValidator : AbstractValidator<SizeChart>
    {
        IRepository<SizeChart> sizaChartContext;

        public SizeChartValidator(IRepository<SizeChart> sizeChartRepository, HttpPostedFileBase imgFile)
        {
            sizaChartContext = sizeChartRepository;

            RuleFor(sizeChart => sizeChart.mChartName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Size Chart Name cannot be empty")
                .Must((chart, chartName) => isUniqueName(chartName, chart.mID))
                    .WithMessage("Size Chart Name must be unique");

            RuleFor(sizeChart => sizeChart.mImageChartUrl)
                .Must((chart, imgUrl) => willHaveImg(imgUrl, imgFile))
                    .WithMessage("Image is missing");

            RuleFor(sizeChart => sizeChart.mSizesToDisplay)
                .NotEmpty()
                    .WithMessage("No sizes were selected");
        }
    
        private bool isUniqueName(string chartName, string Id)
        {
            SizeChart[] allCharts = sizaChartContext.GetCollection().ToArray();

            if(allCharts == null || allCharts.Length <= 0)
            {
                return true;
            }

            return !allCharts.Any(sc => sc.mChartName.ToLower() == chartName.ToLower() && sc.mID != Id);
        }

        private bool willHaveImg(string imgUrl, HttpPostedFileBase imgFile)
        {
            return !String.IsNullOrWhiteSpace(imgUrl) || imgFile != null;
        }
    }
}
