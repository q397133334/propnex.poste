namespace Propnex.Poster.NetCoreWinForm
{
    partial class IPropertyCefPoster
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chromiumWebBrowser = new CefSharp.WinForms.ChromiumWebBrowser();
            this.SuspendLayout();
            // 
            // chromiumWebBrowser1
            // 
            this.chromiumWebBrowser.ActivateBrowserOnCreation = false;
            this.chromiumWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chromiumWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.chromiumWebBrowser.Name = "chromiumWebBrowser1";
            this.chromiumWebBrowser.Size = new System.Drawing.Size(1008, 661);
            this.chromiumWebBrowser.TabIndex = 0;
            this.chromiumWebBrowser.Text = "chromiumWebBrowser1";
            // 
            // IPropertyCefPoster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 661);
            this.Controls.Add(this.chromiumWebBrowser);
            this.Name = "IPropertyCefPoster";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private CefSharp.WinForms.ChromiumWebBrowser chromiumWebBrowser;
    }
}