using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PokéScript2
{
    public static class Extensions
    {
        public static uint? ConvertFormattedStringToUInt32(string s)
        {
            try
            {
                if (s.StartsWith("0b") || s.StartsWith("0B"))
                {
                    return Convert.ToUInt32(s.Substring(2), 2);
                }
                else if (s.StartsWith("$"))
                {
                    return Convert.ToUInt32(s.Substring(1), 10);
                }
                else if (s.StartsWith("0x") || s.StartsWith("0X") || s.StartsWith("&H") || s.StartsWith("&h"))
                {
                    return Convert.ToUInt32(s.Substring(2), 16);
                }
                else
                {
                    return Convert.ToUInt32(s, 10);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static uint? ToUInt32(this string s)
        {
            return ConvertFormattedStringToUInt32(s);
        }

        /*public static byte ToByte(this string s)
        {
            uint? u = ConvertFormattedStringToUInt32(s);

            if (u == null)
            {
                throw new Exception("Invalid number format!");
            }
            else
            {
                return (byte)u;
            }
        }

        public static ushort ToUInt16(this string s)
        {
            uint? u = ConvertFormattedStringToUInt32(s);

            if (u == null)
            {
                throw new Exception("Invalid number format!");
            }
            else
            {
                return (ushort)u;
            }
        }

        public static uint ToUInt32(this string s)
        {
            uint? u = ConvertFormattedStringToUInt32(s);

            if (u == null)
            {
                throw new Exception("Invalid number format!");
            }
            else
            {
                return (uint)u;
            }
        }*/

        /*public static string TrimComments(this string s)
        {
            string[] comments = { ";", "//", "'" };
        }*/

    }
}
