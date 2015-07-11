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
        private const int EOF = -1;

        private string ROMFilePath;
        private DebugForm debug;

        // Comments: ; // '
        // Block Comments: /* */
        // Unimplemented:
        // Line Join: \

        // Some options and stuff
        public enum CompilerMode
        {
            Standard,
            Legacy,
            Battle, // These two are not supported yet. ;)
            BattleAI,
        }
        public CompilerMode Mode = CompilerMode.Standard;
        public enum DebugLevel
        {
            Quiet, Normal, Loud
        }
        public DebugLevel DebugMode = DebugLevel.Normal; // 0 = quiet, 1 = normal, 2 = loud
        public byte FreeSpaceByte = 0xFF;
        public uint DynamicOffset = 0x0;
        public bool DynamicOverwrite = false;
        public Dictionary<string, uint> Definitions = new Dictionary<string, uint>();

        public Compiler(ref DebugForm debug)
        {
            this.debug = debug;
        }

        public void Debug(string code, string romFile)
        {
            debug.WriteHtml(string.Format("<h3>Debug Script at {0}</h3>", DateTime.Now));

            try
            {
                // Reset the compiler
                Reset();
                ROMFilePath = romFile;

                // Load ROM environment variable
                Definitions[LoadROMCode()] = 0;

                Line[] lines = Explode(code);
                Preprocess(lines);

                if (DebugMode == DebugLevel.Loud)
                {
                    debug.WriteHtml("<h4>Lexer:</h4>");
                    foreach (var line in lines)
                    {
                        debug.WriteHtmlLine("" + line.ToString() + "");
                    }
                }

                debug.WriteHtmlLine("<span style =\"color: green;\"><br>Success!</span>");
            }
            catch (Exception ex)
            {
                debug.WriteHtmlLine("<span style =\"color: red;\">" + ex.Message + "</span>");
                debug.WriteHtmlLine("Debug failed!");
            }
        }

        public void Compile(string code, string romFile)
        {
            // Reset
            Reset();
            ROMFilePath = romFile;

            // Load ROM environment variable
            Definitions[LoadROMCode()] = 0;

            //Token[] tokens = Explode(code);
        }

        public void Reset()
        {
            ROMFilePath = string.Empty;

            Mode = CompilerMode.Standard;
            DebugMode = DebugLevel.Normal;
            FreeSpaceByte = 0xFF;
            DynamicOffset = 0;
            DynamicOverwrite = false;
            Definitions.Clear();
        }

        private string LoadROMCode()
        {
            if (ROMFilePath == string.Empty) return null;

            string code;
            using (GBABinaryReader gb = new GBABinaryReader(ROMFilePath))
            {
                gb.BaseStream.Seek(0xAC, SeekOrigin.Begin);
                code = gb.ReadString(4);
            }
            return code;
        }

        // ----------------------------------------------------------
        // Lexer
        // ----------------------------------------------------------
        #region
        /// <summary>
        /// Takes an input string and splits it into tokens and lines.
        /// </summary>
        /// <param name="code">The input string.</param>
        /// <returns>Lines containing tokens.</returns>
        private Line[] Explode(string code)
        {
            List<Token> tokens = new List<Token>();

            using (StringReader sr = new StringReader(code))
            {
                //Line line = null;
                //int num = 1;

                int line = 1;

                bool ignoreNewLine = false;
                while (sr.Peek() != EOF)
                {
                    char c = (char)sr.Read();

                    if (char.IsWhiteSpace(c)) // New line
                    {
                        if (c == '\n')
                        {
                            if (ignoreNewLine) ignoreNewLine = false;
                            else tokens.Add(new Token(TokenType.NewLine, line));

                            line++;
                        }
                    }
                    else if (c == '/') // Comments!
                    {
                        if (sr.Peek() == '/')
                        {
                            // Consume the rest of the line
                            while (sr.Peek() != '\n' && sr.Peek() != EOF)
                            {
                                sr.Read();
                            }
                        }
                        else if (sr.Peek() == '*')
                        {
                            // Eat '*'
                            sr.Read();

                            // Loop
                            while (true)
                            {
                                if (sr.Peek() == EOF)
                                {
                                    throw new Exception("Unterminated multi-line comment!");
                                }

                                c = (char)sr.Read();
                                if (c == '*' && sr.Peek() == '/')
                                {
                                    sr.Read();
                                    break;
                                }
                                else if (c == '\n') line++; // Increment line counter. ;)
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Invalid token '{0}'!", '/'));
                        }
                    }
                    else if (c == ';') // Multile statements on the same line
                    {
                        // So, pretend it's a new line I guess
                        tokens.Add(new Token(TokenType.NewLine, line));
                    }
                    else if (c == '\\')
                    {
                        // Continue line command
                        ignoreNewLine = true;
                    }
                    else if (c == '=') // String literal 1/== Condition
                    {
                        if (sr.Peek() == '=') // ;)
                        {
                            sr.Read();
                            tokens.Add(new Token(TokenType.EqualTo, line));
                        }
                        else
                        {
                            // Ignore first space
                            if (sr.Peek() == ' ') sr.Read();

                            // Check for no text
                            if (sr.Peek() == EOF || sr.Peek() == '\n')
                            {
                                throw new Exception("Expected string literal, got nothing!");
                            }

                            StringBuilder sb = new StringBuilder();
                            while (sr.Peek() != '\n' && sr.Peek() != EOF)
                            {
                                sb.Append((char)sr.Read());
                            }

                            tokens.Add(new Token(TokenType.StringLiteral, sb, line));
                        }
                    }
                    else if (c == '"') // String literal 2
                    {
                        StringBuilder sb = new StringBuilder();
                        while (sr.Peek() != '"')
                        {
                            if (sr.Peek() == EOF || sr.Peek() == '\n')
                            {
                                throw new Exception("Unterminated string literal!");
                            }

                            sb.Append((char)sr.Read());
                        }

                        // Eat "
                        sr.Read();

                        tokens.Add(new Token(TokenType.InlineStringLiteral, sb, line));
                    }
                    else if (c == '<') // String literal 3/Conditionals
                    {
                        // Why does this need to be so complicated?
                        // It would be so much simpler if didn't bother with this type of string.
                        // It's only used in #include anyway.
                        if (sr.Peek() == ' ')
                        {
                            tokens.Add(new Token(TokenType.LessThan, line));
                            sr.Read();
                        }
                        else if (sr.Peek() == '>')
                        {
                            tokens.Add(new Token(TokenType.NotEqualTo, line));
                            sr.Read();
                        }
                        else if (sr.Peek() == '=')
                        {
                            tokens.Add(new Token(TokenType.LessThanOrEqualTo, line));
                            sr.Read();
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            while (sr.Peek() != '>')
                            {
                                if (sr.Peek() == EOF || sr.Peek() == '\n')
                                {
                                    throw new Exception("Unterminated string literal!");
                                }

                                sb.Append((char)sr.Read());
                            }

                            // Eat >
                            sr.Read();

                            tokens.Add(new Token(TokenType.InlineStringLiteral2, sb, line));
                        }
                    }
                    else if (char.IsDigit(c))
                    {
                        StringBuilder sb = new StringBuilder();

                        // Different options
                        if (c == '0' && sr.Peek() == 'b')
                        {
                            // Eat b
                            sr.Read();

                            sb.Append("0b");
                            while (sr.Peek() != EOF)
                            {
                                c = (char)sr.Peek();
                                if (c.IsBinaryDigit())
                                {
                                    sb.Append(c);
                                    sr.Read();
                                }
                                else if (char.IsWhiteSpace(c))
                                {
                                    break;
                                }
                                else
                                {
                                    throw new Exception("Invalid binary number literal!");
                                }
                            }
                        }
                        else if (c == '0' && sr.Peek() == 'x')
                        {
                            // Eat x
                            sr.Read();

                            sb.Append("0x");
                            while (sr.Peek() != EOF)
                            {
                                c = (char)sr.Peek();
                                if (c.IsHexDigit())
                                {
                                    sb.Append(c);
                                    sr.Read();
                                }
                                else if (char.IsWhiteSpace(c))
                                {
                                    break;
                                }
                                else
                                {
                                    throw new Exception("Invalid hexadecimal number literal!");
                                }
                            }
                        }
                        else
                        {
                            sb.Append(c);

                            while (sr.Peek() != EOF)
                            {
                                c = (char)sr.Peek();
                                if (char.IsDigit(c))
                                {
                                    sb.Append(c);
                                    sr.Read();
                                }
                                else if (char.IsWhiteSpace(c)) break;
                                else
                                {
                                    throw new Exception("Invalid decimal number literal!");
                                }
                            }
                        }

                        // To number
                        uint? u = sb.ToString().ToUInt32();
                        if (u == null) throw new Exception("Invalid number literal!");
                        else tokens.Add(new Token(TokenType.NumberLiteral, (uint)u, line));
                    }
                    else if (c == '$')
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("0x");

                        while (sr.Peek() != EOF)
                        {
                            c = (char)sr.Peek();
                            if (c.IsHexDigit())
                            {
                                sb.Append(c);
                                sr.Read();
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                break;
                            }
                            else
                            {
                                throw new Exception("Invalid hexadecimal number literal!");
                            }
                        }

                        // To number
                        uint? u = sb.ToString().ToUInt32();
                        if (u == null) throw new Exception("Invalid number literal!");
                        else tokens.Add(new Token(TokenType.NumberLiteral, (uint)u, line));
                    }
                    else if (c == '#' || c == ':' || c == '@' || char.IsLetter(c))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(c);

                        while (sr.Peek() != EOF)
                        {
                            c = (char)sr.Peek();
                            if (char.IsLetterOrDigit(c) || c == '_')
                            {
                                sb.Append(c);
                                sr.Read();
                            }
                            else break;
                        }

                        //result.Add(new Token(sb.ToString(), line));
                        if (sb[0] == '#') tokens.Add(new Token(TokenType.Directive, sb.ToString(), line));
                        else if (sb[0] == ':') tokens.Add(new Token(TokenType.Label, sb.ToString(), line));
                        else if (sb[0] == '@') tokens.Add(new Token(TokenType.Label2, sb.ToString(), line));
                        else tokens.Add(new Token(TokenType.Command, sb.ToString(), line));
                    }
                    else // Characters with no impact of tokenizing
                    {
                        switch (c)
                        {
                            case '{':
                                tokens.Add(new Token(TokenType.LeftCurlyBrace, line));
                                break;
                            case '}':
                                tokens.Add(new Token(TokenType.RightCurlyBrace, line));
                                break;

                            // Conditions
                            case '!':
                                if (sr.Peek() == '=')
                                {
                                    tokens.Add(new Token(TokenType.NotEqualTo, line));
                                    sr.Read();
                                }
                                else throw new Exception(string.Format("Unexpected character '{0}'!", c));
                                break;

                            case '<':
                                if (sr.Peek() == '>')
                                {
                                    tokens.Add(new Token(TokenType.NotEqualTo, line));
                                    sr.Read();
                                }
                                else if (sr.Peek() == '=')
                                {
                                    tokens.Add(new Token(TokenType.LessThanOrEqualTo, line));
                                    sr.Read();
                                }
                                else
                                {
                                    tokens.Add(new Token(TokenType.LessThan, line));
                                }
                                break;


                            default:
                                throw new Exception(string.Format("Unexpected character '{0}'!", c));
                        }
                    }
                }

                // If a conjoin command was the last used, then error.
                if (ignoreNewLine)
                {
                    throw new Exception("Unfinished conjoin operation!");
                }
            }

            // Split it up into lines
            // A fairly simple
            List<Line> lines = new List<Line>();

            List<Token> line1 = new List<Token>();
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.Type == TokenType.NewLine)
                {
                    // Add the NewLine?
                    // Nah...
                    if (line1.Count > 0)
                    {
                        lines.Add(new Line(line1.ToArray()));
                        line1.Clear();
                    }
                }
                else
                {
                    line1.Add(token);
                }
            }

            return lines.ToArray();
        }

        private class Token
        {
            // placeholder for future
            //public static Token Error = new Token(0);

            public TokenType Type;
            public object Value;
            public int Line;

            // null
            public Token()
            {
                Type = TokenType.None;
                Value = "";
                Line = -1;
            }

            public Token(TokenType type, int line)
            {
                Type = type;
                Value = "";
                Line = line;
            }

            // Characters are handled by token type

            // Statements
            public Token(TokenType type, string s, int line)
            {
                Type = type;
                Value = s;
                Line = line;
            }

            // String literal
            public Token(TokenType type, StringBuilder sb, int line)
            {
                Type = type;
                Value = sb;
                Line = line;
            }

            // Number literal
            public Token(TokenType type, uint i, int line)
            {
                Type = type;
                Value = i;
                Line = line;
            }

            public override string ToString()
            {
                if (Value != null) return Type.ToString() + " ~ " + Value.ToString();
                else return "~";
            }
        }

        private enum TokenType
        {
            None,
            Directive, // #...
            Label, // :...
            Label2, // @...
            Command, // ...
            StringLiteral, // = ...
            InlineStringLiteral, // "..."
            InlineStringLiteral2, // <...>
            NumberLiteral, // 

            NewLine, // \n
            LeftCurlyBrace, // {
            RightCurlyBrace, // }

            NotEqualTo, // !=, <>
            EqualTo, // ==
            LessThan, // <
            LessThanOrEqualTo, // <=
            GreaterThan, // >
            GreaterThanOrEqualTo, // >=
        }

        // A helper class
        // Provides some stuff for reading too
        private class Line
        {
            public Token[] Tokens;
            public int Number;
            public bool Processed;

            private int pos = 0;

            public Line(Token[] tokens)
            {
                Tokens = tokens;
                Number = tokens.First().Line;
                Processed = false;
            }

            public Token this[int index]
            {
                get { return Tokens[index]; }
            }

            public Token First()
            {
                return Tokens.First();
            }

            // Make it act like a "reader"

            public Token Next()
            {
                if (pos >= Tokens.Length) return null;
                else
                {
                    var t = Tokens[pos];
                    pos++;
                    return t;
                }
            }

            public Token Peek()
            {
                if (pos >= Tokens.Length) return null;
                else return Tokens[pos];
            }

            public bool Expect(TokenType type)
            {
                if (pos >= Tokens.Length) return false;
                else return Tokens[pos].Type == type;
            }

            public int TokenPosition
            {
                get { return pos; }
                set { pos = value; }
            }

            public int Length
            {
                get { return Tokens.Length; }
            }

            public override string ToString()
            {
                string s = Number + ": ";
                for (int i = 0; i < Tokens.Length; i++)
                {
                    s += Tokens[i].ToString();
                    if (i < Tokens.Length - 1) s += ", ";
                }
                return s;
            }
        }
        #endregion

        // ----------------------------------------------------------
        // Preprocessor
        // ----------------------------------------------------------
        private void Preprocess(Line[] lines)
        {
            // First, do the directives that can only happen at the top
            // This includes things like #dynamic, #freespace, #mode, #include, etc.
        }

        /*
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

        */
        // This is going to be super light-weight compared to the rest of this thing.
        // It is bare-bones string processing.
        // Because all we need to do is find '#define' commands and trim comments.
        // I'm not exactly sure how #include is intended to work right now...
        public void IncludeFile(string filePath)
        {
            // Break up the file
            #region Simple Lexer
            List<string> lines = new List<string>();

            StringBuilder sb = new StringBuilder();
            bool singleLineComment = false, multiLineComment = false;

            using (StreamReader sr = File.OpenText(filePath))
                while (sr.Peek() != -1)
                {
                    char c = (char)sr.Read();
                    if (c == '\n')
                    {
                        string line = sb.ToString().Trim();
                        if (line.Length > 0) lines.Add(line);
                        sb.Clear();

                        if (singleLineComment) singleLineComment = false;
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
            #endregion

            // Now comprehend each line
            // TODO
        }

        /*

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

        */
    }
}
