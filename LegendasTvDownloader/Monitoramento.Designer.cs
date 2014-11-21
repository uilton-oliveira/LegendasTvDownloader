namespace LegendasTvDownloader
{
    partial class Monitoramento
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.monitorView1 = new System.Windows.Forms.ListView();
            this.id1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.email1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.busca1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.achou1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.datahora1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deletarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expira1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // monitorView1
            // 
            this.monitorView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.monitorView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.id1,
            this.email1,
            this.busca1,
            this.achou1,
            this.datahora1,
            this.expira1});
            this.monitorView1.ContextMenuStrip = this.contextMenuStrip1;
            this.monitorView1.FullRowSelect = true;
            this.monitorView1.Location = new System.Drawing.Point(12, 12);
            this.monitorView1.Name = "monitorView1";
            this.monitorView1.Size = new System.Drawing.Size(811, 328);
            this.monitorView1.TabIndex = 0;
            this.monitorView1.UseCompatibleStateImageBehavior = false;
            this.monitorView1.View = System.Windows.Forms.View.Details;
            // 
            // id1
            // 
            this.id1.Text = "ID";
            this.id1.Width = 32;
            // 
            // email1
            // 
            this.email1.Text = "Email";
            this.email1.Width = 161;
            // 
            // busca1
            // 
            this.busca1.Text = "Busca";
            this.busca1.Width = 258;
            // 
            // achou1
            // 
            this.achou1.Text = "Achou";
            // 
            // datahora1
            // 
            this.datahora1.Text = "Ultima Verificação";
            this.datahora1.Width = 176;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deletarToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(112, 26);
            // 
            // deletarToolStripMenuItem
            // 
            this.deletarToolStripMenuItem.Name = "deletarToolStripMenuItem";
            this.deletarToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.deletarToolStripMenuItem.Text = "Deletar";
            this.deletarToolStripMenuItem.Click += new System.EventHandler(this.deletarToolStripMenuItem_Click);
            // 
            // expira1
            // 
            this.expira1.Text = "Quando expira";
            this.expira1.Width = 118;
            // 
            // Monitoramento
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 352);
            this.Controls.Add(this.monitorView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Monitoramento";
            this.Text = "Monitoramento Legendas.tv";
            this.Shown += new System.EventHandler(this.Monitoramento_Shown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView monitorView1;
        private System.Windows.Forms.ColumnHeader id1;
        private System.Windows.Forms.ColumnHeader email1;
        private System.Windows.Forms.ColumnHeader busca1;
        private System.Windows.Forms.ColumnHeader achou1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deletarToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader datahora1;
        private System.Windows.Forms.ColumnHeader expira1;

    }
}