using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cadencii.ui.dotnet
{
    public partial class FormProgressbar : Form
    {
        public FormProgressbar(string Title)
        {
            this.Text = Title;
            InitializeComponent();
        }

        public ProgressBar pb
        {
            get
            {
                return this.progressBar1;
            }
            set
            {
                this.progressBar1 = value;
                label1.Text = value.Value.ToString()+"/"+value.Maximum.ToString();
            }
        }

        public void setMaxium(int Value)
        {
            this.progressBar1.Maximum = Value;
            label1.Text = this.progressBar1.Value.ToString() + "/" + this.progressBar1.Maximum.ToString();
        }
        public void setValue(int Value)
        {
            this.progressBar1.Value = Value;
            label1.Text = this.progressBar1.Value.ToString() + "/" + this.progressBar1.Maximum.ToString();
        }

        bool CanClose = false;
        public void CloseThis()
        {
            CanClose = true;
            this.Close();
        }
        private void FormProgressbar_Load(object sender, EventArgs e)
        {

        }

        private void FormProgressbar_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CanClose)
            {
                e.Cancel = true;
            }
        }
    }
}
