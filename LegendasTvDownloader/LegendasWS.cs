using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LegendasTvDownloader
{
    public class Legenda
    {
        public string download { get; set; }
        public string id { get; set; }
        public string service { get; set; }
        public string nome { get; set; }
    }

    public class LegendaObj
    {
        public string poster { get; set; }
        public string mais_paginas { get; set; }
        public List<Legenda> legendas { get; set; }
        public string descricao { get; set; }
        public string titulo { get; set; }
    }


    class LegendasWS
    {

        public static LegendaObj buscar(string busca, string hash, int pagina, bool isMovie)
        {
            using (WebClient webClient = new CustomWebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                var json = "";
                if (String.IsNullOrEmpty(hash))
                {
                    string url = "http://legendasws.darksupremo.com/busca=" + Useful.Base64Encode(busca) + "/pagina=" + pagina.ToString();
                    json = webClient.DownloadString(url);
                }
                else
                {
                    string url = "http://legendasws.darksupremo.com/busca=" + Useful.Base64Encode(busca) + "/hash=" + hash + "/pagina=" + pagina.ToString() + "/lgtv_use_hash=" + (isMovie ? "1" : "0");
                    json = webClient.DownloadString(url);
                }


                return JsonConvert.DeserializeObject<LegendaObj>(json);
            }
        }

        public static LegendaObj buscar(string busca, int pagina)
        {
            return buscar(busca, "", pagina, false);
        }

        public static List<Useful.legendas> buscar_old(string busca, string hash, int pagina, bool isMovie)
        {

            LegendaObj lg = buscar(busca, hash, pagina, isMovie);
            Useful.legendas old;
            List<Useful.legendas> oldList = new List<Useful.legendas>();
            foreach (Legenda l in lg.legendas)
            {
                old = new Useful.legendas();
                old.descricao = lg.descricao;
                old.download = l.download;
                old.fotoUrl = lg.poster;
                old.id = l.id;
                old.nome = l.nome;
                old.serviceName = l.service;
                old.titulo = lg.titulo;
                old.maisPagina = lg.mais_paginas == "1" ? true : false;
                oldList.Add(old);
            }

            return oldList;
        }

        public static List<Useful.legendas> buscar_old(string busca, int pagina)
        {
            return buscar_old(busca, "", pagina, false);
        }
    }
}
