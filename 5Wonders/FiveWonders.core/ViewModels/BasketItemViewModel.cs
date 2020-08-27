﻿using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class BasketItemViewModel
    {
        public string basketItemID { get; set; }
        public string productID { get; set; }
        public Product product { get; set; }
        public BasketItem basketItem { get; set; }
        public SizeChart sizeChart { get; set; }
    }
}
