using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class Category
    {
        public string mID { get; private set; }

        [Required(ErrorMessage = "A category name is required")]
        public string mCategoryName { get; set; }

        public Category()
        {
            mID = Guid.NewGuid().ToString();
        }
    }
}

/*
    Balloons: Male Female Kids
    Clothing: Male Female Kids | XS | S | M | L | XL | XXL |
    Giveaway: Create GiveAway, Past GiveAways   ID, Name, Empty List of Customers, Winner, Price, End date
    
*/