using hexaGonalClient.game.bot;
using hexaGonalClient.game.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace hexaGonalClient.game
{
    partial class HexaBot
    {
        private static readonly int BOT_EASY_BAG = 5;

        // posibilities for the bot
        private Dictionary<Coords, BotVal> cloud = new();
        // currently placed dots
        private Dictionary<Coords, Player> points = new();
        //private Coords lastEnemyPoint = null;
        private Random rand = new();
        private List<BotLutEntry> scoreLookup = new();
        private List<BotLutEntry> comboLookup = new();

        public Player Player { get; set; }

        private static readonly int pCloudRadius = 3;
        private static readonly int checkRadius = 4;
        private static readonly int checkDiam = checkRadius * 2 + 1;


        // directions ordered for clockwise movement
        private static readonly Coords[] CCWdirections = {
            new(-1, 1),
            new(-1, 0),
            new(0, -1),
            new(1, -1),
            new(1, 0),
            new(0, 1)
        };

        private static readonly Coords[] directionAxis =
        {
            new(1, 0),
            new(0, 1),
            new(-1, 1)
        };

        public Difficulty Difficulty { get; set; }
        public List<Player> Players { get; internal set; }

        private readonly Coords[,] rowDefinition = new Coords[directionAxis.Length, checkDiam]; 


        public HexaBot()
        {
            InitRowCoords();
            LoadLut();
            Difficulty = Difficulty.Pro;
        }

        public void Clear()
        {
            cloud.Clear();
            points.Clear();
        }

        public void AddCoord(Coords c, Player p)
        {
            points.Add(c, p);
            //remove point from possibilities
            cloud.Remove(c);

            //add new posibilities for the new coord
            GeneratePointCloud(c, pCloudRadius);
        }

        public Coords CalcTurn(Player activePlayer)
        {
            Player = activePlayer;
            Difficulty = activePlayer.Difficulty;

            //if bot has first move just place at 0 0
            if (points.Count == 0)
                return new Coords(0, 0);

            Stopwatch sw = new();
            sw.Start();

            Coords ret = null;

            if (Difficulty < Difficulty.Master)
            {
                ScoreMoves(Player);
                List<Coords> bestMoves;
                double randFactor = 0;
                switch (Difficulty)
                {
                    case Difficulty.Weak: randFactor = 0.3; break;
                    case Difficulty.Advanced: randFactor = 0.15; break;
                    case Difficulty.Strong: randFactor = 0.05; break;
                }

                bestMoves = randFactor > 0 ? GetMovesEasy(randFactor) : GetBestMoves();

                if (bestMoves.Count > 0)
                    ret = bestMoves[rand.Next(0, bestMoves.Count)];
            }
            else if (Difficulty == Difficulty.Master)
            {
                BotMove root = new(null, null);
                GenMoveTree(root, Player);
                List<BotMove> bestMoves = new();
                BotVal max = new();
                foreach (BotMove bm in root.Children)
                {
                    if (bm.Val.StrategicValue > max.StrategicValue)
                    {
                        bestMoves.Clear();
                        max = bm.Val;
                        bestMoves.Add(bm);
                    }
                    else if (bm.Val.StrategicValue == max.StrategicValue)
                        bestMoves.Add(bm);

                    if (bestMoves.Count > 0)
                        ret = bestMoves[rand.Next(0, bestMoves.Count)].Position;
                }
            }

            Console.WriteLine("HexaBot Eval in " + sw.ElapsedMilliseconds + "ms");
            return ret;
        }

        int moveTreeDepth = 1;

        private void GenMoveTree(BotMove bm, Player p, int depth = 0)
        {
            ScoreMoves(Player);
            foreach (Coords cMov in GetTopMoves(5))
            {
                BotMove move = new(cMov, p, cloud[cMov], bm);
                bm.addChild(move);
                points.Add(cMov, p);
                cloud.Remove(cMov);
                List<Coords> addCloud = GeneratePointCloud(cMov, pCloudRadius);

                if (depth < moveTreeDepth)
                    GenMoveTree(move, p, depth + 1);

                points.Remove(cMov);
                cloud.Add(cMov, new());
                foreach (Coords co in addCloud)
                    cloud.Remove(co);
            }
        }

        private List<Coords> GetBestMoves()
        {
            List<Coords> bestMoves = new();
            int score = 0;
            foreach (KeyValuePair<Coords, BotVal> v in cloud)
            {
                if (v.Value.Score > score)
                {
                    bestMoves.Clear();
                    bestMoves.Add(v.Key);
                    score = v.Value.Score;
                }
                else if (v.Value.Score == score)
                    bestMoves.Add(v.Key);
            }

            return bestMoves;
        }

        private List<Coords> GetMovesEasy(double spreadFactor)
        {
            // very easy sometimes makes mistakes so skip best moves and continue for those
            bool mistake = Difficulty == Difficulty.Weak && rand.NextDouble() > 0.9;
            // moves above 10k are forced since it might make the bot too easy if it forgets to defend at all.
            if (cloud.Max(kvp => kvp.Value.Score) > 10000 && !mistake) 
                return GetBestMoves();

            var moves = cloud.ToList();
            int take = (int)(moves.Count * spreadFactor);
            if (take <= 0)
                take = 1;

            moves.Sort((a, b) => b.Value.Score.CompareTo(a.Value.Score)); // sort desc
            var ret = (from m in moves.Take(take) select m.Key).ToList();
            Console.WriteLine("bot range " + ret.Count);
            return ret;
        }

        private List<Coords> GetTopMoves(int n)
        {
            List<Coords> topMoves = new();
            var l = cloud.ToList();
            l.Sort((a, b) => a.Value.Score > b.Value.Score ? 1 : a.Value.Score == b.Value.Score ? 0 : -1);
            n = n > l.Count ? l.Count : n;
            return l.GetRange(0, n).Select(kvp => kvp.Key).ToList();
        }


        private void ScoreMoves(Player p)
        {
            //instead of creating a new array each iteration uses this buffer
            int[] buffer = new int[checkDiam];
            foreach (Coords point in cloud.Keys)
            {
                BotVal pointScore = new();
                int[] comboHit = new int[comboLookup.Count];
                for (int i = 0; i < directionAxis.Length; i++)
                    pointScore += ScoreRow(GetRow(buffer, p, point, i), comboHit);
                
                for (int i = 0; i < comboHit.Length; i++)
                {
                    if (comboHit[i] > 1)
                    {
                        Console.WriteLine("combo hit!");
                        pointScore += comboLookup.ElementAt(i).Value;
                    }
                }

                cloud[point] = pointScore;
            }
        }

        private int[] GetRow(int[] buffer, Player p, Coords point, int direction)
        {
            //int[] ret = new int[checkDiam];
            for (int i = 0; i < checkDiam; i++)
            {
                if (i == checkRadius)
                {
                    buffer[i] = BotLutEntry.POS;
                    continue;
                }

                Coords c = rowDefinition[direction, i] + point;

                if (points.ContainsKey(c))
                    buffer[i] = points[c] == p ? 1 : 2;
                else
                    buffer[i] = 0;
            }

            return buffer;
        }

        private List<Coords> GeneratePointCloud(Coords center, int depth)
        {
            List<Coords> ret = new();
            for (int d = 1; d <= depth; d++)
            {
                Coords pointer = center + new Coords(d, 0);
                foreach (Coords dir in CCWdirections)
                {
                    for (int i = 0; i < d; i++)
                    {
                        pointer += dir;
                        if (!cloud.ContainsKey(pointer) && !points.ContainsKey(pointer))
                        {
                            cloud.Add(pointer, new());
                            ret.Add(pointer);
                        }
                            
                    }
                }
            }

            return ret;
        }

        public Dictionary<Coords, BotVal> getCloud()
        {
            return cloud;
        }

        private BotVal ScoreRow(int[] row, int[] comboHit)
        {
            BotVal score = new();
            bool combo = false;
            int comboId = -1;


            foreach (BotLutEntry bl in scoreLookup)
            {
                // idk why i am checking for positive here
                if (score.IsPositive() && bl.IsBreak)
                    break;

                if (bl.IsComboStart)
                {
                    combo = true;
                    comboId = bl.ComboIndex;
                }
                else if (bl.IsComboEND)
                    combo = false;
                
                if (!combo)
                    score += bl.Check(row);
                else if (bl.IsMatch(row) && Difficulty > Difficulty.Strong)
                    comboHit[comboId]++;
            }

            return score;
        }

        /// <summary>
        /// load bot lookup table which contains patterns for single row evaluation
        /// </summary>
        public void LoadLut()
        {
            Regex reg = new(@"^\s*[!$012x]");
            Regex num = new(@"[+-]{0,1}\d+");
            Regex varScore = new(@"\$s = ");
            Regex varValue = new(@"\$v = ");
            Regex flag = new(@"!\w+");
            Regex remLeadingWhitespace = new(@"^\s+(.*)$");

            int score = 0;
            int stratValue = 0; //TODO introduce startegic value to botlutentry
            int comboId = 0;
            foreach (string _line in Properties.Resources.botConfig.Split('\n'))
            {
                if (string.IsNullOrEmpty(_line) || _line.Length < 2 || !reg.IsMatch(_line))
                    continue;

                string line = _line.Trim();

                BotLutEntry ble = null;
                char s = line[0];
                switch (s)
                {
                    case '$':
                        Match m = num.Match(line);
                        if (m.Success)
                        {
                            int val = int.Parse(m.Groups[0].Value);
                            if (varScore.IsMatch(line))
                                score = val;
                            else if (varValue.IsMatch(line))
                                stratValue = val;
                        }

                        break;
                    case '!':
                        ble = new(line);
                        if (line.Contains("!end"))
                            ble.Value = new(score, stratValue);

                        scoreLookup.Add(ble);

                        if (ble.IsComboStart)
                        {
                            ble.ComboIndex = comboId;
                            ble.Value = new BotVal(score, stratValue);
                            comboLookup.Add(ble);
                            comboId++;
                        }

                        if (ble.IsComboEND || ble.IsComboStart)
                            Console.WriteLine(line);

                        break;
                    case '0':
                    case '1':
                    case '2':
                    case 'x':
                        ble = new(line, score, stratValue);
                        Console.WriteLine(ble);
                        scoreLookup.Add(ble);
                        BotLutEntry bleMir = ble.Mirror();
                        if (!ble.Equals(bleMir))
                        {
                            Console.WriteLine(bleMir);
                            scoreLookup.Add(bleMir);
                        }
                        break;
                    default:
                        break;
                }
            }

        }

        private void InitRowCoords()
        {
            for (int k = 0; k < directionAxis.Length; k++)
            {
                for (int i = -checkRadius; i <= checkRadius; i++)
                {
                    rowDefinition[k, i + checkRadius] = directionAxis[k] * i;
                }
            }
        }
    }
}
