﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class OrderItem : BaseEntity
    {
        public string mBaseOrderID { get; set; }
        public string mProductID { get; set; }
        public string mProductName { get; set; }
        public decimal mPrice { get; set; }
        public int mQuantity { get; set; }
        public string mSize { get; set; }
        public string mCustomText { get; set; }
        public string mCustomNumber { get; set; }
        public string mCustomDate { get; set; }
        public string mCustomTime { get; set; }
        public string mCustomListOpts { get; set; }
    }
}
