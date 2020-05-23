using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class SubCategory : BaseEntity
    {
        [Required(ErrorMessage = "A subcategory name is required")]
        [Display(Name = "Subcategory Name")]
        public string mSubCategoryName { get; set; }

        
        [Display(Name = "Is Event or Theme?", Description = "If checked, this subcategory will appear on the Event & Themes tab.")]
        public bool isEventOrTheme { get; set; }
    }
}
