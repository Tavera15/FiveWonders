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
    public class SubCategory : BaseEntity
    {
        [Required(ErrorMessage = "A subcategory name is required")]
        [Display(Name = "Subcategory Name")]
        public string mSubCategoryName { get; set; }
        
        [Display(Name = "Is Event or Theme?", Description = "If checked, this subcategory will appear on the Event & Themes tab.")]
        public bool isEventOrTheme { get; set; }

        [Display(Name = "Image")]
        public string mImageUrl { get; set; }

        [Display(Name = "Banner Image Shade Amount")]
        [Range(0,1)]
        public float mImgShaderAmount { get; set; }

        [Display(Name = "Banner Text Color")]
        public string bannerTextColor { get; set; }

        public SubCategory()
        {
            mImgShaderAmount = 0.4f;
        }
    }

    public class SubcategoryValidator : AbstractValidator<SubCategory>
    {
        IRepository<SubCategory> subcategoryContext;

        public SubcategoryValidator(IRepository<SubCategory> subcategoryRepository, HttpPostedFileBase imgFile)
        {
            subcategoryContext = subcategoryRepository;

            RuleFor(subcategory => subcategory.mSubCategoryName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Sub-Category Name cannot be empty.")
                .Must((sub, subcategoryName) => IsUniqueName(subcategoryName, sub.mID))
                    .WithMessage("Sub-Category Name must be a unique name.");

            RuleFor(category => category.mImageUrl)
                .Cascade(CascadeMode.Stop)
                .Must((sub, mImageUrl) => willHaveImg(mImageUrl, imgFile))
                    .When(sub => sub.isEventOrTheme)
                    .WithMessage("Image is missing.");
        }

        private bool IsUniqueName(string mSubCategoryName, string mID = "")
        {
            SubCategory[] allSubcategories = subcategoryContext.GetCollection().ToArray();

            if (allSubcategories == null || allSubcategories.Length <= 0)
            {
                return true;
            }

            return !allSubcategories
                .Any(sub => sub.mSubCategoryName.ToLower() == mSubCategoryName.ToLower() 
                    && sub.mID != mID);
        }

        private bool willHaveImg(string mImgUrL, HttpPostedFileBase imgFile)
        {
            return !String.IsNullOrEmpty(mImgUrL) || imgFile != null;
        }
    }
}
