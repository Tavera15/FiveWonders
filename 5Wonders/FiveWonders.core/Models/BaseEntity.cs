using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public abstract class BaseEntity
    {
        [Key]
        [Display(Name = "ID")]
        public string mID { get; set; }

        [Display(Name = "Time Entered")]
        public DateTime mTimeEntered { get; private set; }

        public BaseEntity()
        {
            mID = Guid.NewGuid().ToString();
            mTimeEntered = DateTime.Now;
        }
    }
}
