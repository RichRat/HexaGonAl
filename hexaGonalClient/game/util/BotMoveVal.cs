using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hexaGonalClient.game.util
{
    class BotMoveVal
    {
        public int Score { get; set; }
        public int Value { get; set; }

        public BotMoveVal(int score, int value)
        {
            Score = score;
            Value = value;
        }

        public BotMoveVal()
        {
            Score = 0;
            Value = 0;
        }

        public static BotMoveVal operator +(BotMoveVal a, BotMoveVal b)
        {
            return new BotMoveVal(a.Score + b.Score, a.Value + b.Value);
        }

        public bool IsGtZero()
        {
            return Score > 0 || Value > 0;
        }

        public override string ToString()
        {
            return "(" + Score + ", " + Value + ")";
        }

        public BotMoveVal Clone()
        {
            return new(Score, Value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not BotMoveVal)
                return false;

            BotMoveVal bmv = obj as BotMoveVal;
            return bmv.Score == Score && bmv.Value == Value;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
