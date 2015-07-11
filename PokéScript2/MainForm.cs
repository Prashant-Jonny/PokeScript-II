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
        private string romFile = string.Empty;
        // TODO: dictionary for useful stuff like language settings?
        private HashSet<string> goodROMCodes = new HashSet<string>()
        {
            "AXVE", // Pokemon Ruby - English
            "AXPE", // Pokemon Sapphire - English
            "BPRE", // Pokemon FireRed - English
            "BPGE", // Pokemon LeafGreen - English
            "BPEE", // Pokemon Emerald - English

            "AXVF", // Pokemon Ruby - French
            "AXPF", // Pokemon Sapphire - French
            "BPRF", // Pokemon FireRed - French
            "BPGE", // Pokemon LeafGreen - French
            "BPEF", // Pokemon Emerald - French

            "AXVI", // Pokemon Ruby - Italian
            "AXPI", // Pokemon Sapphire - Italian
            "BPRI", // Pokemon FireRed - Italian
            "BPGI", // Pokemon LeafGreen - Italian
            "BPEI", // Pokemon Emerald - Italian

            "AXVS", // Pokemon Ruby - Spanish
            "AXPS", // Pokemon Sapphire - Spanish
            "BPRS", // Pokemon FireRed - Spanish
            "BPGS", // Pokemon LeafGreen - Spanish
            "BPES", // Pokemon Emerald - Spanish

            "AXVJ", // Pokemon Ruby - Japanese
            "AXPJ", // Pokemon Sapphire - Japanese
            "BPRJ", // Pokemon FireRed - Japanese
            "BPGJ", // Pokemon LeafGreen - Japanese
            "BPEJ", // Pokemon Emerald - Japanese
        };

        private DebugForm debug = new DebugForm();
        private Compiler compiler = null;

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
            compiler = new Compiler(ref debug);
            if (!File.Exists("std.pks2"))
            {
                //MessageBox.Show("std.pks2 not found!\n\nThis isn't an error, it just means your scripts won't support the standard library of defined values.\n\nYou should probably go download it.", "Uh-oh!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                debug.WriteHtmlLine("<span style=\"color: red;\">std.pks2 not found!<br><br>This isn't an error, it just means your scripts won't support the standard library of defined values.<br><br>You should probably go download it.</span>");
            }

            debug.Show();
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
            if (romFile == string.Empty)
            {
                MessageBox.Show("No ROM file has been opened!", "Uh-oh!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (txtCode.TextLength > 0)
            {
                debug.Show();

                compiler.Debug(txtCode.Text, romFile);
            }
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open ROM";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "GBA ROMs|*.gba";
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            using (GBABinaryReader gb = new GBABinaryReader(openFileDialog1.FileName))
            {
                gb.BaseStream.Seek(0xA0, SeekOrigin.Begin);
                string name = gb.ReadString(12);
                string code = gb.ReadString(4);

                //MessageBox.Show(string.Format("Name: {0}\nCode: {1}", name, code));

                if (!goodROMCodes.Contains(code))
                {
                    MessageBox.Show(string.Format("{0} is not a recognized Pokémon ROM code!", code), "Uh-oh!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            romFile = openFileDialog1.FileName;
        }
    }
}
