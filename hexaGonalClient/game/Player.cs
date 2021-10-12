using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace hexaGoNal
{
    public class Player
    {
        private Color col;
        private String name;

        public Player(Color col, String name)
        {
            this.col = col;
            this.name = name;
            Score = 0;
        }

        public String Name => name;

        public Color Color => col;

        public int Score { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Brush Brush { get => new SolidColorBrush(Color); }
    }
}
