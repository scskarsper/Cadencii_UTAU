using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace balthasar.presamp2dictionary
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> VMap = new Dictionary<string, string>();
        Dictionary<string, string> CMap = new Dictionary<string, string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd=new OpenFileDialog();
            ofd.FileName = textBox1.Text;
            if(ofd.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
                LoadSamp(textBox1.Text);
            }
        }

        void LoadSamp(string T)
        {
            bool isVOWEL=false;
            bool isCONSONANT=false;
            string[] S=System.IO.File.ReadAllLines(T);
            foreach (string s in S)
            {
                if (s.Substring(0, 1) == "[")
                {
                    isVOWEL = false;
                    isCONSONANT = false;
                }
                if (s.IndexOf("[VOWEL]") >= 0)
                {
                    isVOWEL = true;
                    continue;
                }
                if (s.IndexOf("[CONSONANT]") >= 0)
                {
                    isCONSONANT = true;
                    continue;
                }
                if (isVOWEL)
                {
                    string[] k = s.Split('=');
                    string Si1 = k[0];
                    string Si2 = k[1];
                    string Si3 = k[2];
                    string[] o = Si3.Split(',');
                    foreach (string so in o)
                    {
                        try
                        {
                            VMap.Add(so, Si1);
                            listBox1.Items.Add(so + " : " + Si1);
                        }
                        catch { ;}
                    }
                }
                if (isCONSONANT)
                {
                    string[] k = s.Split('=');
                    string Si1 = k[0];
                    string Si2 = k[1];
                    string[] o = Si2.Split(',');
                    foreach (string so in o)
                    {
                        try
                        {
                            CMap.Add(so, Si1);
                            listBox2.Items.Add(so + " : " + Si1);
                        }
                        catch { ;}
                    }
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "") return;
            if (textBox2.Text == "") return;
            if (VMap.Count==0 || CMap.Count==0) return;
            StringBuilder SB = new StringBuilder();
            SB.AppendLine("#本文件为程序生成自动拆音表1.0");
            SB.AppendLine("#拆音表格式为：");
            SB.AppendLine("#     前中后三音决定发音：前音+\",\"+当前音+\", \"+下一音=发音音素A+\", \"+发音音素B");
            SB.AppendLine("#     中后两音决定发音：当前音+\", \"+下一音=发音音素A+\", \"+发音音素B");
            SB.AppendLine("#     前后两音决定发音：前音+\",\"+当前音+\",*\"=发音音素A+\", \"+发音音素B");
            SB.AppendLine("#     结尾休止音：当前音+\",R\"=发音音素A+\", \"+发音音素B");
            SB.AppendLine("#拆音表有以下必有属性");
            SB.AppendLine("#属性1：Charset，是当前文件的文字编码，中文系统填GBK就好，日文的写ShiftJIS");
            SB.AppendLine("#属性2：Name，当前发音表在Cadencii_UTAU中的识别名称");
            SB.AppendLine("#拆音表有以下可选属性属性");
            SB.AppendLine("#默认情况下,MinSplitNoteLength=120,设置小于该长度的音符不切音");
            SB.AppendLine("#=======================================");
            SB.AppendLine("#以下是属性配置区域");
            SB.AppendLine("#Charset:GBK");
            SB.AppendLine("#Name:" + textBox3.Text);
            SB.AppendLine("#MinSplitNoteLength:" + textBox4.Text);
            SB.AppendLine("#=======================================");
            SB.AppendLine("#发音表开始");
            if (checkBox1.Checked)
            {
                if (!CMap.ContainsKey("R"))
                {
                    CMap.Add("R", textBox5.Text);
                }
            }
            foreach (KeyValuePair<string, string> KV1 in VMap)
            {
                foreach (KeyValuePair<string, string> KV2 in CMap)
                {
                    SB.AppendLine(KV1.Key + "," + KV2.Key + "="+KV1.Key+"," + KV1.Value + " " + KV2.Value);
                }
            }
            System.IO.File.WriteAllText(textBox2.Text, SB.ToString(),Encoding.GetEncoding("GBK"));
            MessageBox.Show("FINISHED");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.FileName = textBox2.Text;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = ofd.FileName;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
