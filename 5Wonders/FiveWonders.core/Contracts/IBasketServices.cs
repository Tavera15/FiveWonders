using FiveWonders.core.Models;
using FiveWonders.core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiveWonders.core.Contracts
{
    public interface IBasketServices
    {
        void AddToBasket(HttpContextBase httpContext, BasketItem item);
        void RemoveFromBasket(HttpContextBase httpContext, string itemToRemoveID);
        List<BasketItemViewModel> GetBasketItems(HttpContextBase httpContext);
        void UpdateBasketItem(HttpContextBase httpContext, BasketItem newBasketItem);
        bool IsItemInUserBasket(HttpContextBase httpContext, string basketItemID, out BasketItem basketItem);
    }
}
