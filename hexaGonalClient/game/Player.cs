using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace hexaGoNal
{
    class Player
    {
        private Color col;
        private String name;
        private int score = 0;

        public Player(Color col, String name)
        {
            this.col = col;
            this.name = name;
        }

        public String Name => name;

        public Color Color => col;

        public int GetScore()
        {
            return score;
        }

        public void IncScore()
        {
            score++;
        }
    }
}
