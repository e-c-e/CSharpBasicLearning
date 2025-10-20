using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace serialportSend
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReceiveData receiveData = new ReceiveData(richTextBox1);
            try 
            {
                if (receiveData.Start())
                {
                    label7.Text = "Open Success!";
                } 
            }
            catch (Exception ex)
            {
                label7.Text = ex.ToString();
            }
        }
    }
}
