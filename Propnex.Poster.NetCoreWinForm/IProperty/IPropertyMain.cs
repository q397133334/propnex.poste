using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class IPropertyMain : Form
    {
        public IPropertyMain()
        {
            InitializeComponent();
        }

        private void mainControl1_Load(object sender, EventArgs e)
        {
            mainControl1.GetForm = () =>
            {
                return new IPropertyCefPoster();
            };
        }

        private void IPropertyMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel= true;
        }
    }
}
