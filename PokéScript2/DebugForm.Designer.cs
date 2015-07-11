namespace PokéScript2
{
    partial class DebugForm
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
            this.txtDebug = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // txtDebug
            // 
            this.txtDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDebug.Location = new System.Drawing.Point(0, 0);
            this.txtDebug.MinimumSize = new System.Drawing.Size(20, 20);
            this.txtDebug.Name = "txtDebug";
            this.txtDebug.Size = new System.Drawing.Size(1059, 490);
            this.txtDebug.TabIndex = 0;
            this.txtDebug.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.txtDebug_Navigating);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 490);
            this.Controls.Add(this.txtDebug);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "DebugForm";
            this.Text = "Debug";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebugForm_FormClosing);
            this.Load += new System.EventHandler(this.DebugForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser txtDebug;
    }
}