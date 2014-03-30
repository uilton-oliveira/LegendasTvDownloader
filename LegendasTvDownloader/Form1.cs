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

using System.Diagnostics;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;

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

        Dictionary<int, Useful.legendas> founds = new Dictionary<int, Useful.legendas>();
        public string curFileName = "";
        public string curFullFileName = "";
        public static int pagina = 1;
        public int cv = 16;
        

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
                List<Useful.legendas> list = LegendasTv.Buscar(fileSearch, loadFoto, pagina);
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

                foreach (Useful.legendas leg in list)
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


        public void CheckedListBox1Clear() {
            foreach (int index in checkedListBox1.CheckedIndices) {
                checkedListBox1.Invoke(new Action(() => checkedListBox1.SetItemChecked(index, false)));
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


                Useful.subtitles sub = Useful.GetInfoSubtitles(curFullFileName);
                


                // verificar primeiro se é seriado pois é verificação local, filme usa internet...
                Useful.series serie = file.extractSerieName();
                if (serie.match) // é seriado
                {
                    file = serie.searchText;
                }
                else // é filme
                {
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


                List<Useful.legendas> list = LegendasTv.Buscar(fileSearch, true, 1);
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
                list.AddRange(LegendasBrasil.Buscar(sub.hash, false));

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
                bool legtv = false;
                bool legbrasil = false;

                foreach (Useful.legendas leg in list)
                {
                    bool isLegtv = leg.serviceName == "Legendas.tv";
                    bool isLegbr = leg.serviceName == "LegendasBrasil";
                    CheckState check = CheckState.Unchecked;
                    if (checkedListBox1.Items.Count == 0)
                    {
                        check = CheckState.Checked;
                    }
                    if (isLegbr)
                    {
                        if (!legbrasil && (!legtv || !perfect))
                        {
                            check = CheckState.Checked;
                            legbrasil = true;
                            CheckedListBox1Clear();
                        }
                    }
                    else if ((resolution != "" && leg.nome.ToLower().Contains(resolution)) || (group != "" && leg.nome.ToLower().Contains(group)))
                    {
                        if (group != "" && leg.nome.ToLower().Contains(group))
                        {
                            check = CheckState.Checked;
                            CheckedListBox1Clear();
                            perfect = true;
                            legtv = isLegtv;
                            legbrasil = isLegbr;
                        }
                        if (resolution != "" && leg.nome.ToLower().Contains(resolution))
                        {
                            if (perfect)
                            {
                                if (leg.nome.ToLower().Contains(group))
                                {
                                    check = CheckState.Checked;
                                    CheckedListBox1Clear();
                                    legtv = isLegtv;
                                    legbrasil = isLegbr;

                                }
                            }
                            else
                            {
                                if (resFound == 0)
                                {
                                    check = CheckState.Checked;
                                    CheckedListBox1Clear();
                                    legtv = isLegtv;
                                    legbrasil = isLegbr;

                                }
                            }
                            resFound++;
                        }
                    }
                    founds[checkedListBox1.Items.Count] = leg;
                    checkedListBox1.Invoke(new Action(() => checkedListBox1.Items.Add("["+leg.serviceName+"] "+ leg.nome, check)));
                }

                checkedListBox1.Invoke(new Action(() => checkedListBox1.ClearSelected()));

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


        public static int downCount = 0;
        public static int downCountPos = 0;
        public AsyncCompletedEventHandler DownloadFileCompleted(string filename)
        {
            
            //MessageBox.Show("downCountPos: " + downCountPos + " / downCount:" + downCount);
            Action<object,AsyncCompletedEventArgs> action = (sender,e) =>
            {
                downCountPos++;
                var _filename = filename;
                if (curFileName == "")
                {
                    if (downCountPos == downCount)
                    {
                        button1.Enabled = true;
                        MessageBox.Show("Baixado(s) com sucesso!");
                    }
                    return;
                }

                if (e.Error != null)
                {
                    if (downCountPos == downCount)
                    {
                        button1.Enabled = true;
                        MessageBox.Show("Baixado(s) com sucesso!");
                    }
                    //throw e.Error;
                    return;
                }

                if (_filename.EndsWith(".zip".ToLower()))
                {
                    using (ZipInputStream s = new ZipInputStream(File.OpenRead(_filename)))
                    {

                        ZipEntry theEntry;
                        while ((theEntry = s.GetNextEntry()) != null)
                        {

                            Console.WriteLine(theEntry.Name);

                            string directoryName = Path.GetDirectoryName(theEntry.Name);
                            string fileName = theEntry.Name.ExtractFileName();


                            string curDir = Directory.GetCurrentDirectory();
                            string path = curDir + "\\" + theEntry.Name.ExtractFileNameExt().RemoveAccents();

                            if (fileName.Equals(curFileName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (fileName != String.Empty)
                                {
                                    string extractPath = path;
                                    if (File.Exists(extractPath))
                                    {
                                        int z = 1;
                                        do
                                        {
                                            extractPath = curDir + "\\" + theEntry.Name.ExtractFileName().RemoveAccents() + "(" + z + ")" + Path.GetExtension(path);
                                            z++;
                                        } while (File.Exists(extractPath));

                                    }

                                    using (FileStream streamWriter = File.Create(extractPath))
                                    {

                                        int size = 2048;
                                        byte[] data = new byte[2048];
                                        while (true)
                                        {
                                            size = s.Read(data, 0, data.Length);
                                            if (size > 0)
                                            {
                                                streamWriter.Write(data, 0, size);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    File.Delete(_filename);
                                }
                            }
                        }
                    }
 
                }
                else if (_filename.EndsWith(".rar".ToLower()))
                {
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
                                if (File.Exists(path))
                                {
                                    int z = 1;
                                    string tmp3 = "";
                                    do
                                    {
                                        tmp3 = curDir + "\\" + entry.FilePath.ExtractFileName().RemoveAccents() + "(" + z + ")" + Path.GetExtension(path);
                                        z++;
                                    } while (File.Exists(tmp3));

                                    entry.WriteToFile(tmp3);
                                }
                                else
                                {
                                    entry.WriteToFile(path);
                                }
                                fileStream.Close();
                                File.Delete(_filename);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                            break;
                        }
                        //entry.extr
                    }
                    fileStream.Close();
                }
                if (downCountPos == downCount)
                {
                    button1.Enabled = true;
                    MessageBox.Show("Baixado(s) com sucesso!");
                }
                //Application.Exit();
            };
            return new AsyncCompletedEventHandler(action);
        }

        public void btn1Thread()
        {
            downCount = 0;
            downCountPos = 0;

            if (checkedListBox1.Items.Count == 0)
            {
                MessageBox.Show("Busque algo primeiro!");
                button1.Enabled = true;
                return;
            }
            downCount = checkedListBox1.CheckedIndices.Count;

            foreach (int ic in checkedListBox1.CheckedIndices)
            {
                using (WebClient webClient = new WebClient())
                {
                    string url = founds[ic].download;
                    var stream = webClient.OpenRead(url);

                    string header_contentDisposition = webClient.ResponseHeaders["content-disposition"];
                    string filename = new ContentDisposition(header_contentDisposition).FileName;
                    stream.Close();
                    if (curFullFileName != "")
                    {
                        filename = curFileName + Path.GetExtension(filename);
                        if (File.Exists(filename))
                        {
                            int z = 1;
                            string tmp3 = "";
                            do
                            {
                                filename = curFileName + "(" + z + ")" + Path.GetExtension(filename);
                                z++;
                            } while (File.Exists(tmp3));

                        }
                    }
                    else
                    {
                        if (File.Exists(filename))
                        {
                            int z = 1;
                            string tmp3 = "";
                            do
                            {
                                filename = filename.ExtractFileName() + "(" + z + ")" + Path.GetExtension(filename);
                                z++;
                            } while (File.Exists(tmp3));

                        }
                    }
                    //MessageBox.Show("Baixando: " + url + " ("+filename+")");
                    try
                    {
                        webClient.DownloadFileCompleted += DownloadFileCompleted(filename);
                        webClient.DownloadFileAsync(new Uri(url), filename);
                        //webClient.DownloadFile(new Uri(url), filename);
                        //webClient.Dispose();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception: " + ex.ToString());
                    }
                }
            }
            //webClient.Dispose();

            button1.Enabled = true;
            //MessageBox.Show();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            new Thread(new ThreadStart(btn1Thread)).Start();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //if (e.NewValue == CheckState.Checked)
              //  for (int ix = 0; ix < checkedListBox1.Items.Count; ++ix)
                //    if (e.Index != ix) checkedListBox1.SetItemChecked(ix, false);
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

        private void button3_Click(object sender, EventArgs e)
        {
            using (WebClient webClient = new WebClient())
            {
                if (pictureBox1.ImageLocation == "" || pictureBox1.ImageLocation.Length == 0)
                {
                    MessageBox.Show("Busque algo primeiro!");
                    return;
                }

                webClient.DownloadFile(new Uri(pictureBox1.ImageLocation), "Poster" + Path.GetExtension(pictureBox1.ImageLocation));
                MessageBox.Show("Poster baixado com sucesso!");
            }
        }

    
    
    }

}
