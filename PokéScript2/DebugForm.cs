using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace PokéScript2
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();

            txtDebug.DocumentText = "<html></html>";
            WriteHtmlLine("<center><h2>PokéScript II " + Assembly.GetExecutingAssembly().GetName().Version + "</h2></center>");
        }

        private void DebugForm_Load(object sender, EventArgs e)
        {
            
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void txtDebug_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            // Prevents the browser from going elsewhere
            if (e.Url.ToString() != "about:blank") e.Cancel = true;
        }

        public void WriteHtml(string html, bool autoscroll = true)
        {
            // Write HTML
            txtDebug.Document.Write(html);

            // Scroll to message
            if (autoscroll) txtDebug.Document.Body.ScrollTop = txtDebug.Document.Body.ScrollRectangle.Height;
        }

        public void WriteHtmlLine(string html, bool autoscroll = true)
        {
            // Write HTML
            txtDebug.Document.Write(html + "<br>");

            // Scroll to message
            if (autoscroll) txtDebug.Document.Body.ScrollTop = txtDebug.Document.Body.ScrollRectangle.Height;
        }
        
        // Does not work, for whatever reason
        /*public void Clear()
        {
            txtDebug.DocumentText = "<html></html>";
            WriteHtmlLine("<h3>PokéScript II " + Assembly.GetExecutingAssembly().GetName().Version + "</h3>");
        }*/

    }
}
