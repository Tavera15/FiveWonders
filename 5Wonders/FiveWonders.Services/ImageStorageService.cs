using FiveWonders.core.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiveWonders.Services
{
    public class ImageStorageService : IImageStorageService
    {
        public void AddImage(EFolderName folder, HttpServerUtilityBase Server, HttpPostedFileBase imageFile, string Id, out string newImageUrl, string manualFileName = "")
        {
            string folderName = GetFolderName(folder);

            newImageUrl = String.IsNullOrWhiteSpace(manualFileName) 
                ? Id + String.Concat(imageFile.FileName.Where(c => !Char.IsWhiteSpace(c)))      // Id + file name without spaces
                : manualFileName + Path.GetExtension(imageFile.FileName);

            imageFile.SaveAs(Server.MapPath("//Content//" + folderName + "//") + newImageUrl);
        }

        public void DeleteImage(EFolderName folder, string currentImageURL, HttpServerUtilityBase Server)
        {
            string folderName = GetFolderName(folder);
            string path = Server.MapPath("//Content//" + folderName + "//") + currentImageURL;

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        public void AddMultipleImages(EFolderName folder, HttpServerUtilityBase Server, HttpPostedFileBase[] imageFiles, string Id, out string newImageUrl)
        {
            var allFileNames = new List<string>();

            foreach (HttpPostedFileBase file in imageFiles)
            {
                string singleImageUrl;
                AddImage(folder, Server, file, Id, out singleImageUrl);
                allFileNames.Add(singleImageUrl);
            }

            newImageUrl = String.Join(",", allFileNames);
        }

        public void DeleteMultipleImages(EFolderName folder, string[] currentImageFiles, HttpServerUtilityBase Server)
        {
            foreach (string file in currentImageFiles)
            {
                DeleteImage(folder, file, Server);
            }
        }

        private string GetFolderName(EFolderName folder)
        {
            switch(folder)
            {
                case EFolderName.Products:
                    return "ProductImages";
                case EFolderName.Home:
                    return "Home";
                case EFolderName.Category:
                    return "CategoryImages";
                case EFolderName.Subcategory:
                    return "SubcategoryImages";
                case EFolderName.SizeCharts:
                    return "SizeCharts";
                case EFolderName.Icons:
                    return "Icons";
                case EFolderName.Gallery:
                    return "GalleryImages";
                default:
                    throw new Exception("Not a valid folder name");
            }
        }
    }
}
