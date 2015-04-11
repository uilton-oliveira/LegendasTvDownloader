using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;

namespace LegendasTvDownloader
{
    public partial class Monitoramento : Form
    {
        public Monitoramento()
        {
            InitializeComponent();
        }

        public class Monitor
        {
            public string datahora_verificacao { get; set; }
            public string datahora { get; set; }
            public int achou { get; set; }
            public string consulta { get; set; }
            public int pk { get; set; }
            public string email { get; set; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void updateList()
        {
            monitorView1.Items.Clear();
            using (WebClient webClient = new CustomWebClient())
            {
                webClient.Encoding = Encoding.UTF8;


                string json = webClient.DownloadString("http://legendasws.darksupremo.com/monitor/list/" + Form1.email);


                if (!json.Equals("not_found"))
                {
                    try
                    {
                        List<Monitor> mList = JsonConvert.DeserializeObject<List<Monitor>>(json);
                        foreach (Monitor m in mList)
                        {
                            ListViewItem item1 = new ListViewItem(m.pk.ToString());
                            item1.SubItems.Add(m.email);
                            item1.SubItems.Add(m.consulta);
                            item1.SubItems.Add(m.achou == 1 ? "Sim" : "Não");

                            DateTime myDate = DateTime.ParseExact(m.datahora_verificacao, "dd/MM/yy HH:mm:ss",
                                               System.Globalization.CultureInfo.CurrentCulture);

                            TimeZoneInfo nzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                            DateTime nzDateTime = TimeZoneInfo.ConvertTimeFromUtc(myDate, nzTimeZone);

                            item1.SubItems.Add(nzDateTime.ToString("dd/MM/yy - HH:mm"));
                            monitorView1.Items.Add(item1);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Falha ao se conectar com o servidor, ou o servidor retornou algo inesperado, retorno: " + json);
                    }
                }
                else
                {
                    MessageBox.Show("Você atualmente não está monitorando nada, para monitorar algo, primeiro procure por algo que não seja encontrado, aí será exibido um aviso para monitorar");
                }


                

            }
        }

        private void Monitoramento_Shown(object sender, EventArgs e)
        {
            if (Form1.email.Equals("your@email.com"))
            {
                MessageBox.Show("Cadastre seu email em: \"Menu->Monitor->Configurar Email\"");
                this.Close();
            }
            else
            {
                updateList();
            }
        }

        private void deletarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection indexes = monitorView1.SelectedIndices;
            foreach (int index in indexes)
            {
                string email = this.monitorView1.Items[index].SubItems[1].Text;
                string id = this.monitorView1.Items[index].SubItems[0].Text;
                string busca = this.monitorView1.Items[index].SubItems[2].Text;
                DialogResult dr = MessageBox.Show("Deseja realmente deletar este monitoramento?", busca, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dr == DialogResult.Yes)
                {
                    using (WebClient webClient = new CustomWebClient())
                    {
                        webClient.Encoding = Encoding.UTF8;
                        string msg = webClient.DownloadString(@"http://legendasws.darksupremo.com/monitor/del/"+id+@"/"+email);
                        if (msg.Equals("done"))
                        {
                            updateList();
                            MessageBox.Show("Apagado com sucesso!", busca);
                        }
                        else
                        {
                            MessageBox.Show(msg);
                        }
                        
                    }
                    
                }

            }
            
        }
    }
}
