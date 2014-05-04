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
using System.Runtime.CompilerServices;

namespace LegendasTvDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string curDir = exe.Substring(0, exe.LastIndexOf("\\"));
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\LegendasTv");
            reg.SetValue("", "Buscar Legendas");
            reg.SetValue("Icon", "\"" + curDir + "\\" + "icon.ico\"");
            reg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\LegendasTv\command");
            reg.SetValue("", "\"" + exe + "\" \"%1\"");
            this.MinimumSize = new System.Drawing.Size(800, 615);

            RegistryKey reg2 = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\LegendasTv2");
            reg2.SetValue("", "Baixar melhor Legenda (suporta multiplos)");
            reg2.SetValue("Icon", "\"" + curDir + "\\" + "icon.ico\"");
            reg2 = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\LegendasTv2\command");
            reg2.SetValue("", "\"" + exe + "\" \"%1\" -hide");
            this.MinimumSize = new System.Drawing.Size(800, 615);

            InitializeComponent();
        }

        Dictionary<int, Useful.legendas> founds = new Dictionary<int, Useful.legendas>();
        public string curFileName = "";
        public string curFullFileName = "";
        public static int pagina = 1;
        public int cv = 20;
        public static bool hide = false;
        public static string serviceName;
        private readonly object syncLock = new object();
        private readonly object syncLock2 = new object();
        private int waiting = 0;


        public void NewInstance(string param)
        {
            ShowMessage("new instance: " + param);
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
             
        }
        
        public void VersionThread()
        {
            using (WebClient webClient = new CustomWebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                string html = webClient.DownloadString("http://www.garenaworld.com/images/g_master/checker/lgtv.txt");
                string v = html.SearchAndCut("[v]", "[/v]").text;
                string url = html.SearchAndCut("[url]", "[/url]").text;
                int lv = Convert.ToInt32(v);
                if (lv > cv)
                {
                    if (MessageBox.Show(this, "Nova versão disponível, deseja visualizar?", "Atualização",
                         MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000)
                         == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(url);
                        if (hide)
                        {
                            Application.Exit();
                        }
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
                    ShowMessage("Nenhuma legenda encontrada para: " + fileSearch);
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

                //ShowMessage(html);
            }
            catch (System.InvalidOperationException ex)
            {

            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString() + ": " + ex.Message);
                // log errors
            }
        }


        public void CheckedListBox1Clear() {
            foreach (int index in checkedListBox1.CheckedIndices) {
                checkedListBox1.Invoke(new Action(() => checkedListBox1.SetItemChecked(index, false)));
            }
        }


        public void StartWorkThread(String[] arguments)
        {
            Thread thread = new Thread(() => WorkThread(arguments));
            thread.Start();
        }

        public void WorkThread(String[] arguments)
        {
            lock (syncLock2)
            {
                waiting++;
            }

            lock (syncLock)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                this.Invoke(new Action(() => this.Text = "Buscando legenda, aguarde..."));
                try
                {
                    //String[] arguments = Environment.GetCommandLineArgs();
                    string file = "";//"helix.s01e12.720p.hdtv.x264-killers";
                    string title2 = "";

                    if (arguments.Length == 1)
                    {
                        this.Invoke(new Action(() => this.Hide()));
                        ShowMessage("Clique com o botão direito no arquivo desejado e clique para buscar legenda!");
                        Application.Exit();
                        return;
                    }

                    // obter nome passado pelo parametro
                    file = arguments[1];
                    curFullFileName = file;
                    string locFullFileName = file;

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


                    List<Useful.legendas> list = LegendasTv.Buscar(fileSearch, !hide, 1);
                    if (!list.Any())
                    {
                        if ((fileSearch = file.GetOriginalTitleImdb()) != "")
                        {
                            fileSearch = Regex.Replace(fileSearch, @"\s+", " "); // remover espaços duplos
                            this.Invoke(new Action(() => this.Text = fileSearch));
                            textBox1.Invoke(new Action(() => textBox1.Text = fileSearch));

                            list = LegendasTv.Buscar(fileSearch, !hide, 1);
                        }
                    }
                    list.AddRange(LegendasBrasil.Buscar(sub.hash, false));

                    this.Invoke(new Action(() => this.Text = title2));

                    if (!list.Any())
                    {
                        ShowMessage("Nenhuma legenda encontrada para: " + file);
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
                    //ShowMessage("Resolution: " + resolution +"\nGroup: " + group);
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
                        checkedListBox1.Invoke(new Action(() => checkedListBox1.Items.Add("[" + leg.serviceName + "] " + leg.nome, check)));
                    }

                    checkedListBox1.Invoke(new Action(() => checkedListBox1.ClearSelected()));

                    button1.Enabled = true;
                    button2.Enabled = true;

                    if (hide)
                    {
                        btn1Thread(locFullFileName);
                    }


                    //ShowMessage(html);
                }
                catch (System.InvalidOperationException ex)
                {

                }
                catch (Exception ex)
                {
                    ShowMessage(ex.ToString() + ": " + ex.Message);
                    // log errors
                }
            }
        }


        public static int downCount = 0;
        public static int downCountPos = 0;
        public AsyncCompletedEventHandler DownloadFileCompleted(string filename, string locFullFileName, string locFileName)
        {

            //ShowMessage("downCountPos: " + downCountPos + " / downCount:" + downCount);
            Action<object,AsyncCompletedEventArgs> action = (sender,e) =>
            {
                downCountPos++;
                var _filename = filename;
                if (locFileName == "")
                {
                    //if (downCountPos == downCount)
                    //{
                    //    button1.Enabled = true;
                    //    //ShowMessage("Baixado(s) com sucesso" + serviceName + "!");
                    //}
                    return;
                }

                if (e.Error != null)
                {
                    //if (downCountPos == downCount)
                    //{
                    //    button1.Enabled = true;
                    //    //ShowMessage("Baixado(s) com sucesso" + serviceName + "!");
                    //}
                    //throw e.Error;
                    return;
                }

                if (_filename.EndsWith(".zip".ToLower()))
                {
                    bool delete = false;
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

                            if (fileName.Equals(locFileName, StringComparison.OrdinalIgnoreCase))
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
                                    delete = true;
                                }
                            }
                        }
                    }
                    if (delete)
                    {
                        File.Delete(_filename);
                    }

                }
                else if (_filename.EndsWith(".rar".ToLower()))
                {
                    //ShowMessage("0");
                    FileStream fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
                    //ShowMessage("1: " + _filename);

                    RarArchive archive = RarArchive.Open(fileStream, RarOptions.None);
                    string curDir = Directory.GetCurrentDirectory();
                    string curFileNameN = locFileName;
                    //ShowMessage("2: " + curDir);
                    foreach (RarArchiveEntry entry in archive.Entries)
                    {
                        //ShowMessage("3: " + entry.FilePath.ExtractFileNameExt());
                        string path = curDir + "\\" + entry.FilePath.ExtractFileNameExt().RemoveAccents();//Path.Combine(curDir, entry.FilePath.ExtractFileNameExt());
                        //ShowMessage("4: " + path);
                        string tmp = entry.FilePath.ExtractFileName().RemoveAccents();
                        if (tmp == "")
                        {
                            continue;
                        }
                        //ShowMessage(tmp);
                        //ShowMessage("Comparando: \""+tmp+"\" com \""+curFileNameN+"\""); 
                        if (tmp.Equals(curFileNameN, StringComparison.OrdinalIgnoreCase))
                        {
                            string wpath = "";
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
                                    wpath = tmp3;
                                    entry.WriteToFile(tmp3);
                                }
                                else
                                {
                                    wpath = path;
                                    entry.WriteToFile(path);
                                }
                                fileStream.Close();
                                File.Delete(_filename);
                            }
                            catch (NUnrar.InvalidRarFormatException ex)
                            {
                                if (wpath != "")
                                {
                                    File.Delete(wpath);
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowMessage(ex.ToString());
                            }
                            break;
                        }
                        //entry.extr
                    }
                    fileStream.Close();
                }
                if (hide)
                {
                    lock (syncLock2)
                    {
                        waiting--;
                    }
                    if (waiting == 0)
                    {
                        ShowMessage("Baixado(s) com sucesso" + serviceName + "!");
                        if (hide)
                        {
                            Application.Exit();
                            return;
                        }
                    }
                }
                if (downCountPos == downCount)
                {
                    button1.Enabled = true;
                }
                //Application.Exit();
            };
            return new AsyncCompletedEventHandler(action);
        }

        string GetWebPageContent(string url, int bytesToGet)
        {
            string result = string.Empty;
            HttpWebRequest request;
            request = WebRequest.Create(url) as HttpWebRequest;

            //get first 1000 bytes
            request.AddRange(0, bytesToGet - 1);

            // the following code is alternative, you may implement the function after your needs
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }



        public void btn1Thread()
        {
            btn1Thread("");
        }
        public void btn1Thread(String firName)
        {
            try
            {
                string locFullFileName;
                string locFileName;
                if (firName != "")
                {
                    locFullFileName = firName;
                    locFileName = firName.ExtractFileName();
                }
                else
                {
                    locFullFileName = curFullFileName;
                    locFileName = curFileName;
                }

                downCount = 0;
                downCountPos = 0;

                if (checkedListBox1.Items.Count == 0)
                {
                    ShowMessage("Busque algo primeiro!");
                    button1.Enabled = true;
                    if (hide)
                    {
                        Application.Exit();
                        return;
                    }
                    return;
                }
                downCount = checkedListBox1.CheckedIndices.Count;



                foreach (int ic in checkedListBox1.CheckedIndices)
                {
                    if (downCount == 1)
                    {
                        serviceName = " (" + founds[ic].serviceName + ")";
                    }
                    else
                    {
                        serviceName = "";
                    }

                    using (CustomWebClient webClient = new CustomWebClient())
                    {
                        string url = founds[ic].download;

                        //string tst = webClient.
                        //ShowMessage(tst);

                       

                        var stream = webClient.OpenRead(url);
                        string filename = "";

                        string header_contentDisposition = webClient.ResponseHeaders["content-disposition"];
                        if (!string.IsNullOrEmpty(header_contentDisposition))
                        {
                            filename = new ContentDisposition(header_contentDisposition).FileName;
                            stream.Close();
                        }
                        else
                        {

                            string head = GetWebPageContent(url, 3);
                            if (head.StartsWith("Rar"))
                            {
                                filename = String.Join("", textBox1.Text.Split(Path.GetInvalidFileNameChars())) + ".rar";
                            }
                            else if (head.StartsWith("PK"))
                            {
                                filename = String.Join("", textBox1.Text.Split(Path.GetInvalidFileNameChars())) + ".zip";
                            }
                            else if (head.StartsWith("1\n"))
                            {
                                filename = String.Join("", textBox1.Text.Split(Path.GetInvalidFileNameChars())) + ".srt";
                            }
                        }
                        
                        //ShowMessage("filename: " + filename);
                        
                        if (locFullFileName != "")
                        {
                            filename = locFileName + Path.GetExtension(filename);
                            if (File.Exists(filename))
                            {
                                int z = 1;
                                string tmp3 = "";
                                do
                                {
                                    filename = locFileName + "(" + z + ")" + Path.GetExtension(filename);
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
                        //ShowMessage("Baixando: " + url + " ("+filename+")");
                        try
                        {
                            webClient.DownloadFileCompleted += DownloadFileCompleted(filename, locFullFileName, locFileName);
                            webClient.DownloadFileAsync(new Uri(url), filename);
                            //webClient.DownloadFile(new Uri(url), filename);
                            //webClient.Dispose();
                        }
                        catch (Exception ex)
                        {
                            ShowMessage("Exception: " + ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Exception: " + ex.ToString());
            }
            //webClient.Dispose();

            button1.Enabled = true;
            //ShowMessage();
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
            using (WebClient webClient = new CustomWebClient())
            {
                if (pictureBox1.ImageLocation == "" || pictureBox1.ImageLocation.Length == 0)
                {
                    ShowMessage("Busque algo primeiro!");
                    return;
                }

                webClient.DownloadFile(new Uri(pictureBox1.ImageLocation), "Poster" + Path.GetExtension(pictureBox1.ImageLocation));
                ShowMessage("Poster baixado com sucesso!");
            }
        }
        bool once = false;
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!once)
            {
                //WindowState = FormWindowState.Minimized;
                once = true;
            }
            else
            {
                return;
            }
            new Thread(new ThreadStart(VersionThread)).Start();
            String[] arguments = Environment.GetCommandLineArgs();

            if (arguments.Length > 1)
            {
                if (arguments.Length > 2)
                {
                    hide = arguments[2].ToLower() == "-hide";
                    if (hide)
                    {
                        WindowState = FormWindowState.Minimized;
                    }

                }
                //String[] args = Environment.GetCommandLineArgs();
                //new Thread(new ThreadStart(WorkThread)).Start();
                StartWorkThread(arguments);
            }
            else
            {
                textBox1.Text = "Digite aqui o que deseja buscar!";
            }
        }

        public void ShowMessage(string text)
        {
            ShowMessage(text, "Legendas.tv Downloader");
        }

        public void ShowMessage(string text, string caption)
        {
            MessageBox.Show(new Form() { TopMost = true },
                text,
                caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,  // specify "Yes" as the default
                (MessageBoxOptions)0x40000);      // specify MB_TOPMOST
        }

    
    
    }

}
