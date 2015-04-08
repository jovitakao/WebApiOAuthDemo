﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WebApiOAuthDemo.Core.ApiSettings;
using WebApiOAuthDemo.Core.Helpers;
using WebApiOAuthDemo.Models.ViewModels;

namespace WebApiOAuthDemo.Controllers
{
    public class FacebookController : FacebookBaseController
    {
        public ActionResult Index()
        {
            if (String.IsNullOrEmpty(fbAccessToken))
            {
                // 向Facebook取得code
                return new RedirectResult(String.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&scope={2}",
                    apiSettings.ClientId, redirectUrl, "user_photos"));
            }
            else
            {
                return RedirectToAction("MyPhotos");
            }
        }

        public override ActionResult AuthReturn()
        {
            // 將code, client_id, client_secret和redirect_uri參數向facebook取得access_token
            // redirect_uri必須和向Facebook取得code所傳入的一樣
            string code = Request["code"];

            // v2.3 access_token會以json格式回傳, 不指定版本時回傳為key1=value1&key2=value2格式
            string url = @"https://graph.facebook.com/v2.3/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";
            WebClient client = new WebClient();
            string returnJsonString = client.DownloadString(String.Format(url, apiSettings.ClientId, redirectUrl, apiSettings.Secret, code));

            string returnAccessToken = (JsonConvert.DeserializeObject(returnJsonString) as JObject)["access_token"].ToString();

            fbAccessToken = returnAccessToken;

            return RedirectToAction("MyPhotos");
        }

        public ActionResult MyPhotos()
        {
            WebClient client = new WebClient();
            string graphUrlMyPhotos = @"https://graph.facebook.com/me?fields=photos&access_token={0}";
            string myPhotosJsonString = client.DownloadString(String.Format(graphUrlMyPhotos, fbAccessToken));
            JObject myPhotos = (JsonConvert.DeserializeObject(myPhotosJsonString) as JObject);

            List<FacebookPhoto> photos = new List<FacebookPhoto>();
            foreach (JObject photo in myPhotos["photos"]["data"])
            {
                photos.Add(new FacebookPhoto()
                {
                    Name = photo["name"] == null ? "" : photo["name"].ToString(),
                    Link = photo["link"].ToString(),
                    Picture = photo["picture"].ToString()
                });
            }

            return View(photos);
        }
    }
}