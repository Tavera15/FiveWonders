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
}
