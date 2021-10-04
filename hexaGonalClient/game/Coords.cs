using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hexaGonalClient.game
{
    public class Coords
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coords(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Coords c)
        {
            return c != null && X == c.X && Y == c.Y;
        }

        public override string ToString()
        {
            return X + ";" + Y;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
