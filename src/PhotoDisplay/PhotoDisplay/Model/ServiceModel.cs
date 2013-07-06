using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Windows.Phone.System.UserProfile;

namespace MVVM.Models
{
    public class ServiceModel
    {
        public event EventHandler<GenericEventArgs<ImagesBrand>> GetImagesCompleted;






        public void GetImagesBrands(String Seach)
        {
            
            string uri = String.Format("http://www.degraeve.com/flickr-rss/rss.php?tags={0}&tagmode=all&sort=interestingness-desc&num=9", Seach);

            var request = HttpWebRequest.Create(new Uri(uri, UriKind.Absolute));
            request.Method = "GET";
            request.BeginGetResponse((cb) =>
            {
                var response = request.EndGetResponse(cb);
                var stream = response.GetResponseStream();
                var result = new StreamReader(stream).ReadToEnd();

                var doc = XDocument.Parse(result);

                 XNamespace ns = "http://search.yahoo.com/mrss/";

                      
                 var query = from c in doc.Descendants(ns + "content").Take(9)
                             select new ImagesBrand() { Name = c.Attribute("url").Value ,uri= new Uri(c.Attribute("url").Value, UriKind.RelativeOrAbsolute) };

 

                var results = query.ToList();

                if (GetImagesCompleted != null)
                {
                    GetImagesCompleted(this, new GenericEventArgs<ImagesBrand>(results));
                }

            }, null);
           
        }

       

    }
}
