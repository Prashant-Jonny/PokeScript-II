using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PokéScript2
{
    public class Compiler
    {
        // Comments: ; // '
        // Block Comments: /* */
        // Unimplemented:
        // Line Join: \

        // Some options and stuff
        public bool LegacyMode = false;
        public int DebugLevel = 1; // 0 = quiet, 1 = normal, 2 = loud
        public byte FreeSpaceByte = 0xFF;
        public uint DynamicOffset = 0x0;
        public bool DynamicOverwrite = false;
        public Dictionary<string, uint> Definitions = new Dictionary<string, uint>();

        public void Debug(string code)
        {
            string[] lines = Explode(code);
            Block[] blocks = Preprocess(lines);
        }

        // ----------------------------------------------------------
        // Lexer
        // ----------------------------------------------------------
        /// <summary>
        /// Takes an input string and splits it by lines.
        /// It also removes comments and trims whitespace.
        /// </summary>
        /// <param name="code">Input</param>
        /// <returns>Formatted strings.</returns>
        public static string[] Explode(string code)
        {
            List<string> result = new List<string>();

            // TODO: make this more intense

            StringBuilder sb = new StringBuilder();
            bool singleLineComment = false, multiLineComment = false;

            using (StringReader sr = new StringReader(code))
                while (sr.Peek() != -1)
                {
                    char c = (char)sr.Read();
                    if (c == '\n')
                    {
                        string line = sb.ToString().TrimEnd();
                        if (line.Length > 0) result.Add(line);
                        sb.Clear();

                        if (singleLineComment) singleLineComment = false;
                    }
                    else if (c == ';')
                    {
                        singleLineComment = true;
                    }
                    else if (c == '\'')
                    {
                        singleLineComment = true;
                    }
                    else if (c == '/')
                    {
                        if (sr.Peek() == '/')
                        {
                            sr.Read();
                            singleLineComment = true;
                        }
                        else if (sr.Peek() == '*')
                        {
                            sr.Read();
                            multiLineComment = true;
                        }
                    }
                    else if (c == '*')
                    {
                        if (multiLineComment && sr.Peek() == '/')
                        {
                            sr.Read();
                            multiLineComment = false;
                        }
                    }
                    else
                    {
                        if (!singleLineComment && !multiLineComment) sb.Append(c);
                    }
                }

            return result.ToArray();
        }

        // ----------------------------------------------------------
        // Preprocessor
        // ----------------------------------------------------------
        private Block[] Preprocess(string[] lines)
        {
            List<Block> result = new List<Block>();
            // TODO: dictionary of blocks? hashset?

            Block blk = null;
            string lastDirective = "";
            bool dynamicSet = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("#"))
                {
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;

                    #region Directives
                    if (IsValidDirective(parts, "#legacy")) LegacyMode = true;
                    else if (IsValidDirective(parts, "#org", 1))
                    {
                        // Add old block to collection, if necessary
                        if (blk != null)
                        {
                            result.Add(blk);
                        }

                        // Type of block
                        if (parts[1].StartsWith("@"))
                        {
                            blk = Block.DynamicBlock(parts[1]);
                        }
                        else
                        {
                            uint? u = parts[1].ToUInt32();
                            if (u != null) blk = Block.StaticBlock((uint)u);
                            else
                            {
                                throw new Exception("Invalid block offset!");
                            }
                        }
                    }
                    else if (IsValidDirective(parts, "#eorg", 1))
                    {
                        // Add old block to collection, if necessary
                        if (blk != null)
                        {
                            result.Add(blk);
                        }

                        // Type of block
                        if (parts[1].StartsWith("@"))
                        {
                            blk = Block.DynamicBlock(parts[1]);
                        }
                        else
                        {
                            uint? u = parts[1].ToUInt32();
                            if (u != null) blk = Block.StaticBlock((uint)u);
                            else
                            {
                                throw new Exception("Invalid block offset!");
                            }
                        }

                        // Erase flag
                        blk.EOrg = true;
                    }
                    else if (IsValidDirective(parts, "#erase", 2))
                    {

                    }
                    else if (IsValidDirective(parts, "#clean"))
                    {
                        // not supported yet
                        throw new NotImplementedException();
                    }
                    else if (IsValidDirective(parts, "#include", 1))
                    {

                    }
                    else if (IsValidDirective(parts, "#define", 2))
                    {

                    }
                    else if (parts[0] == "#raw" && parts.Length > 1)
                    {
                        // A special one
                        // Treat it like a command
                        // So error handling happens later...
                        if (blk != null)
                        {
                            blk.Lines.Add(line);
                        }
                        else
                        {
                            throw new Exception("Unable to have #raw outside a code block!");
                        }
                    }
                    else if (IsValidDirective(parts, "#quiet")) DebugLevel = 0;
                    else if (IsValidDirective(parts, "#loud")) DebugLevel = 2;
                    else if (IsValidDirective(parts, "#dynamic", 1) || IsValidDirective(parts, "#dyn", 1))
                    {
                        if (dynamicSet) throw new Exception("Dynamic offset has already been set once!");

                        uint? u = parts[1].ToUInt32();
                        if (u == null || u > 0x3FFFFFF) throw new Exception("Invalid dynamic offset!");
                        else DynamicOffset = (uint)u;

                        dynamicSet = true;
                    }
                    else if (IsValidDirective(parts, "#dynamic2", 1) || IsValidDirective(parts, "#dyn2", 1))
                    {
                        if (dynamicSet) throw new Exception("Dynamic offset has already been set once!");

                        uint? u = parts[1].ToUInt32();
                        if (u == null || u > 0x3FFFFFF) throw new Exception("Invalid dynamic offset!");
                        else DynamicOffset = (uint)u;

                        dynamicSet = true;
                        DynamicOverwrite = true;
                    }
                    else if (IsValidDirective(parts, "#ifdef", 1) || IsValidDirective(parts, "#ifdef", 2))
                    {

                    }
                    else if (IsValidDirective(parts, "#ifndef", 1) || IsValidDirective(parts, "#ifndef", 2))
                    {

                    }
                    else if (IsValidDirective(parts, "#endif"))
                    {

                    }
                    else if (IsValidDirective(parts, "#thumb") && lastDirective == "#org")
                    {
                        throw new NotImplementedException("Thumb code is not supported yet!");
                    }
                    else if (IsValidDirective(parts, "#freespace", 1))
                    {
                        uint? u = parts[1].ToUInt32();

                        if (u == null || u > 255) throw new Exception("Invalid freespace byte!");
                        else FreeSpaceByte = (byte)u;
                    }
                    else if (IsValidDirective(parts, "#reserve", 1) || IsValidDirective(parts, "#reserve", 2))
                    {

                    }
                    else
                    {
                        throw new Exception("Invalid directive " + parts[0] + "!");
                    }
                    #endregion

                    lastDirective = parts[0];

                }
                else if (line.StartsWith(":"))
                {
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    // labels
                    // basically an alias for #org (as I understand)
                }
                else if (blk != null)
                {
                    blk.Lines.Add(line);
                }
                else
                {
                    throw new Exception("Unable to process a command outside a code block!");
                }
            }

            if (blk != null) result.Add(blk);

            return result.ToArray();
        }

        public static bool IsValidDirective(string[] line, string command, int arguments = 0)
        {
            // Check if null
            if (line.Length == 0) return false;

            // Check command name and argument count
            bool name = line[0] == command;
            bool args = arguments == line.Length - 1;

            // Some boolean logic
            return name && args;
        }

        /// <summary>
        /// Represents a block of compilable code.
        /// </summary>
        public class Block
        {
            // TODO: make this two subclasses?
            public List<string> Lines;
            public bool EOrg;
            public bool Static;
            public string Name;
            public uint Offset;

            private Block()
            {
                Lines = new List<string>();
                EOrg = false;
                Static = false;
                Name = "";
            }

            public static Block StaticBlock(uint offset)
            {
                Block b = new Block();
                b.Static = true;
                b.Offset = offset;
                b.Name = "#";
                return b;
            }

            public static Block DynamicBlock(string name)
            {
                Block b = new Block();
                b.Static = false;
                b.Offset = 0;
                b.Name = name;
                return b;
            }

            public override string ToString()
            {
                //return base.ToString();
                return (EOrg ? "#eorg " : "#org ") + (Static ? Offset.ToString("X") : Name) + '\n' + string.Join("\n", Lines);
            }
        }


    }
}
