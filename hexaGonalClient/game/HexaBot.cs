using hexaGoNal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Player Player { get; set; }

        private static readonly int pCloudRadius = 3;

        // directions ordered for clockwise movement
        private static readonly Coords[] CCWdirections = {
            new Coords(-1, 1),
            new Coords(-1, 0),
            new Coords(0, -1),
            new Coords(1, -1),
            new Coords(1, 0),
            new Coords(0, 1),
        };

        private static readonly Coords[] directionAxis =
        {
            new Coords(1, 0),
            new Coords(0, 1),
            new Coords(-1, 1)
        };

        public void nextGame()
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
            //if bot has first move just place at 0 0
            if (lastEnemyPoint == null)
                return new Coords(0, 0);

            //TODO play single forced movce


            //TODO play multiple forced moves
            //TODO play all moves which score highly and some that don't
            //TODO setup virutal pointcloud and points in a tree structure of moves
            //TODO return highest scoring tree branch
            //TODO increase depth
            //TODO prune bad branches

            //---- writeup of possible moves: -----
            //TODO search for abstractions

            //# forced defensive moves need to be hardcoded! since failure to comply = loss

            //## forced moves single result

            //## forced moves multiple options - run both but score depening on axis score or counterattack


            //# Winning moves - hardcode end of branch here

            //## winning by placing 5th point


            //# attacking - score pointcloud and select highest scoring n points

            //## creating open 4 row (checkmate)

            //## creating open 3 row 

            //## creating gapped attack

            //### 5 with gap (-1 point: xx_xx or x_xxx)

            //### 4 with gap

            //## creating half open 4 row (force enemy keep initiative)


            //# setup - score pointcloud and select highest scoring n points

            //## creat axis with distance 0 - 3 (axis = direct unblocked line between points)

            //## create half open axis with distance 0-2 (lowest scoring move, mostly intended for used in blocking and setting up half open 4 attacks)

            //## offensive block with open axis without any firendly dots

            //## random dot (if everything else fails create random dot withing pointspace)
            return pointCloud.ElementAt(rand.Next(0, pointCloud.Count)).Key;
        }


        //TODO write general method for other uses
        private List<Coords> CheckWinMove(Player p, Coords point)
        {
            List<Coords> ret = new();
            foreach (Coords dir in directionAxis)
            {
                int streak = 0;
                int streakStart = -1;
                int streakEnd = -1;
                Coords[] row = GetCheckRows(point, dir, 4);
                for (int i = 0; i < row.Length; i++)
                {
                    Coords c = row[i];
                    bool s = points.ContainsKey(c) && points[c] == p;
                    if (s && streakStart == -1)
                        streak = i;

                    if (s)
                        streak++;


                    //TODO for this case (range5) sum amount of pieces in a 5 distance where whin condition is 4pieces and 1 empty. if a enemy piece is in the range it is dead 
                }
            }

            return ret;
        }

        private Coords[] GetCheckRows(Coords pos, Coords direction, int radius)
        {
            Coords[] ret = new Coords[9];
            for (int i = -radius; i <= radius; i++)
            {
                ret[i + 4] = pos + (direction * i);
            }

            return ret;
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
                        if (!pointCloud.ContainsKey(pointer))
                            pointCloud.Add(pointer, 0);
                    }
                }
            }
        }

        public List<Coords> getCloud()
        {
            return pointCloud.Select(kvp => kvp.Key).ToList();
        }

    }
}
