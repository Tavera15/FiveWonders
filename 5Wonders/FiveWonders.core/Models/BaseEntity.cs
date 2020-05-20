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
        public string mID { get; set; }
        public DateTime mTimeEntered { get; private set; }

        public BaseEntity()
        {
            mID = Guid.NewGuid().ToString();
            mTimeEntered = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("ID: " + mID);
        }
    }
}
