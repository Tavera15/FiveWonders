using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class Basket : BaseEntity
    {
        public virtual ICollection<BasketItem> mBasket { get; set; }
    
        public Basket()
        {
            mBasket = new List<BasketItem>();
        }
    }
}
