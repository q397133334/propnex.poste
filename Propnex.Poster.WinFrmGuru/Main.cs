using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cefPoster == null)
            {
                cefPoster = _iocManager.Resolve<CefPoster>();
            }

            cefPoster.Show();
            cefPoster.FormClosed += (s, ev) =>
            {
                cefPoster = null;
            };
            cefPoster.PosterStart();
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
                };
                cefPoster.Show();
                cefPoster.PosterStart();
            }
            else
            {
                if (cefPoster == null)
                {
                    this.Close();
                }
            }
        }

        bool isStop = false;

        private void btnClose_Click(object sender, EventArgs e)
        {
            isStop = true;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isStop == false)
            {
                e.Cancel = true;
                MessageBox.Show("click close buttom");
            }
            if (cefPoster != null)
            {
                e.Cancel = true;
                MessageBox.Show("click close buttom");
            }
        }
    }
}
