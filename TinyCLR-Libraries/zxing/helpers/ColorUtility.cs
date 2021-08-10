using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

namespace ZXing {
    public static class ColorUtility {
        public static Color ColorFromRGB(byte r, byte g, byte b) {
            return Color.FromArgb(((b << 16) | (g << 8) | r));
        }

        public static byte GetRValue(Color color) {
            return color.R;
        }

        public static byte GetGValue(Color color) {
            return color.G;
        }

        public static byte GetBValue(Color color) {
            return color.B;
        }
    }
    /*
    public enum Color : uint {
        Black = 0x00000000,
        White = 0x00ffffff,
    }*/
}
