using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.Services
{
    public class InstagramService : IInstagramService
    {
        IRepository<GalleryImg> galleryContext;

        public InstagramService(IRepository<GalleryImg> galleryRepository)
        {
            galleryContext = galleryRepository;
        }

        public GalleryImg[] GetGalleryImgs()
        {
            try
            {
                return galleryContext.GetCollection().OrderByDescending(x => x.mTimeEntered).ToArray();
            }
            catch(Exception e)
            {
                _ = e;
                return new GalleryImg[] { };
            }
        }
    }
}
