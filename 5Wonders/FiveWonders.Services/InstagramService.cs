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

        const string URL = "https://www.instagram.com/5wondersballoons/?__a=1";
        
        public async Task<List<InstagramPost>> GetIGMediaAsync()
        {
            var InstagramPosts = new List<InstagramPost>();
    
            using (HttpClient client = new HttpClient())
            {
                // Setup the IG API and grab the JSON data from URL
                client.BaseAddress = new Uri(URL);
                HttpResponseMessage response = await client.GetAsync(URL);

                if (response.IsSuccessStatusCode)
                {
                    // Grab the content from the response
                    HttpContent reqContent = response.Content;
                    string strContent = reqContent.ReadAsStringAsync().Result;

                    // Convert the content from string into JSON in order to extract data
                    dynamic jsonContent = JValue.Parse(strContent);

                    // Target the IG media posts - Includes images, video thumbnails, captions, etc
                    JArray IGImages = jsonContent.graphql.user.edge_owner_to_timeline_media.edges;

                    foreach (JToken x in IGImages)
                    {
                        // Gets images from posts that have "swipe" media - Max should be 9 or 10
                        if (x.SelectToken("node").SelectToken("edge_sidecar_to_children") != null)
                        {
                            // Grab all the swipe images from the posts
                            JToken childImages = x.SelectToken("node").SelectToken("edge_sidecar_to_children").SelectToken("edges");

                            // Add each image into our array
                            foreach (JToken e in childImages as JArray)
                            {
                                JToken imageURL = e.SelectToken("node").SelectToken("display_url");

                                if (imageURL == null)
                                    throw new Exception("Instagram Service found no post");

                                InstagramPost post = new InstagramPost();
                                post.mImageURL = imageURL.ToString();

                                InstagramPosts.Add(post);
                            }
                        }
                        // Gets images that contain only one single picture/video thumbnail on a post
                        else
                        {
                            // Add to our array
                            JToken imageURL = x.SelectToken("node").SelectToken("display_url");
                            InstagramPost post = new InstagramPost();
                            post.mImageURL = imageURL.ToString();

                            InstagramPosts.Add(post);
                        }
                    }
                }
            }

            return InstagramPosts;
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
