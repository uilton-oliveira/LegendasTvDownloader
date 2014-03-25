using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NinjaCode;
using System.Windows.Forms;

namespace LegendasTvDownloader
{
    public static class LegendasTv
    {
        public struct legendas
        {
            public string id;
            public string nome;
            public string titulo;
            public string download;
            public string fotoUrl;
            public string descricao;
            public bool maisPagina;

        }

        public static List<legendas> Buscar(string fileSearch, bool popularFotoDesc, int pagina = 0)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                legendas sub;
                List<legendas> list = new List<legendas>();
                string page = "";
                if (pagina > 1)
                {
                    page = "/page:"+pagina;
                }

                string html = webClient.DownloadString("http://legendas.tv/util/carrega_legendas_busca/termo:" + fileSearch + "/id_idioma:1" + page);
                string ret = html.SearchAndCut("<div class=\"middle \"> ", "<div class=\"clear\">").text; // inicio e fim de onde fica as legendas
                bool mais = html.Contains("load_more");
                string id = "";
                string nome;
                int startPos = 0;
                NinjaCode.Useful.search ret2 = ret.SearchAndCut("<div class=\"f_left\"><p><a href=\"", "</a>", startPos);
                if (ret2.pos == -1)
                {
                    return new List<legendas>();
                }
                bool foto = false;

                while (ret2.pos != -1)
                {
                    sub = new legendas();
                    sub.maisPagina = mais;
                    id = ret2.text.SearchAndCut("download/", "/").text;
                    nome = ret2.text.SearchAndCut(">", "").text;

                    sub.id = id;
                    sub.nome = nome;
                    sub.download = "http://legendas.tv/downloadarquivo/" + id;

                    startPos = ret2.pos;

                    if (popularFotoDesc && !foto)
                    {
                        foto = true;
                        string html2 = webClient.DownloadString("http://legendas.tv/download/" + id);
                        NinjaCode.Useful.search ret3 = html2.SearchAndCut("<section class=\"first\">", "</section>");
                        string img = "http://legendas.tv/" + ret3.text.SearchAndCut("<img src=\"", "\"").text;
                        string titulo = ret3.text.SearchAndCut("<h5>", "</h5>").text;
                        string desc = ret3.text.SearchAndCut("<p>", "</p>").text.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");

                        sub.titulo = titulo;
                        sub.descricao = desc;
                        sub.fotoUrl = img;
                    }

                    list.Add(sub);

                    ret2 = ret.SearchAndCut("<div class=\"f_left\"><p><a href=\"", "</a>", startPos);
                }
                return list;

            }
        }
    }
}
