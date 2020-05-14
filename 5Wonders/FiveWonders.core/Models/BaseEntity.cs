using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public abstract class BaseEntity
    {
        public string mID { get; private set; }
        public DateTime mTimeEntered { get; private set; }

        public BaseEntity()
        {
            mID = Guid.NewGuid().ToString();
            mTimeEntered = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("ID: " + mID);
        }
    }
}
