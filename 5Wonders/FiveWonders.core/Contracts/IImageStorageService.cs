using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiveWonders.core.Contracts
{
    // Todo implement this to all files

    public enum EFolderName { Home, Category, Subcategory, Gallery, Products, SizeCharts, Icons };
    
    public interface IImageStorageService
    {
        void AddImage(EFolderName folder, HttpServerUtilityBase Server, HttpPostedFileBase imageFile, string Id, out string newImageUrl, string manualFileName = "");
        void DeleteImage(EFolderName folder, string currentImageURL, HttpServerUtilityBase Server);
        void AddMultipleImages(EFolderName folder, HttpServerUtilityBase Server, HttpPostedFileBase[] imageFiles, string Id, out string newImageUrl);
        void DeleteMultipleImages(EFolderName folder, string[] currentImageFiles, HttpServerUtilityBase Server);
        void UpdateImages(HttpServerUtilityBase Server, EFolderName folder, string[] savedCarouselImgs, string[] checkedImgs, HttpPostedFileBase[] newImageFiles, out string newImageURL, string fileNamePrefix);
    }
}
