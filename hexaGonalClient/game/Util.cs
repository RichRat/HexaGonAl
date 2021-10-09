using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace hexaGonalClient.game
{
    class Util
    {
        public static Color ChangeColorBrightness(Color color, double correctionFactor)
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
    }
}
