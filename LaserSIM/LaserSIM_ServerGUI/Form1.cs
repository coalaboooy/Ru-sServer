using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LaserSIM_ServerGUI
{
    public partial class Form1 : Form
    {
        public static IntPtr MainFormHandle;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MainFormHandle = this.Handle;
            Task.Run(() => Server.Run());
        }

        public void GetMessage(string message)
        {
            if (InfoTextBox.InvokeRequired)
            {
                InfoTextBox.BeginInvoke(new MethodInvoker(() =>
                {
                    InfoTextBox.AppendText(message + "\r\n");
                }));
            }
            else
                InfoTextBox.AppendText(message + "\r\n");
        }
    }
}
