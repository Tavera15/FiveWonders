using FiveWonders.DataAccess.InMemory;
using FluentValidation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Models
{
    public class BasketItem : BaseEntity
    {
        public string mProductID { get; set; }
        public string basketID { get; set; }

        [Range(1, Int32.MaxValue)]
        [Display(Name = "Quantity")]
        public int mQuantity { get; set; }

        [Display(Name = "Size")]
        public string mSize { get; set; }

        [Display(Name = "Custom Text")]
        public string mCustomText { get; set; }

        [Display(Name = "Custom Number")]
        [MaxLength(99)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Number must be numeric")]
        public string mCustomNum { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Custom Date")]
        public string customDate { get; set; }

        // Top is 0-24
        //[RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid Time.")]
        [RegularExpression(@"^([0]?[0-9]|1[0-2]):[0-5][0-9]$", ErrorMessage = "Invalid Time.")]
        public string customTime { get; set; }


        // This is serialized: Dictionary <string (list id), string (choice)>
        [Display(Name = "Custom List Options")]
        public string mCustomListOptions { get; set; }

        public BasketItem()
        {
            mQuantity = 1;
            mSize = "";
            mCustomText = "";
            mCustomNum = "";
            customTime = "";
            customDate = "";
        }
    }

    public class BasketItemValidator : AbstractValidator<BasketItem>
    {
        IRepository<Product> productsContext;
        IRepository<SizeChart> sizeChartContext;
        IRepository<CustomOptionList> customListContext;

        public BasketItemValidator(IRepository<Product> productsRepository, IRepository<SizeChart> sizeChartRepository, IRepository<CustomOptionList> customListRepository, Dictionary<string, string> deserializedList)
        {
            productsContext = productsRepository;
            sizeChartContext = sizeChartRepository;
            customListContext = customListRepository;

            // Check for Quantity
            RuleFor(basketItem => basketItem.mQuantity)
                .GreaterThan(0)
                    .WithMessage("Quantity must be greater than 0.");

            // Check for Size
            RuleFor(basketItem => basketItem.mSize)
                .Must((item, size) => isValidSize(size, item.mProductID))
                    .WithMessage("Not a valid size");

            // Check for Custom List
            RuleFor(basketItem => basketItem.mCustomListOptions)
                .Must((item, serializedList) => bAreListOptionsValid(deserializedList, item))
                    .WithMessage("Custom options not valid. Try again.");

            // Check for Custom Text
            RuleFor(basketItem => basketItem.mCustomText)
                .Must((item, cText) => isCustomTextValid(cText, item))
                    .WithMessage("Custom Text is not valid. Try Again.");

            // Check for Custom Time
            RuleFor(basketItem => basketItem.customTime)
                .Must((item, cTime) => isCustomTimeValid(cTime, item))
                    .WithMessage("Custom Time is not valid. Try again.");

            // Check for Custom Number
            RuleFor(basketItem => basketItem.mCustomNum)
                .Must((item, cNum) => isCustomNumValid(cNum, item))
                    .WithMessage("Custom Number is not valid. Try again.");

            // Check for Custom Date
            RuleFor(basketItem => basketItem.customDate)
                .Must((item, cDate) => isCustomDateValid(cDate, item))
                    .WithMessage("Custom Date is not valid. Try again.");
        }

        private bool isCustomDateValid(string customDate, BasketItem item)
        {
            try
            {
                Product product = productsContext.Find(item.mProductID, true);

                if(String.IsNullOrWhiteSpace(customDate)) { return true; }

                return product.isDateCustomizable;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private bool isCustomNumValid(string customNumber, BasketItem item)
        {
            try
            {
                Product product = productsContext.Find(item.mProductID, true);

                if (String.IsNullOrWhiteSpace(customNumber)) { return true; }

                int parseCustomNumber;

                // True if product is number customizable, input is a digit, and between 0-100
                return (product.isNumberCustomizable
                    && int.TryParse(customNumber, out parseCustomNumber) 
                    && (parseCustomNumber >= 0 && parseCustomNumber < 100));
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private bool isCustomTimeValid(string customTime, BasketItem item)
        {
            try
            {
                Product product = productsContext.Find(item.mProductID, true);

                if (String.IsNullOrWhiteSpace(customTime)) { return true; }

                return product.isTimeCustomizable;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private bool isCustomTextValid(string customText, BasketItem item)
        {
            try
            {
                Product product = productsContext.Find(item.mProductID, true);

                if(String.IsNullOrWhiteSpace(customText)) { return true; }

                return product.isTextCustomizable;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private bool bAreListOptionsValid(Dictionary<string, string> deserializedList, BasketItem item)
        {
            try
            {
                Product product = productsContext.Find(item.mProductID, true);

                if (!String.IsNullOrWhiteSpace(product.mCustomLists) && (deserializedList != null && deserializedList.Count > 0))
                {
                    string[] productListIds = product.mCustomLists.Split(',');
                    bool bInputContainsAllLists = productListIds.All(listId => deserializedList.ContainsKey(listId))
                        && deserializedList.All(cList => productListIds.Contains(cList.Key));

                    if(!bInputContainsAllLists)
                    {
                        return false;
                    }

                    foreach (string listId in productListIds)
                    {
                        CustomOptionList customList = customListContext.Find(listId, true);

                        if (!customList.options.Split(',').Contains(deserializedList[listId]))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return (String.IsNullOrWhiteSpace(product.mCustomLists) && (deserializedList == null || deserializedList.Count <= 0));
            }
            catch(Exception e)
            {
                return false;
            }
            
        }

        private bool isValidSize(string selectedSize, string productId)
        {
            try
            {
                Product product = productsContext.Find(productId, true);

                if(String.IsNullOrWhiteSpace(selectedSize) && product.mSizeChart == "0")
                {
                    return true;
                }

                SizeChart sizeChart = sizeChartContext.Find(product.mSizeChart, true);
                string[] sizes = sizeChart.mSizesToDisplay.Split(',');

                if(sizes == null || sizes.Length <= 0)
                {
                    throw new Exception("No sizes were found");
                }

                return (!String.IsNullOrWhiteSpace(selectedSize) && sizes.Contains(selectedSize));
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}
// https://i.ytimg.com/vi/Mc_xhKEZMN4/maxresdefault.jpg