using System;

namespace SoftOne.Soe.Common.Util
{
    public static class GraphicsUtil
    {
        public struct RGB
        {
            private byte _r;
            private byte _g;
            private byte _b;

            public RGB(byte r, byte g, byte b)
            {
                this._r = r;
                this._g = g;
                this._b = b;
            }

            public byte R
            {
                get { return this._r; }
                set { this._r = value; }
            }

            public byte G
            {
                get { return this._g; }
                set { this._g = value; }
            }

            public byte B
            {
                get { return this._b; }
                set { this._b = value; }
            }

            public bool Equals(RGB rgb)
            {
                return (this.R == rgb.R) && (this.G == rgb.G) && (this.B == rgb.B);
            }
        }

        public static RGB HexToRGB(string hex)
        {
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");

            byte r = (byte)HexToDec(hex.Substring(0, 2));
            byte g = (byte)HexToDec(hex.Substring(2, 2));
            byte b = (byte)HexToDec(hex.Substring(4, 2));

            return new RGB(r, g, b);
        }

        private static int HexToDec(string hex)
        {
            hex = hex.ToUpper();

            int hexLength = hex.Length;
            double dec = 0;

            for (int i = 0; i < hexLength; ++i)
            {
                byte b = (byte)hex[i];

                if (b >= 48 && b <= 57)
                    b -= 48;
                else if (b >= 65 && b <= 70)
                    b -= 55;

                dec += b * Math.Pow(16, ((hexLength - i) - 1));
            }

            return (int)dec;
        }

        public static string ForegroundColorByBackgroundBrightness(string backgroundColor)
        {
            if (String.IsNullOrEmpty(backgroundColor))
                backgroundColor = "#FFFFFF";

            RGB rgb = HexToRGB(backgroundColor);
            decimal o = Math.Round(((rgb.R * 299) + (rgb.G * 587) + (rgb.B * 114)) / 1000M);

            return o > 125 ? "#000000" : "#FFFFFF";
        }

        public static string Opacitate(string color, decimal opacity)
        {
            RGB rgb = HexToRGB(RemoveAlphaValue(color));
            rgb.R = Convert.ToByte((1 - opacity) * 255 + opacity * rgb.R);
            rgb.G = Convert.ToByte((1 - opacity) * 255 + opacity * rgb.G);
            rgb.B = Convert.ToByte((1 - opacity) * 255 + opacity * rgb.B);
            return ToHexString(rgb);
        }

        public static string ToHexString(this RGB c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        public static string ToRgbString(this RGB c) => $"RGB({c.R}, {c.G}, {c.B})";

        public static string RemoveAlphaValue(string color, string defaultColor = "#FFFFFF")
        {
            // Remove alpha values from color
            if (!string.IsNullOrEmpty(color) && color.Length == 9)
                color = "#" + color.Substring(3);
            else if (string.IsNullOrEmpty(color))
                color = defaultColor;
            return color;
        }
    }
}
