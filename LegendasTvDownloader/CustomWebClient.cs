using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace LegendasTvDownloader
{
    public class CustomWebClient : WebClient
    {
        public CustomWebClient() { }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/6.0;)";

            //... your other custom code...

            return request;
        }
    }
}
