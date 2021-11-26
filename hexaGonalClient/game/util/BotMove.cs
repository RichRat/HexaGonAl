using hexaGoNal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hexaGonalClient.game.util
{
    class BotMove
    {
        Player player;
        Coords pos;
        BotMove parent;

        public List<BotMove> Children { get; set; }

        public int Score { get; set; }
        public int Value { get; set; }

        public BotMove(Coords c, Player p ,BotMove parent = null)
        {
            pos = c;
            this.parent = parent;
            player = p;
        }

        public bool IsRoot => parent == null;

    }
}
