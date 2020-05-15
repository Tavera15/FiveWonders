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
    }
}
