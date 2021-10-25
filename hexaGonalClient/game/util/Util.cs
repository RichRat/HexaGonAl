using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace hexaGonalClient.game
{
    /// <summary>
    /// Util Class for game functionality
    /// </summary>
    internal class Util
    {
        /// <summary>
        /// mod color brightness
        /// </summary>
        /// <param name="color">input color</param>
        /// <param name="correctionFactor">brightness factor -1 to 1</param>
        /// <returns></returns>
        public static Color ModColBrightness(Color color, double correctionFactor)
        {
            double r = (double)color.R;
            double g = (double)color.G;
            double b = (double)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                r *= correctionFactor;
                g *= correctionFactor;
                b *= correctionFactor;
            }
            else
            {
                r = (255 - r) * correctionFactor + r;
                g = (255 - g) * correctionFactor + g;
                b = (255 - b) * correctionFactor + b;
            }

            return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
        }

        /// <summary>
        /// util for reading color from "r g b" format
        /// </summary>
        /// <param name="colStr">string input</param>
        /// <returns>resulting color or white on error</returns>
        public static Color ColFromStr(string colStr)
        {
            string[] x = colStr.Split(' ');
            if (x.Length != 3)
                return Colors.White;

            try
            {
                return Color.FromRgb(byte.Parse(x[0]), byte.Parse(x[1]), byte.Parse(x[2]));
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed parsing config color " + e);
                return Colors.White;
            }
        }

        /// <summary>
        /// util for creating "r g b" string from color
        /// </summary>
        /// <param name="c">input Color</param>
        /// <returns>generated string</returns>
        public static string StrFromColor(Color c)
        {
            return c.R + " " + c.G + " " + c.B;
        }
    }
}
