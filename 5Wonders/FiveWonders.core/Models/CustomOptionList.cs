using FiveWonders.DataAccess.InMemory;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class CustomOptionList : BaseEntity
    {
        [Display(Name = "List Name")]
        [Required(ErrorMessage = "A unique name is required.")]
        public string mName { get; set; }

        [Display(Name = "List Items")]
        [Required(ErrorMessage = "List Items are required")]
        public string options { get; set; }
    }

    public class CustomListValidator : AbstractValidator<CustomOptionList>
    {
        IRepository<CustomOptionList> customListContext;

        public CustomListValidator(IRepository<CustomOptionList> customListRepository)
        {
            customListContext = customListRepository;

            RuleFor(cList => cList.mName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Custom List Name cannot be empty.")
                .Must((cl, categoryName) => IsUniqueName(categoryName, cl.mID))
                    .WithMessage("Custom List Name must be a unique name.");

            RuleFor(cList => cList.options)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Options List cannot be empty.");
        }

        private bool IsUniqueName(string mCategoryName, string mID = "")
        {
            CustomOptionList[] allLists = customListContext.GetCollection().ToArray();

            if (allLists == null || allLists.Length <= 0)
            {
                return true;
            }

            return !allLists.Any(cl => cl.mName.ToLower() == mCategoryName.ToLower() && cl.mID != mID);
        }
    }
}
