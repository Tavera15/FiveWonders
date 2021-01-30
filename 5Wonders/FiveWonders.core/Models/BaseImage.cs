using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public abstract class BaseImage : BaseEntity
    {
        public byte[] mImage { get; set; }
        public string mImageType { get; set; }
    }
}
