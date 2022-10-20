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
    public partial class Main : Form
    {

        CefPoster cefPoster;

        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cefPoster = new CefPoster();
            cefPoster.Show();
            cefPoster.PosterStart();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (cefPoster == null)
            //{
            //    cefPoster = new CefPoster();
            //    cefPoster.FormClosed += (s, ev) =>
            //    {
            //        cefPoster = null;
            //    };
            //    cefPoster.Show();
            //}
        }
    }
}
