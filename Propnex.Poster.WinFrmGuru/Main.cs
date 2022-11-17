using Abp.Dependency;
using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Propnex.Poster.Guru
{
    public partial class Main : Form, ITransientDependency
    {

        CefPoster cefPoster;
        Abp.Dependency.IIocManager _iocManager;

        public Main(IIocManager iocManager)
        {
            InitializeComponent();
            _iocManager = iocManager;
            textBox1.Text = _iocManager.Resolve<ConfigurationJson<Setting>>().Value.AnyDesk;
            Console.SetOut(new ConsoleWrite(richTextBox1));
            Console.SetError(new ConsoleWrite(richTextBox1));
            richTextBox1.HideSelection = false;
            this.Text = "Propnex.Poster " + Application.ProductVersion;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Please input AnyDesk Number,if not AnyDesk,input 7 digits at random");
                return;
            }
            var setting = IocManager.Instance.Resolve<ConfigurationJson<Setting>>();
            setting.Value.AnyDesk = textBox1.Text;
            if (setting.Value.Id == "")
            {
                setting.Value.Id = await Api.WebServer.GetMachineIdAsync(setting.Value.AnyDesk);
                setting.Value.Id = setting.Value.Id.Trim('\"');
            }
            try
            {
                setting.Save();
            }
            catch (Exception ex)
            {
                IocManager.Instance.Resolve<ILogger>().Error(ex.Message, ex);
                MessageBox.Show("save error" + ex.StackTrace);
                return;
            }

            button1.Enabled = false;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cefPoster == null && isStop == false)
            {
                isStop = false;
                cefPoster = _iocManager.Resolve<CefPoster>();
                cefPoster.FormClosed += (s, ev) =>
                {
                    cefPoster = null;
                    if (isStop)
                    {
                        this.Close();
                    }
                };
                cefPoster.Show();
                Console.WriteLine("PosterStart");
                cefPoster.PosterStart();
            }
            else
            {
                if (cefPoster == null && isStop)
                {
                    this.Close();
                }
            }
        }

        bool isStop = false;

        private void btnClose_Click(object sender, EventArgs e)
        {
            isStop = true;
            btnClose.Enabled = false;
            timer1.Stop();
            this.Close();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (isStop == false)
            //{
            //    e.Cancel = true;
            //    MessageBox.Show("click close buttom");
            //}
            if (cefPoster != null || isStop == false)
            {
                e.Cancel = true;
                return;
            }

            _iocManager.Resolve<ILogger>().Info("FormClosing");
        }
    }


    public class ConsoleWrite : TextWriter
    {
        RichTextBox textBox;
        delegate void WriteFunc(string value);
        WriteFunc write;
        WriteFunc writeLine;

        public ConsoleWrite(RichTextBox textBox)
        {
            this.textBox = textBox;
            write = Write;
            writeLine = WriteLine;
        }

        /// <summary>
        /// 编码转换-UTF8
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
            //get { return Encoding.Unicode; }
        }

        /// <summary>
        /// 最低限度需要重写的方法
        /// </summary>
        public override void Write(string value)
        {
            if (textBox.InvokeRequired)
            {

                textBox.BeginInvoke(write, value);
            }
            else
            {
                textBox.AppendText(value);
            }
        }

        /// <summary>
        /// 为提高效率直接处理一行的输出
        /// </summary>
        public override void WriteLine(string value)
        {
            if (textBox.InvokeRequired)
            {
                textBox.BeginInvoke(writeLine, value);
            }
            else
            {
                textBox.AppendText(value);
                textBox.AppendText(this.NewLine);

            }

        }
    }
}
