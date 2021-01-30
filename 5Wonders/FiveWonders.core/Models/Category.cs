using FiveWonders.DataAccess.InMemory;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace FiveWonders.core.Models
{
    public class Category : BaseEntity
    {
        [Display(Name = "Category Name")]
        [Required(ErrorMessage = "A category name is required")]
        public string mCategoryName { get; set; }

        [Display(Name = "Image")]
        public byte[] mImage { get; set; }

        [Display(Name = "Banner Image Shade Amount")]
        [Range(0, 1)]
        public float mImgShaderAmount { get; set; }

        [Display(Name = "Banner Text Color")]
        public string bannerTextColor { get; set; }

        public string mImageType { get; set; }

        public Category()
        {
            mImgShaderAmount = 0.4f;
        }

    }

    public class CategoryValidator : AbstractValidator<Category>
    {
        IRepository<Category> categoryContext;

        public CategoryValidator(IRepository<Category> categoryRepository, HttpPostedFileBase imgFile)
        {
            categoryContext = categoryRepository;

            RuleFor(category => category.mCategoryName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Category Name cannot be empty.")
                .Must((cat, categoryName) => IsUniqueName(categoryName, cat.mID))
                    .WithMessage("Category Name must be a unique name.");

            RuleFor(category => category.mImage)
                .Cascade(CascadeMode.Stop)
                .Must((cat, imageUrl) => willHaveImg(cat.mImage, imgFile))
                    .WithMessage("Image is missing.");
        }

        private bool IsUniqueName(string mCategoryName, string mID = "")
        {
            Category[] allCategories = categoryContext.GetCollection().ToArray();

            if(allCategories == null || allCategories.Length <= 0)
            {
                return true;
            }

            return !allCategories.Any(cat => cat.mCategoryName.ToLower() == mCategoryName.ToLower() && cat.mID != mID);
        }

        private bool willHaveImg(byte[] storedImg, HttpPostedFileBase imgFile)
        {
            return storedImg != null || imgFile != null;
        }
    }
}
