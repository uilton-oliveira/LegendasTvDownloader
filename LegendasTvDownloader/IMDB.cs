using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;

namespace LegendasTvDownloader
{
    public static class IMDB
    {
        public static string GetOriginalTitleImdb(this string title) {
            string busca = title.Replace(".", " ").Replace(" ", "+").RemoveAccents();
            string findt = "http://www.imdb.com/find?q=" + busca;

            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                string tmpHtml = webClient.DownloadString(findt);
                Useful.search first = tmpHtml.SearchAndCut("<td class=\"result_text\"> <a href=\"", "/a>");
                if (first.pos >= 0)
                {
                    Useful.search ftitle = first.text.SearchAndCut(">", "<");
                    if (ftitle.pos >= 0)
                    {
                        // primeira página do imdb mostra o nome em português, comparar pra ver se é o mesmo que estamos buscando, se for abrir a página e pegar o original title
                        if (title.Replace(".", " ").ToLower().RemoveAccents().Trim().Equals(ftitle.text.Replace(".", " ").ToLower().RemoveAccents().Trim()))
                        {
                            string url = "http://www.imdb.com" + first.text.SearchAndCut("", "\"").text;
                            string tmpHtml2 = webClient.DownloadString(url); // abrir a pagina de resultado pra pegar o original title
                            Useful.search sotitle = tmpHtml2.SearchAndCut("<span class=\"title-extra\" itemprop=\"name\">", "</span>");
                            if (sotitle.pos >= 0)
                            {
                                Useful.search otitle = sotitle.text.SearchAndCut("\"", "\"");
                                if (otitle.pos >= 0)
                                {
                                    return otitle.text.Replace(":", " ");
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }
    }
}
