﻿using System;
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
        private static readonly Regex regSelVals = new(@"[x0-2]");
        private static readonly Regex isNum = new(@"\d+");

        private int[] pattern;
        private int score;
        private int stratValue;
        private int offset;

        public bool IsBreak { get; }

        public BotLutEntry(bool isBreak = true)
        {
            IsBreak = isBreak;
        }

        public BotLutEntry(string input, int score, int stratValue)
        {
            this.score = score;
            this.stratValue = stratValue;
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

        //TODO check function which returns a score if matching

        public override string ToString()
        {
            if (IsBreak)
                return "!break";

            String ret = "";
            foreach (int b in pattern)
                ret += b + " ";

            return ret + " (" + score + ", " + stratValue + ")";
                
        }

        public int Check(int[] check)
        {
            return IsMatch(check) ? score : 0;
        }

        public bool IsMatch(int[] check)
        {
            if (IsBreak)
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
            if (IsBreak)
                return this;

            BotLutEntry bl = new(false);
            bl.score = score;
            bl.stratValue = stratValue;
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

            return score == bl.score && stratValue == bl.stratValue && pattern.SequenceEqual(bl.pattern);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}