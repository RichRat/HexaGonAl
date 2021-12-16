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
    class BotVal
    {
        /// <summary>
        /// urgency score
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// strategic value
        /// </summary>
        public int StrategicValue { get; set; }

        public BotVal(int score, int value)
        {
            Score = score;
            StrategicValue = value;
        }

        /// <summary>
        /// creates zero value instance, identical to new BotMoveVal(0, 0)
        /// </summary>
        public BotVal()
        {
            Score = 0;
            StrategicValue = 0;
        }

        public static BotVal operator +(BotVal a, BotVal b)
        {
            return new BotVal(a.Score + b.Score, a.StrategicValue + b.StrategicValue);
        }

        public static BotVal operator -(BotVal a, BotVal b)
        {
            return new BotVal(a.Score - b.Score, a.StrategicValue - b.StrategicValue);
        }

        public static BotVal operator -(BotVal bv)
        {
            return new BotVal(-bv.Score, -bv.StrategicValue);
        }

        /// <summary>
        /// check value for being positive in either score or value
        /// </summary>
        /// <returns>true if either score or value are greater than zero</returns>
        public bool IsPositive()
        {
            return Score > 0 || StrategicValue > 0;
        }

        public override string ToString()
        {
            return "(" + Score + ", " + StrategicValue + ")";
        }

        public BotVal Clone()
        {
            return new(Score, StrategicValue);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not BotVal)
                return false;

            BotVal bmv = obj as BotVal;
            return bmv.Score == Score && bmv.StrategicValue == StrategicValue;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
