using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.core.Contracts
{
    public interface IInstagramService
    {
        Task<List<InstagramPost>> GetIGMediaAsync();
        GalleryImg[] GetGalleryImgs();
    }
}
