using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volo.Abp.EventBus;

namespace Propnex.Poster.NetCoreWinForm
{
    public partial class IPropertyMain : Form,Volo.Abp.DependencyInjection.ITransientDependency
    {
        IServiceProvider _serviceProvider;
        public IPropertyMain(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();

            this.Text = $"IProperty Poster Version{Assembly.GetEntryAssembly().GetFileVersion()}"; 
        }

        private void mainControl1_Load(object sender, EventArgs e)
        {
            mainControl1.GetForm = () =>
            {
                return _serviceProvider.GetService<IPropertyCefPoster>();
            };
        }

        private void IPropertyMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
