using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hexaGonalClient.game.util
{
    class BotLutEntry
    {
        public static readonly int POS = 0xFF;
        private static readonly Regex regSelVals = new(@"[v0-2]");
        private static readonly Regex isNum = new(@"\d+");

        private int[] vals;
        private int score;
        private int offset;

        public bool IsBreak { get; }

        public BotLutEntry(bool isBreak = true)
        {
            IsBreak = isBreak;
        }

        public BotLutEntry(String input, int score)
        {
            this.score = score;
            IsBreak = false;
            
            if (input == null)
                return;

            var ml = regSelVals.Matches(input);
            vals = new int[ml.Count];
            int i = 0;
            foreach (Match m in ml)
            {
                if (isNum.IsMatch(m.Value))
                    vals[i++] = int.Parse(m.Value);
                else
                {
                    vals[i] = POS;
                    offset = i;
                    i++;
                }
            }
        }

        //TODO check function which returns a score if matching

        public override string ToString()
        {
            if (IsBreak)
                return "!break";

            String ret = "";
            foreach (int b in vals)
                ret += b + " ";

            return ret;
                
        }

        public int Check(int[] check)
        {
            if (IsBreak)
                return 0;

            for (int i = 0; i < vals.Length; i++)
            {
                if (check[i + 4 - offset] != vals[i])
                    return 0;
            }

            return score;
        }

        public BotLutEntry Mirror()
        {
            if (IsBreak)
                return this;

            BotLutEntry bl = new(false);
            bl.score = score;
            bl.vals = new int[vals.Length];
            for (int i = 0; i < vals.Length; i++)
                bl.vals[i] = vals[vals.Length - 1 - i];

            bl.offset = vals.Length - 1 - offset;

            return bl;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not BotLutEntry)
                return false;

            BotLutEntry bl = obj as BotLutEntry;
            if (bl.IsBreak)
                return IsBreak == bl.IsBreak;

            return score == bl.score && vals.SequenceEqual(bl.vals);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}