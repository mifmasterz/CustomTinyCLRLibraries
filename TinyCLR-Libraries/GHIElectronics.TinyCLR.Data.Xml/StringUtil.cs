using System;

namespace GHIElectronics.TinyCLR.Data.Xml
{
    internal abstract class Utility
    {

        internal static string ToHexDigits(uint val)
        {
            const string digits = "0123456789ABCDEF";

            var text = new char[2 * 4];
            var pos = 8;

            do
            {
                text[--pos] = digits[(int)(val & 0xF)];
                val = val >> 4;
            }

            while (val != 0);

            return new string(text, pos, 8 - pos);
        }
    }
}


