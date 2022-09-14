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
    public partial class CefPoster : Form
    {
        public CefPoster()
        {
            InitializeComponent();
        }

        private async void CefPoster_Load(object sender, EventArgs e)
        {
            await chromiumWebBrowser1.LoadUrlAsync("www.baidu.com");
        }
    }
}
