namespace Propnex.Poster.Guru
{
    partial class CefPoster
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cwb = new CefSharp.WinForms.ChromiumWebBrowser();
            this.SuspendLayout();
            // 
            // cwb
            // 
            this.cwb.ActivateBrowserOnCreation = false;
            this.cwb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cwb.Location = new System.Drawing.Point(0, 0);
            this.cwb.Name = "cwb";
            this.cwb.Size = new System.Drawing.Size(1424, 861);
            this.cwb.TabIndex = 0;
            // 
            // CefPoster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1424, 861);
            this.Controls.Add(this.cwb);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CefPoster";
            this.Text = "CefPoster";
            this.Load += new System.EventHandler(this.CefPoster_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CefSharp.WinForms.ChromiumWebBrowser cwb;
    }
}