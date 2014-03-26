using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Windows.Forms;   

namespace NinjaCode
{
    public static class Useful
    {
        public static string RemoveAccents(this string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();

            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        public struct search
        {
            public int pos;
            public string text;
        }

        public struct subtitles
        {
            public string id;
            public string title;
            public string hash;
            public string year;
            public bool resultado;
        }

        public struct series
        {
            public string name;
            public string season;
            public string episode;
            public string searchText;
            public bool match;
        }

        public static string ExtractFileName(this string filePath)
        {
            string file = filePath;
            //MessageBox.Show("ExtractFileName: " + filePath);
            if (file.Contains(@"\"))
            {
                int start = file.LastIndexOf(@"\") + 1;
                if (file.Contains("."))
                {
                    int end = file.LastIndexOf(".") - start;
                    if (end <= 0)
                    {
                        //MessageBox.Show("Pasta!!!");
                        return "";
                    }
                    file = file.Substring(start, end);
                }
                else
                {
                    file = file.Substring(start);
                }
            }
            else
            {
                int end = file.LastIndexOf(".");
                if (end <= 0)
                {
                    return "";
                }
                file = file.Substring(0, end);
            }
            return file;
        }

        public static string ExtractFileNameExt(this string filePath)
        {
            string file = filePath;
            if (filePath.Contains("\\"))
            {
                int start = file.LastIndexOf(@"\") + 1;
                file = file.Substring(start);
            }
            return file;
        }

        public static string ExtractExtension(this string filePath)
        {
            string file = filePath;
            if (filePath.Contains("\\"))
            {
                int start = file.LastIndexOf(@"\") + 1;
                file = file.Substring(start);
            }
            return file;
        }

        public static subtitles GetInfoSubtitles(string filePath)
        {
            using (WebClient webClient = new WebClient())
            {
                subtitles sub = new subtitles();
                webClient.Encoding = Encoding.UTF8;
                string hash = ToHexadecimal(ComputeMovieHash(filePath));
                string html = webClient.DownloadString("http://www.getsubtitle.com/webService/downloadManager/get_movie_by_hash.php?hash=" + hash);
                if (!html.StartsWith("10000000")) // 10000000 = não encontrado
                {
                    sub.resultado = true;
                    sub.title = html.SearchAndCut("|", "(").text;
                    sub.year = html.SearchAndCut("(", ")").text;
                    sub.id = html.SearchAndCut("", "|").text;
                    sub.hash = hash;
                }
                else
                {
                    sub.resultado = false;
                    sub.title = "";
                    sub.year = "";
                    sub.id = "";
                    sub.hash = hash;

                }
                return sub;
            }

        }

        public static string extractMovieName(this string line)
        {
            string text = line.Replace(".", " ");


            Regex regex2 = new Regex(@"(.*?)(dvdrip|xvid| cd[0-9]|dvdscr|420p|720p|1080p|brrip|divx|[\{\(\[]?[0-9]{4}).*");
            Match match2 = regex2.Match(text);
            if (match2.Success)
            {
                text = match2.Groups[1].Value;
            }

            Regex regex3 = new Regex(@"(.*?)\(.*\)(.*)");
            Match match3 = regex3.Match(text);
            if (match3.Success)
            {
                text = match3.Groups[1].Value;
            }
            return text.Trim();
        }

        public static series extractSerieName(this string file)
        {
            string Standard = @"^((?<series_name>.+?)[. _-]+)?s(?<season_num>\d+)[. _-]*e(?<ep_num>\d+)(([. _-]*e|-)(?<extra_ep_num>(?!(1080|720)[pi])\d+))*[. _-]*((?<extra_info>.+?)((?<![. _-])-(?<release_group>[^-]+))?)?$";

            Regex regex = new Regex(Standard);
            Match match = regex.Match(file.ToLower());
            series serie = new series();
            if (match.Success)
            {
                string Showname = match.Groups["series_name"].Value;
                string Season = match.Groups["season_num"].Value;
                string Episode = match.Groups["ep_num"].Value;
                serie.episode = Episode;
                serie.season = Season;
                serie.name = Showname;
                serie.searchText = Showname.Replace(".", " ") + " S" + Season + "E" + Episode;
                serie.match = true;
            }
            else
            {
                serie.episode = "";
                serie.season = "";
                serie.name = "";
                serie.searchText = "";
                serie.match = false;
            }
            return serie;
        }

        public static search SearchAndCut(this string text, string start, string end)
        {
            return SearchAndCut(text, start, end, 0);
        }

        public static search SearchAndCut(this string text, string start, string end, int startPos)
        {
            int st = 0;
            if (!start.Equals(""))
            {
                st = text.IndexOf(start, startPos);
            }
            if (st == -1)
            {
                search sc2 = new search();
                sc2.text = "";
                sc2.pos = -1;
                return sc2;
            }
            st += start.Length;
            int en = text.Length;
            if (!end.Equals(""))
            {
                en = text.IndexOf(end, st);
            }
            search sc = new search();
            sc.pos = st;
            sc.text = text.Substring(st, en - st);
            return sc;
        }

        private static byte[] ComputeMovieHash(Stream input)
        {
            long length = input.Length;
            long num1 = length;
            long num2 = 0L;
            byte[] buffer = new byte[8];
            while (num2 < 8192L && input.Read(buffer, 0, 8) > 0)
            {
                ++num2;
                num1 += BitConverter.ToInt64(buffer, 0);
            }
            input.Position = Math.Max(0L, length - 65536L);
            long num3 = 0L;
            while (num3 < 8192L && input.Read(buffer, 0, 8) > 0)
            {
                ++num3;
                num1 += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            byte[] bytes = BitConverter.GetBytes(num1);
            Array.Reverse((Array)bytes);
            return bytes;
        }

        private static byte[] ComputeMovieHash(string filename)
        {
            using (Stream input = (Stream)File.OpenRead(filename))
                return ComputeMovieHash(input);
        }

        private static string ToHexadecimal(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < bytes.Length; ++index)
                stringBuilder.Append(bytes[index].ToString("x2"));
            return ((object)stringBuilder).ToString();
        }
    }
}
