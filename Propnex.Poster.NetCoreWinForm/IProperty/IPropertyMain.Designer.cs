namespace Propnex.Poster.NetCoreWinForm
{
    partial class IPropertyMain : Volo.Abp.DependencyInjection.ITransientDependency
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IPropertyMain));
            this.mainControl1 = new Propnex.Poster.NetCoreWinForm.MainControl();
            this.SuspendLayout();
            // 
            // mainControl1
            // 
            this.mainControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainControl1.GetForm = null;
            this.mainControl1.Location = new System.Drawing.Point(0, 0);
            this.mainControl1.Name = "mainControl1";
            this.mainControl1.Size = new System.Drawing.Size(784, 461);
            this.mainControl1.TabIndex = 0;
            this.mainControl1.Load += new System.EventHandler(this.mainControl1_Load);
            // 
            // IPropertyMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.mainControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IPropertyMain";
            this.Text = "IPropertyPoster";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IPropertyMain_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private MainControl mainControl1;
    }
}