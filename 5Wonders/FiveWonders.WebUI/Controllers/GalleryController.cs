using FiveWonders.core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FiveWonders.WebUI.Controllers
{
    public class GalleryController : Controller
    {
        // GET: Gallery
        public async Task<ActionResult> Index()
        {
            const string URL = "https://www.instagram.com/5wondersballoons/?__a=1";
            var InstagramPosts = new List<InstagramPost>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                HttpResponseMessage response = await client.GetAsync(URL);
                
                if(response.IsSuccessStatusCode)
                {
                    HttpContent reqContent = response.Content;
                    string strContent = reqContent.ReadAsStringAsync().Result;

                    dynamic jsonContent = JValue.Parse(strContent);
                    JArray IGImages = jsonContent.graphql.user.edge_owner_to_timeline_media.edges;

                    foreach(var x in IGImages)
                    {
                        
                        if(x.SelectToken("node").SelectToken("edge_sidecar_to_children") != null)
                        {
                            var childImages = x.SelectToken("node").SelectToken("edge_sidecar_to_children").SelectToken("edges");

                            foreach(var e in childImages as JArray)
                            {
                                var imageURL = e.SelectToken("node").SelectToken("display_url");
                                InstagramPost post = new InstagramPost();
                                post.mImageURL = imageURL.ToString();

                                InstagramPosts.Add(post);
                            }
                        }
                        else
                        {
                            var imageURL = x.SelectToken("node").SelectToken("display_url");
                            InstagramPost post = new InstagramPost();
                            post.mImageURL = imageURL.ToString();

                            InstagramPosts.Add(post);
                        }
                    }
                }
            }

            return View(InstagramPosts);
        }
    }
}