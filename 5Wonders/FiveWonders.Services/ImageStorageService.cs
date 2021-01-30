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
        public static string GetImageExtension(HttpPostedFileBase imageFile)
        {
            string extension = Path.GetExtension(imageFile.FileName).ToLower();

            switch (extension)
            {
                case ".png":
                    return "data:image/png;base64,";
                case ".gif":
                    return "data:image/gif;base64,";
                case ".svg":
                    return "data:image/svg+xml;base64,";
                case ".jpg":
                    return "data:image/jpg;base64,";
                default:
                    return "data:image/jpeg;base64,";
            }
        }

        public static byte[] GetImageBytes(HttpPostedFileBase imageFile)
        {
            byte[] imageBytes = new byte[imageFile.ContentLength];
            imageFile.InputStream.Read(imageBytes, 0, imageFile.ContentLength);

            return imageBytes;
        }

        public static byte[][] GetMultipleImageBytes(HttpPostedFileBase[] imageFiles, out string[] imageExtensions)
        {
            if(imageFiles == null || imageFiles.Length == 0)
            {
                imageExtensions = null;
                return null;
            }

            List<byte[]> imagesToBytes = new List<byte[]>();
            List<string> imageTypes = new List<string>();

            foreach(var img in imageFiles)
            {
                imagesToBytes.Add(GetImageBytes(img));
                imageTypes.Add(GetImageExtension(img));
            }

            imageExtensions = imageTypes.ToArray();
            return imagesToBytes.ToArray();
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
