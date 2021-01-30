using FiveWonders.DataAccess.InMemory;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.core.Models
{
    public class Product : BaseEntity
    {
        [Required(ErrorMessage = "A name is required")]
        [Display(Name = "Name")]
        public string mName { get; set; }

        [Display(Name = "Is Visible?")]
        public bool isDisplayed { get; set; }

        [Display(Name = "Description")]
        [AllowHtml]
        public string mHtmlDesc { get; set; }

        [Display(Name = "Category")]
        public string mCategory { get; set; }

        [Display(Name = "Images")]
        public string mImageIDs { get; set; }

        [Required(ErrorMessage = "A price is required")]
        [Range(0.01, Double.PositiveInfinity, ErrorMessage = "Price must be greater than $0")]
        [Display(Name = "Price")]
        public decimal mPrice { get; set; }

        [Display(Name = "Sub-Categories")]
        public string mSubCategories { get; set; }

        [Display(Name = "Size Chart")]
        public string mSizeChart { get; set; }

        [Display(Name = "Is Text Customizable?")]
        public bool isTextCustomizable { get; set; }

        [Display(Name = "Custom Text Description")]
        public string mCustomText { get; set; }

        [Display(Name = "Is Number Customizable?")]
        public bool isNumberCustomizable { get; set; }

        [Display(Name = "Is Date Customizable")]
        public bool isDateCustomizable { get; set; }

        [Display(Name = "Is Time Customizable")]
        public bool isTimeCustomizable { get; set; }

        [Display(Name = "Custom Option Lists")]
        public string mCustomLists { get; set; }

        public Product()
        {
            isDisplayed = true;
        }
    }

    public class ProductsValidator : AbstractValidator<Product>
    {
        IRepository<Category> categoryContext;
        IRepository<SubCategory> subcategoryContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<CustomOptionList> customOptionListsContext;

        public ProductsValidator(IRepository<Category> categoryRepository, IRepository<SubCategory> subcategoryRepository, IRepository<SizeChart> sizachartRepository, IRepository<CustomOptionList> customListRepository,
            HttpPostedFileBase[] newImages)
        {
            categoryContext = categoryRepository;
            subcategoryContext = subcategoryRepository;
            sizeChartContext = sizachartRepository;
            customOptionListsContext = customListRepository;

            RuleFor(product => product.mCategory)
                .Cascade(CascadeMode.Stop)
                .Must((prod, categoryId) => categoryContext.Find(categoryId) != null)
                    .WithMessage("Invalid Category.");

            RuleFor(product => product.mSubCategories)
                .Cascade(CascadeMode.Stop)
                .Must((prod, subCats) => AreSubcategoriesValid(subCats))
                    .WithMessage("Invalid Subcategories.");

            RuleFor(product => product.mImageIDs)
                .Cascade(CascadeMode.Stop)
                .Must((prod, storedImages) => willHaveImg(storedImages, newImages))
                    .WithMessage("No images found.");

            RuleFor(product => product.mSizeChart)
                .Cascade(CascadeMode.Stop)
                .Must((prod, sizechartID) => String.IsNullOrWhiteSpace(sizechartID) || sizeChartContext.Find(sizechartID) != null)
                    .WithMessage("Invalid Size Chart.");

            RuleFor(product => product.mCustomLists)
                .Cascade(CascadeMode.Stop)
                .Must((product, customLists) => AreCustomListsValid(customLists))
                    .WithMessage("Invalid Custom Lists.");
        }

        private bool AreSubcategoriesValid(string subCats)
        {
            if(String.IsNullOrWhiteSpace(subCats))
            {
                return true;
            }

            var allSubCategories = subcategoryContext.GetCollection();
            return subCats.Split(',').All(selectedSubID => allSubCategories.Any(sub => sub.mID == selectedSubID));
        }

        private bool AreCustomListsValid(string customListIDs)
        {
            if (String.IsNullOrWhiteSpace(customListIDs))
            {
                return true;
            }

            var allCustomLists = customOptionListsContext.GetCollection();

            return customListIDs.Split(',')
                .All(selectedListID => allCustomLists.Any(customList => customList.mID == selectedListID));
        }

        private bool willHaveImg(string storedImages, HttpPostedFileBase[] imgFiles)
        {
            return !String.IsNullOrWhiteSpace(storedImages) || (imgFiles != null && imgFiles[0] != null);
        }
    }
}
