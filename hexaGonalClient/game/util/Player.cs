using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace hexaGonalClient.game.util
{
    public class Player
    {

        public Player(Color col, string name)
        {
            Color = col;
            Name = name;
            Score = 0;
        }

        public string Name { get; set; }

        public Color Color { get; set; }

        public int Score { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Brush Brush { get => new SolidColorBrush(Color); }
    }
}
