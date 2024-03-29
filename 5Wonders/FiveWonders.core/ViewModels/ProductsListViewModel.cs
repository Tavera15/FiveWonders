﻿using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.ViewModels
{
    public class ProductsListViewModel
    {
        public ProductData[] products { get; set; }
        public string pageTitle { get; set; }
        public string pageTitleColor { get; set; }
        public byte[] image { get; set; }
        public string imageType { get; set; }
        public float mImgShaderAmount { get; set; }
    }
}
