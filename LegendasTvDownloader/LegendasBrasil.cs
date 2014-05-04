using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;

namespace LegendasTvDownloader
{
    class LegendasBrasil
    {
        public static string ver = "";

        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static List<Useful.legendas> Buscar(string hash, bool popularFoto)
        {
            using (WebClient client = new CustomWebClient())
            {
                client.Encoding = Encoding.UTF8;


                if (ver == "")
                {
                    ver = client.DownloadString("http://www.getsubtitle.com/webService/downloadManager/get_version.php?ver=");
                    ver = ver.SearchAndCut("software=Legendas", "").text;
                    if (ver == "" || !IsDigitsOnly(ver))
                    {
                        ver = "232";
                    }
                }

                System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection();
                reqparm.Add("version", ver);
                reqparm.Add("response", "xml");
                reqparm.Add("cod_language", "6");
                reqparm.Add("hash", Useful.Base64Encode(hash));
                byte[] responsebytes = client.UploadValues("http://www.getsubtitle.com/webService/downloadManager/get_subtitles_by_hash.php", "POST", reqparm);
                string html = Encoding.UTF8.GetString(responsebytes);

                //MessageBox.Show(html);

                Useful.legendas sub;
                List<Useful.legendas> list = new List<Useful.legendas>();

                string serviceName = "LegendasBrasil";

                int startPos = 0;
                Useful.search ret2 = html.SearchAndCut("<subtitle>", "</subtitle>", startPos);
                if (ret2.pos == -1)
                {
                    return new List<Useful.legendas>();
                }

                while (ret2.pos != -1)
                {
                    sub = new Useful.legendas();
                    startPos = ret2.pos;
                    
                    string id = Useful.Base64Decode(ret2.text.SearchAndCut("<id>", "</id>").text);
                    string name = Useful.Base64Decode(ret2.text.SearchAndCut("<name>", "</name>").text);
                    string date = Useful.Base64Decode(ret2.text.SearchAndCut("<date>", "</date>").text);

                    sub.id = id;
                    sub.nome = name;

                    sub.download = "http://www.getsubtitle.com/webService/download_subtitle.php?post_date="+date+"&cod_bsplayer="+id;

                    sub.serviceName = serviceName;

                    list.Add(sub);
                    
                    ret2 = html.SearchAndCut("<subtitle>", "</subtitle>", startPos);
                }

                return list;
            }
        }
    }
}
