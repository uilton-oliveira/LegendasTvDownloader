using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NUnrar.Archive;
using NUnrar.Common;
using NUnrar.Reader;
using System.IO;
using System.IO.Compression;
using System.Globalization;

using NinjaCode;
using System.Diagnostics;
using System.Reflection;
using IMDB;

namespace LegendasTvDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string curDir = exe.Substring(0, exe.LastIndexOf("\\"));
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\LegendasTv");
            reg.SetValue("", "Buscar via Legendas.tv");
            reg.SetValue("Icon", "\"" + curDir + "\\" + "icon.ico\"");
            reg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\LegendasTv\command");
            reg.SetValue("", "\"" + exe + "\" \"%1\"");
            this.MinimumSize = new System.Drawing.Size(800, 615);
            InitializeComponent();
        }

        Dictionary<int, LegendasTv.legendas> founds = new Dictionary<int, LegendasTv.legendas>();
        public string curFileName = "";
        public string curFullFileName = "";
        public static int pagina = 1;
        public int cv = 13;
        

        private void Form1_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(VersionThread)).Start();
            String[] arguments = Environment.GetCommandLineArgs();

            if (arguments.Length > 1)
            {
                new Thread(new ThreadStart(WorkThread)).Start();
            }
            else
            {
                textBox1.Text = "Digite aqui o que deseja buscar!";
            }

            
        }
        
        public void VersionThread()
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                string html = webClient.DownloadString("http://www.garenaworld.com/images/g_master/checker/lgtv.txt");
                string v = html.SearchAndCut("[v]", "[/v]").text;
                string url = html.SearchAndCut("[url]", "[/url]").text;
                int lv = Convert.ToInt32(v);
                if (lv > cv)
                {
                    if (MessageBox.Show("Nova versão disponível, deseja visualizar?", "Atualização",
                         MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                         == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(url);
                        //Application.Exit();
                    }
                }
            }
        }

        


        public void ManualThread()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            this.Invoke(new Action(() => this.Text = "Buscando legenda, aguarde..."));
            try
            {
                string fileSearch = textBox1.Text;
                fileSearch = Regex.Replace(fileSearch, @"\s+", " "); // remover espaços duplos
                this.Invoke(new Action(() => this.Text = fileSearch));
                bool loadFoto = pagina <= 1 ? true : false;
                List<LegendasTv.legendas> list = LegendasTv.Buscar(fileSearch, loadFoto, pagina);
                if (!list.Any())
                {
                    MessageBox.Show("Nenhuma legenda encontrada para: " + fileSearch);
                    button1.Enabled = true;
                    button2.Enabled = true;
                    return;
                }

                // carregar mais paginas
                bool mais = list[list.Count - 1].maisPagina;
                carregarMais.Invoke(new Action(() => carregarMais.Visible = mais));

                //carregar foto e descrição
                if (loadFoto)
                {
                    pictureBox1.Invoke(new Action(() => pictureBox1.ImageLocation = list[0].fotoUrl));
                    description1.Invoke(new Action(() => description1.Text = list[0].descricao));
                    this.Invoke(new Action(() => this.Text = list[0].titulo));
                }

                foreach (LegendasTv.legendas leg in list)
                {
                    CheckState check = CheckState.Unchecked;
                    if (checkedListBox1.Items.Count == 0)
                    {
                        check = CheckState.Checked;
                    }
                    founds[checkedListBox1.Items.Count] = leg;
                    checkedListBox1.Invoke(new Action(() => checkedListBox1.Items.Add(leg.nome, check)));
                }

                button1.Enabled = true;
                button2.Enabled = true;

                //MessageBox.Show(html);
            }
            catch (System.InvalidOperationException ex)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": " + ex.Message);
                // log errors
            }
        }


        public void WorkThread()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            this.Invoke(new Action(() => this.Text = "Buscando legenda, aguarde..."));
            try
            {
                String[] arguments = Environment.GetCommandLineArgs();
                string file = "";//"helix.s01e12.720p.hdtv.x264-killers";
                string title2 = "";

                if (arguments.Length == 1)
                {
                    this.Invoke(new Action(() => this.Hide()));
                    MessageBox.Show("Clique com o botão direito no arquivo desejado e clique para buscar legenda!");
                    Application.Exit();
                    return;
                }

                // obter nome passado pelo parametro
                file = arguments[1];
                curFullFileName = arguments[1];

                // extrair o diretório
                curFileName = file = file.ExtractFileName();
                
                string resolution = "";
                string group = "";
                string year = "";

                if (curFileName.Contains("720p"))
                {
                    resolution = "720p";
                }
                else if (curFileName.Contains("1080p"))
                {
                    resolution = "1080p";
                }
                else if (curFileName.Contains("480p"))
                {
                    resolution = "480p";
                }
                group = curFileName.Replace("-", ".");
                group = group.Substring(group.LastIndexOf(".") + 1).ToLower();



                // verificar primeiro se é seriado pois é verificação local, filme usa internet...
                NinjaCode.Useful.series serie = file.extractSerieName();
                if (serie.match) // é seriado
                {
                    file = serie.searchText;
                }
                else // é filme
                {
                    NinjaCode.Useful.subtitles sub = NinjaCode.Useful.GetInfoSubtitles(curFullFileName);
                    if (sub.resultado)
                    {
                        // extrair do subtitles.com
                        title2 = sub.title;
                        file = title2;
                        year = sub.year;
                    }
                    else
                    {
                        // extrair do proprio nome do arquivo
                        title2 = curFileName.extractMovieName();
                        file = title2;
                    }
                }
                this.Invoke(new Action(() => this.Text = file));


                string fileSearch = file.Replace(".", " ").Replace(":", " ");
                fileSearch = Regex.Replace(fileSearch, @"\s+", " "); // remover espaços duplos
                textBox1.Invoke(new Action(() => textBox1.Text = fileSearch));


                List<LegendasTv.legendas> list = LegendasTv.Buscar(fileSearch, true, 1);
                if (!list.Any())
                {
                    if ((fileSearch = file.GetOriginalTitleImdb()) != "")
                    {
                        fileSearch = Regex.Replace(fileSearch, @"\s+", " "); // remover espaços duplos
                        this.Invoke(new Action(() => this.Text = fileSearch));
                        textBox1.Invoke(new Action(() => textBox1.Text = fileSearch));

                        list = LegendasTv.Buscar(fileSearch, true, 1);
                    }
                }

                this.Invoke(new Action(() => this.Text = title2));

                if (!list.Any())
                {
                    MessageBox.Show("Nenhuma legenda encontrada para: " + file);
                    Application.Exit();
                    return;
                }


                // carregar mais paginas
                bool mais = list[list.Count - 1].maisPagina;
                carregarMais.Visible = mais;

                // carregar foto e descrição
                //if (list[0].fotoUrl != "") {
                pictureBox1.Invoke(new Action(() => pictureBox1.ImageLocation = list[0].fotoUrl));
                description1.Invoke(new Action(() => description1.Text = list[0].descricao));
                this.Invoke(new Action(() => this.Text = list[0].titulo));
                //}
                //MessageBox.Show("Resolution: " + resolution +"\nGroup: " + group);
                bool perfect = false;
                int resFound = 0;

                foreach (LegendasTv.legendas leg in list)
                {
                    CheckState check = CheckState.Unchecked;
                    if (checkedListBox1.Items.Count == 0)
                    {
                        check = CheckState.Checked;
                    }
                    if ((resolution != "" && leg.nome.ToLower().Contains(resolution)) || (group != "" && leg.nome.ToLower().Contains(group)))
                    {
                        if (group != "" && leg.nome.ToLower().Contains(group))
                        {
                            check = CheckState.Checked;
                            perfect = true;
                        }
                        if (resolution != "" && leg.nome.ToLower().Contains(resolution))
                        {
                            if (perfect)
                            {
                                if (leg.nome.ToLower().Contains(group))
                                {
                                    check = CheckState.Checked;
                                }
                            }
                            else
                            {
                                if (resFound == 0)
                                {
                                    check = CheckState.Checked;
                                }
                            }
                            resFound++;
                        }
                    }
                    founds[checkedListBox1.Items.Count] = leg;
                    checkedListBox1.Invoke(new Action(() => checkedListBox1.Items.Add(leg.nome, check)));
                }

                button1.Enabled = true;
                button2.Enabled = true;


                //MessageBox.Show(html);
            }
            catch (System.InvalidOperationException ex)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": " + ex.Message);
                // log errors
            }
        }

        public AsyncCompletedEventHandler DownloadFileCompleted(string filename)
        {
            button1.Enabled = true;
            Action<object,AsyncCompletedEventArgs> action = (sender,e) =>
            {
                var _filename = filename;
                if (curFileName == "")
                {
                    MessageBox.Show("Baixado com sucesso!");
                    return;
                }

                if (e.Error != null)
                {
                    throw e.Error;
                }

                //MessageBox.Show("0");
                FileStream fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
                //MessageBox.Show("1: " + _filename);
                RarArchive archive = RarArchive.Open(fileStream, RarOptions.None);
                string curDir = Directory.GetCurrentDirectory();
                string curFileNameN = curFileName;
                //MessageBox.Show("2: " + curDir);
                foreach (RarArchiveEntry entry in archive.Entries)
                {
                    //MessageBox.Show("3: " + entry.FilePath.ExtractFileNameExt());
                    string path = curDir + "\\" + entry.FilePath.ExtractFileNameExt().RemoveAccents();//Path.Combine(curDir, entry.FilePath.ExtractFileNameExt());
                    //MessageBox.Show("4: " + path);
                    string tmp = entry.FilePath.ExtractFileName().RemoveAccents();
                    if (tmp == "")
                    {
                        continue;
                    }
                    //MessageBox.Show(tmp);
                    //MessageBox.Show("Comparando: \""+tmp+"\" com \""+curFileNameN+"\""); 
                    if (tmp.Equals(curFileNameN, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            entry.WriteToFile(path, ExtractOptions.ExtractFullPath);
                            fileStream.Close();
                            File.Delete(_filename);
                        }
                        catch (Exception ex)
                        {

                        }
                        break;
                    }
                    //entry.extr
                }
                fileStream.Close();

                MessageBox.Show("Baixado com sucesso!");
                Application.Exit();
            };
            return new AsyncCompletedEventHandler(action);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            using (WebClient webClient = new WebClient())
            {
                if (checkedListBox1.Items.Count == 0)
                {
                    MessageBox.Show("Busque algo primeiro!");
                    button1.Enabled = true;
                    return;
                }
                string url = founds[checkedListBox1.CheckedIndices[0]].download;
                webClient.OpenRead(url);

                string header_contentDisposition = webClient.ResponseHeaders["content-disposition"];
                string filename = new ContentDisposition(header_contentDisposition).FileName;
                if (curFullFileName != "")
                {
                    filename = curFileName + Path.GetExtension(filename);
                }
                webClient.DownloadFileCompleted += DownloadFileCompleted(filename);
                webClient.DownloadFileAsync(new Uri(url), filename);
            }
            //MessageBox.Show();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                for (int ix = 0; ix < checkedListBox1.Items.Count; ++ix)
                    if (e.Index != ix) checkedListBox1.SetItemChecked(ix, false);
        }



        

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.garenaworld.com");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            founds.Clear();
            pagina = 1;
            curFileName = "";
            curFullFileName = "";
            new Thread(new ThreadStart(ManualThread)).Start();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Digite aqui o que deseja buscar!")
            {
                textBox1.Text = "";
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "Digite aqui o que deseja buscar!";
            }
        }

        private void carregarMais_Click(object sender, EventArgs e)
        {
            pagina++;
            carregarMais.Visible = false;
            this.Text = "Carregando mais, aguarde...";
            new Thread(new ThreadStart(ManualThread)).Start();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                button2.PerformClick();
            }
        }

    
    
    }

}
