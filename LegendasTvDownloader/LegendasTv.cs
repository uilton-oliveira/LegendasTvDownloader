using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;

namespace LegendasTvDownloader
{
    public static class LegendasTv
    {
        

        public static List<Useful.legendas> Buscar(string fileSearch, bool popularFotoDesc, int pagina = 0)
        {
            using (WebClient webClient = new CustomWebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                Useful.legendas sub;
                List<Useful.legendas> list = new List<Useful.legendas>();
                string page = "";
                if (pagina > 1)
                {
                    page = "/-/"+pagina;
                }

                //MessageBox.Show("Iniciando busca");                                                                         //1 = idioma
                string html = webClient.DownloadString("http://legendas.tv/util/carrega_legendas_busca/" + fileSearch + "/1" + page);
                //MessageBox.Show("busca ok1");
                string ret = html.SearchAndCut("<div class=\"middle \"> ", "<div class=\"clear\">").text; // inicio e fim de onde fica as legendas
                bool mais = html.Contains("load_more");
                string id = "";
                string nome;
                int startPos = 0;
                Useful.search ret2 = ret.SearchAndCut("<div class=\"f_left\"><p><a href=\"", "</a>", startPos);
                if (ret2.pos == -1)
                {
                    return new List<Useful.legendas>();
                }
                bool foto = false;

                string serviceName = "Legendas.tv";

                //MessageBox.Show("entrou while");
                while (ret2.pos != -1)
                {
                    sub = new Useful.legendas();
                    sub.maisPagina = mais;
                    id = ret2.text.SearchAndCut("download/", "/").text;
                    //MessageBox.Show("ID: " + id);
                    nome = ret2.text.SearchAndCut(">", "").text;

                    //sub.id = id;
                    sub.nome = nome;
                    sub.download = "http://legendas.tv/downloadarquivo/" + id;
                    sub.serviceName = serviceName;

                    startPos = ret2.pos;

                    if (popularFotoDesc && !foto)
                    {
                        //MessageBox.Show("Baixando foto...");
                        foto = true;
                        string html2 = webClient.DownloadString("http://legendas.tv/download/" + id);
                        //MessageBox.Show(html2);
                        Useful.search ret3 = html2.SearchAndCut("<section class=\"first\">", "</section>");
                        string img = ret3.text.SearchAndCut("<img src=\"", "\"").text;
                        if (!img.Contains("http")) {
                            img = "http://legendas.tv/" + img;
                        }
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
