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

        public BotVal Val { get; set; }
        public Coords Position { get => pos; }

        public BotMove(Coords c, Player p)
        {
            pos = c;
            player = p;
        }

        public BotMove(Coords c, Player p, BotVal val, BotMove parent) : this(c, p)
        {
            Val = val;
            this.parent = parent;
        }

        public bool IsRoot => parent == null;

        public BotVal ValueOfSubtree(Player p)
        {
            BotVal val = Val.Clone();

            //opponent val is subtracted from the overal subtree value
            if (p != player)
                val = -val;

            foreach (BotMove m in Children)
                val += m.ValueOfSubtree(p);

            return val;
        }

        public void AddChild(BotMove child)
        {
            if (Children == null)
                Children = new();

            Children.Add(child);
        }
    }
}