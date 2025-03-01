using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace hexaGonalClient.game.bot
{
    class BotLutEntry
    {
        public static readonly int POS = 0xFF;
        private static readonly Regex regSelVals = new(@"[x0-2]");
        private static readonly Regex isNum = new(@"\d+");

        private int[] pattern;
        public BotVal Value { get; set; }
        private int offset;
        public int ComboIndex { get; set; }
        public bool IsBreak { get; }

        public bool IsComboStart { get; set; }
        public bool IsComboEND { get; set; }

        public enum ComboType
        {
            Defensive = 0
        }

        public BotLutEntry(string flag = "")
        {
            string flag_val = null;
            int delim = flag.IndexOf(' ');

            if (delim > 0)
            {
                string tmp = flag.Substring(0, delim);
                flag_val = flag.Substring(delim + 1);
                flag = tmp;
            }

            switch (flag)
            {
                case "!break": IsBreak = true; break;
                case "!combo": IsComboStart = true; break;
                case "!endcombo": IsComboEND = true; break;

                default:
                    break;
            }
        }

        public BotLutEntry(string input, int score, int stratValue)
        {
            Value = new(score, stratValue);
            IsBreak = false;

            if (input == null)
                return;

            MatchCollection matches = regSelVals.Matches(input);
            pattern = new int[matches.Count];
            int i = 0;
            foreach (Match mat in matches)
            {
                if (isNum.IsMatch(mat.Value))
                    pattern[i++] = int.Parse(mat.Value);
                else
                {
                    pattern[i] = POS;
                    offset = i;
                    i++;
                }
            }
        }

        public override string ToString()
        {
            if (IsBreak)
                return "!break";
            else if (IsComboStart)
                return "!combo";
            else if (IsComboEND)
                return "!endcombo";

            string ret = "";
            foreach (int b in pattern)
                ret += b + " ";

            return ret + " " + Value;

        }

        public BotVal Check(int[] check)
        {
            return IsMatch(check) ? Value : BotVal.ZERO;
        }

        public bool IsMatch(int[] check)
        {
            if (IsBreak || IsComboStart || IsComboEND)
                return false;

            for (int i = 0; i < pattern.Length; i++)
            {
                if (check[i + 4 - offset] != pattern[i])
                    return false;
            }

            return true;
        }

        public BotLutEntry Mirror()
        {
            if (IsBreak || IsComboStart || IsComboEND)
                return this;

            BotLutEntry bl = new();
            bl.Value = Value.Clone();
            bl.pattern = new int[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
                bl.pattern[i] = pattern[pattern.Length - 1 - i];

            bl.offset = pattern.Length - 1 - offset;

            return bl;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not BotLutEntry)
                return false;

            BotLutEntry bl = obj as BotLutEntry;
            if (bl.IsBreak)
                return IsBreak == bl.IsBreak;

            return Value.Equals(bl.Value) && pattern.SequenceEqual(bl.pattern);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}