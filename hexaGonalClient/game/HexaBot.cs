﻿using hexaGoNal;
using hexaGonalClient.game.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hexaGonalClient.game
{
    class HexaBot
    {
        
        // posibilities for the bot
        private Dictionary<Coords, int> pointCloud = new();
        // currently placed dots
        private Dictionary<Coords, Player> points = new();
        private Coords lastEnemyPoint = null;
        private Random rand = new();
        private List<BotLutEntry> scoreLookup = new();

        public Player Player { get; set; }

        private static readonly int pCloudRadius = 3;
        private static readonly int checkRadius = 4;
        private static readonly int checkDiam = checkRadius * 2 + 1;

        // directions ordered for clockwise movement
        private static readonly Coords[] CCWdirections = {
            new Coords(-1, 1),
            new Coords(-1, 0),
            new Coords(0, -1),
            new Coords(1, -1),
            new Coords(1, 0),
            new Coords(0, 1)
        };

        private static readonly Coords[] directionAxis =
        {
            new Coords(1, 0),
            new Coords(0, 1),
            new Coords(-1, 1)
        };

        private readonly Coords[,] rowDefinition = new Coords[directionAxis.Length, checkDiam]; 


        public HexaBot()
        {
            InitRowCoords();
            LoadLut();
        }

        public void Clear()
        {
            pointCloud.Clear();
            points.Clear();
            lastEnemyPoint = null;
        }

        public void AddCoord(Coords c, Player p)
        {
            points.Add(c, p);
            //remove point from possibilities
            pointCloud.Remove(c);

            //add new posibilities for the new coord
            GeneratePointCloud(c, pCloudRadius);

            if (p != Player)
                lastEnemyPoint = c;
        }

        public Coords CalcTurn()
        {
            Stopwatch sw = new();
            sw.Start();
            //if bot has first move just place at 0 0
            if (lastEnemyPoint == null)
                return new Coords(0, 0);

            CalcBestMoves(Player);

            List<Coords> bestMoves = new();
            int score = 0;
            foreach (var v in pointCloud)
                if (v.Value > score)
                {
                    bestMoves.Clear();
                    bestMoves.Add(v.Key);
                    score = v.Value;
                }
                else if (v.Value == score)
                    bestMoves.Add(v.Key);

            Console.WriteLine("HexaBot Eval in " + sw.ElapsedMilliseconds + "ms");

            if (bestMoves.Count == 1)
                return bestMoves[0];
            else if (bestMoves.Count > 1)
                return bestMoves[rand.Next(0, bestMoves.Count - 1)];
            else
                return null;
        }


        private void CalcBestMoves(Player p)
        {
            //instead of creating a new array each iteration uses this buffer
            int[] buffer = new int[checkDiam];
            foreach (Coords point in pointCloud.Keys)
            {
                int pointScore = 0;
                for (int i = 0; i < directionAxis.Length; i++)
                    pointScore += ScoreRow(GetRow(buffer, p, point, i));

                pointCloud[point] = pointScore;
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

        private void GeneratePointCloud(Coords center, int depth)
        {
            for (int d = 1; d <= depth; d++)
            {
                Coords pointer = center + new Coords(d, 0);
                foreach (Coords dir in CCWdirections)
                {
                    for (int i = 0; i < d; i++)
                    {
                        pointer += dir;
                        if (!pointCloud.ContainsKey(pointer) && !points.ContainsKey(pointer))
                            pointCloud.Add(pointer, 0);
                    }
                }
            }
        }

        public Dictionary<Coords, int> getCloud()
        {
            return pointCloud;
        }

        private int ScoreRow(int[] row)
        {
            int score = 0;
            foreach (BotLutEntry bl in scoreLookup)
            {
                if (score > 0 && bl.IsBreak)
                    break;

                score += bl.Check(row);
            }

            return score;
        }

        public void LoadLut()
        {
            Regex reg = new(@"^[!$012x]");
            Regex num = new(@"[+-]{0,1}\d+");
            Regex varScore = new(@"\$s = ");
            Regex varValue = new(@"\$v = ");

            int score = 0;
            int stratValue = 0; //TODO introduce startegic value to botlutentry
            foreach (string line in Properties.Resources.botConfig.Split('\n'))
            {
                if (string.IsNullOrEmpty(line) || line.Length < 2 || !reg.IsMatch(line))
                    continue;

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
                        if (line.Contains("!break"))
                        {
                            scoreLookup.Add(new BotLutEntry());
                            Console.WriteLine("!break");
                        }
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case 'x':
                        BotLutEntry ble = new(line, score);
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
