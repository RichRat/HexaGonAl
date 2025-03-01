using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hexaGonalClient.game.util
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

        public override bool Equals(object obj)
        {
            if (obj is Coords)
            {
                Coords b = (Coords)obj;
                return X == b.X && Y == b.Y;
            }

            return false;
        }

        public static bool operator ==(Coords a, Coords b)
        {
            if (a is null)
                return b is null;

            return a.Equals(b);
        }

        public static bool operator !=(Coords a, Coords b)
        {
            if (a is null)
                return b is not null;

            return !a.Equals(b);

        }

        public static Coords operator *(Coords a, int scalar)
        {
            if (a is null)
                return null;

            return new Coords(a.X * scalar, a.Y * scalar);
        }

        public override string ToString()
        {
            return X + ";" + Y;
        }

        public override int GetHashCode()
        {
            int hash = 4133;
            hash *= 2029 * X.GetHashCode();
            hash *= 5669 * Y.GetHashCode();

            return 1;
        }

        public static Coords operator +(Coords a, Coords b)
        {
            if (a == null || b == null)
                return a ?? b;

            return new Coords(a.X + b.X, a.Y + b.Y);
        }
    }
}
