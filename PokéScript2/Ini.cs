using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PokéScript2
{
    // I'm taking this approach for .ini files.
    // It's very .Net.
    public class IniReader : IDisposable
    {
        private StreamReader sr;

        public IniReader(string file)
        {
            sr = File.OpenText(file);
        }

        public void Dispose()
        {
            sr.Dispose();
        }

        public void Close()
        {
            sr.Close();
        }

        public Dictionary<string, string> ReadSection(string section)
        {
            // Find the section
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            string sectionName = "[" + section + "]";
            string sectionEndName = "[" + section + "]";

            //
            bool found = false;
            while (!sr.EndOfStream)
            {
                // Read line
                string line = sr.ReadLine().Trim();

                // Skip comments
                if (line.StartsWith("#")) continue;

                // Check line
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (line == sectionName)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found) throw new Exception(string.Format("Section '[{0}]' does not exist!", section));

            // Now, read the entire section
            // This means go until [/SECTION] or a new section begins
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            while (!sr.EndOfStream)
            {
                // Read line
                string line = sr.ReadLine();//.Trim();

                // Skip comments
                if (line.StartsWith("#")) continue;

                // Check for exit/new section
                if (line.StartsWith("[") && line.EndsWith("]")) break;

                int index = line.IndexOf('=');
                if (index == -1) continue;
                else
                {
                    pairs[line.Substring(0, index)] = line.Substring(index + 1);
                }
            }

            return pairs;
        }

        public StreamReader BaseStream
        {
            get { return sr; }
        }
    }

    public class IniWriter : IDisposable
    {
        private StreamWriter sw;

        public IniWriter(string filePath)
        {
            sw = File.CreateText(filePath);
        }

        public void Dispose()
        {
            sw.Dispose();
        }

        public void Close()
        {
            sw.Close();
        }

        public StreamWriter BaseStream
        {
            get { return sw; }
        }

        public void WriteSectionName(string section)
        {
            sw.WriteLine("[{0}]", section);
        }

        public void WriteSectionEndName(string section)
        {
            sw.WriteLine("[/{0}]", section);
        }

        public void WritePair(string key, string value)
        {
            sw.WriteLine("{0}={1}", key, value);
        }

        public void WriteComment(string comment)
        {
            sw.WriteLine("# " + comment);
        }

        public void WriteBlankLine()
        {
            sw.WriteLine();
        }
    }
}
