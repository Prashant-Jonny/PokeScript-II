using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PokéScript2
{
    public class BadPointerException : Exception
    {
        public BadPointerException(uint data, long offset)
            : base("Bad pointer encountered at 0x" + offset.ToString("X") + "!\n0x" + data.ToString("X8") + " is not a pointer!")
        { }

        public BadPointerException(uint data)
            : base("0x" + data.ToString("X8") + " cannot be converted to a pointer!")
        { }
    }

    public class GBABinaryReader : BinaryReader
    {
        // Simple constructor for a string ;)
        public GBABinaryReader(string filePath)
            : base(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        { }

        /// <summary>
        /// Reads a 4-byte pointer from the stream and advances the position by four bytes.
        /// </summary>
        /// <returns>The pointer, adjusted for the ROM memory bank.</returns>
        public uint ReadPointer()
        {
            // Read
            uint data = base.ReadUInt32();
            
            // Safety
            if (data < 0x08000000) throw new BadPointerException(data, base.BaseStream.Position - 4);

            // Return
            return data - 0x08000000;
        }

        /// <summary>
        /// Reads a string of fixed length from the stream.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <returns>The string.</returns>
        public string ReadString(int length)
        {
            // TODO: remove \0 characters
            return Encoding.UTF8.GetString(base.ReadBytes(length));
        }
    }

    public class GBABinaryWriter : BinaryWriter
    {
        public GBABinaryWriter(string filePath)
            : base(File.Open(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
        { }

        /// <summary>
        /// Writes a 4-byte pointer to the given ROM offset.
        /// </summary>
        /// <param name="offset">The ROM offset to point to.</param>
        public void WritePointer(uint offset)
        {
            // Safety
            if (offset >= 0x08000000) throw new BadPointerException(offset);

            // Write
            base.Write(offset + 0x08000000);
        }
    }
}
