using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LegendasTvDownloader
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.usuario.Text = Properties.Settings.Default.usuario;
            this.senha.Text = Properties.Settings.Default.senha;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(usuario.Text) || String.IsNullOrEmpty(senha.Text))
            {
                MessageBox.Show("Usuário ou Senhas vazio!");
                return;
            }
            if (Useful.loginLegendasTv(false, usuario.Text, senha.Text) != null)
            {
                Properties.Settings.Default["usuario"] = usuario.Text;
                Properties.Settings.Default["senha"] = Useful.Base64Encode(senha.Text);
                Properties.Settings.Default.Save();

                MessageBox.Show("Usuário e senha gravados com sucesso!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario ou senha inválidos!");
                return;
            }
            

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["usuario"] = "";
            Properties.Settings.Default["senha"] = "";
            Properties.Settings.Default.Save();
            usuario.Text = "";
            senha.Text = "";
        }
    }
}
