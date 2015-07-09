using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PokéScript2
{
    public partial class MainForm : Form
    {
        private string codeFile = string.Empty;

        public MainForm(string[] args)
        {
            InitializeComponent();

            // Choose "No" on the A-Map script editor dialog
            // Let's hope this works
            if (args.Length == 2)
            {
                // Arg 1 is file, arg 2 is offset
                // Decompile accordingly

                if (!File.Exists(args[0]))
                {
                    MessageBox.Show("The passed ROM file does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }

                // TODO
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists("std.pks2"))
            {
                MessageBox.Show("std.pks2 not found!\n\nThis isn't an error, it just means your scripts won't support the standard library of defined values.\n\nYou should probably go download it.", "Uh-oh!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Ask to save
            var ask = MessageBox.Show("Would you like to save the current script?", "Save?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (ask == DialogResult.Cancel) return;
            else if (ask == DialogResult.Yes)
            {
                // I could invoke the save menu item, but this is more customizable.

                if (codeFile == string.Empty)
                {
                    saveFileDialog1.Title = "Save Script As...";
                    saveFileDialog1.Filter = "PokéScript II Files|*.pks2;*.pp|Text Files|*.txt|All Files|*.*";
                    //saveFileDialog1.FileName = "";

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        codeFile = saveFileDialog1.FileName;
                    }
                    else return;
                }

                File.WriteAllText(codeFile, txtCode.Text);
            }

            txtCode.Text = "";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Ask to save
            var ask = MessageBox.Show("Would you like to save the current script?", "Save?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (ask == DialogResult.Cancel) return;
            else if (ask == DialogResult.Yes)
            {
                if (codeFile == string.Empty)
                {
                    saveFileDialog1.Title = "Save Script As...";
                    saveFileDialog1.Filter = "PokéScript II Files|*.pks2;*.pp|Text Files|*.txt|All Files|*.*";
                    //saveFileDialog1.FileName = "";

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        codeFile = saveFileDialog1.FileName;
                    }
                    else return;
                }

                File.WriteAllText(codeFile, txtCode.Text);
            }

            // Get the file
            openFileDialog1.Title = "Open Script";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Supported Files|*.pks2;*.pp;*.txt|PokéScript II Files|*.pks2;*.pp|Text Files|*.txt|All Files|*.*";
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            // Load the file
            codeFile = openFileDialog1.FileName;
            //txtCode.LoadFile(codeFile);
            txtCode.Text = File.ReadAllText(codeFile);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (codeFile == string.Empty)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                File.WriteAllText(codeFile, txtCode.Text);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Save Script As...";
            saveFileDialog1.Filter = "PokéScript II Files|*.pks2;*.pp|Text Files|*.txt|All Files|*.*";
            //saveFileDialog1.FileName = "";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                codeFile = saveFileDialog1.FileName;
                File.WriteAllText(codeFile, txtCode.Text);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtCode.TextLength > 0)
                {
                    Compiler cmpler = new Compiler();
                    //string[] lines = Compiler.Explode(txtCode.Text);
                    cmpler.Debug(txtCode.Text);

                    string rlines = "dynamic: 0x" + cmpler.DynamicOffset.ToString("X") + "\nfreespace: " + cmpler.FreeSpaceByte + "\n\n";
                    /*foreach(string line in lines)
                    {
                        rlines += "$" + line + "$\n";
                    }*/
                    /*foreach (var b in blocks)
                    {
                        rlines += b.ToString() + "\n\n";
                    }*/
                    MessageBox.Show(rlines);
                }
            }
            catch( Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
