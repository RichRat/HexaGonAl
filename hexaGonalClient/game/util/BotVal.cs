using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hexaGonalClient.game.util
{
    /// <summary>
    /// Two dimension integer class used for bot lookup tabl
    /// </summary>
    struct BotVal
    {
        /// <summary>
        /// urgency score
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// strategic value
        /// </summary>
        public int StrategicValue { get; set; }

        public bool ForcedFlag { get; set; }

        public BotVal(int score = 0, int value = 0, bool forced = false)
        {
            Score = score;
            StrategicValue = value;
            ForcedFlag = forced;
        }

        public static BotVal operator +(BotVal a, BotVal b)
        {
            return new BotVal(a.Score + b.Score, a.StrategicValue + b.StrategicValue, a.ForcedFlag || b.ForcedFlag);
        }

        public static BotVal operator -(BotVal a, BotVal b)
        {
            return new BotVal(a.Score - b.Score, a.StrategicValue - b.StrategicValue, a.ForcedFlag || b.ForcedFlag);
        }

        public static BotVal operator -(BotVal bv)
        {
            return new BotVal(-bv.Score, -bv.StrategicValue, bv.ForcedFlag);
        }

        public override string ToString()
        {
            return "(" + Score + ", " + StrategicValue + (ForcedFlag ? ", f" : "") +")";
        }

        public BotVal Clone()
        {
            return new(Score, StrategicValue, ForcedFlag);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not BotVal)
                return false;

            BotVal bmv = (BotVal)obj;
            return bmv.Score == Score && bmv.StrategicValue == StrategicValue && ForcedFlag == bmv.ForcedFlag;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Score, StrategicValue, ForcedFlag);
        }
    }
}
