using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using hexaGonalClient.game;

namespace hexaGoNal.game
{

    class HexMatchGame : Canvas
    {
        private Dictionary<Coords, Dot> dots = new();
        private Canvas offCan = new();
        private Vector offset = new(0, 0);
        private List<Player> players = new();
        private int activePlayer;

        private bool isDrag = false;
        private Point? prevMousePos = null;

        private bool isPreview = true;
        private Coords prevCoords = null;
        private Dot prevDot = null;

        private double dotSpacing = 26;
        private double dotDiameter = 22;
        private static readonly double yAchisRad = Math.PI / 3;
        private Vector xAxsis = new(1, 0);
        private Vector yAxsis = new(Math.Cos(yAchisRad), Math.Sin(yAchisRad));

        private GameState state = GameState.Preview;

        public event EventHandler<Player> PlayerChanged;
        public enum GameState
        {
            Preview = 0,
            PlayerTransition = 1,
            GameTransition = 2,
            GameFinished = 0xF0
        }

        private static readonly Coords[] neighboursCoords = {
            new Coords(1, 0),
            new Coords(-1, 0),
            new Coords(1, -1),
            new Coords(0, -1),
            new Coords(-1, 1),
            new Coords(0, -1)
        };

        private static readonly Coords[] directionCoords =
        {
            new Coords(1, 0),
            new Coords(0, 1),
            new Coords(-1, 1)
        };

        public HexMatchGame()
        {
            this.ClipToBounds = true;
            this.Children.Add(offCan);
            this.Background = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));

            this.SizeChanged += RecalcOffCanPos;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.MouseLeave += OnMouseLeave;
            this.MouseMove += OnMouseMove;

            offCan.ClipToBounds = false;
            //offCan.Width = 25;
            //offCan.Height = 25;
            //offCan.Background = new SolidColorBrush(Colors.Orange);
            RecalcOffCanPos(null, null);
        }

        public void StartGame()
        {
            System.Console.WriteLine("Start Game");
            //clear up previous game
            //offCan.Children.Clear();
            offset.X = 0;
            offset.Y = 0;
            RecalcOffCanPos(null, null);

            players.Clear();
            //todo enable custom names and colors(?)
            players.Add(new Player(Colors.Orange, "Player 1"));
            players.Add(new Player(Colors.Cyan, "Player 2"));
            activePlayer = 0;
            PlayerChanged?.Invoke(this, players[activePlayer]);

            offCan.Children.Clear();
            isPreview = true;
            //state = GameState.Preview;
            dots.Clear();
        }

        private Vector CoordsToScreen(Coords c)
        {
            return xAxsis * c.X * dotSpacing + yAxsis * c.Y * dotSpacing;
        }

        /// <summary>
        /// Calculate Coordintes for a given screen position (relative to offCanv)
        /// <br/> that the result is a best estimate including a slight preference 
        /// for the previous result defined by the distance to the neighbouring coordinates
        /// <param name="pos">Position relative to offCanv</param>
        /// <returns>Coordinates in the game space</returns>
        private Coords ScreenToCoords(Vector pos)
        {

            //y axis is just y component of yAchsis vector 
            int y = (int)Math.Round(pos.Y / yAxsis.Y / dotSpacing);

            //now that y is known subtract the distance from pos to create a vector of only the x component
            Vector xPos = pos - (yAxsis * y * dotSpacing);
            int x = (int)Math.Round(xPos.X / xAxsis.X / dotSpacing);

            Coords estim = new(x, y);
            List<Coords> candidates = GetNeighbours(estim, true);

            Coords min = estim;
            double minDist = double.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                double dist = (pos - CoordsToScreen(candidates[i])).Length;

                //give the previous coords a bit of an advantage to avoid it feeling nervous
                if (candidates[i] == prevCoords)
                    dist *= 0.8;

                if (dist < minDist)
                {
                    minDist = dist;
                    min = candidates[i];
                }
            }

            return min;
        }

        private void RecalcOffCanPos(Object sender, EventArgs e)
        {
            Canvas.SetLeft(offCan, offset.X + ActualWidth / 2);
            Canvas.SetTop(offCan, offset.Y + this.ActualHeight / 2);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag && prevMousePos != null)
            {
                //introduce smoothing here! 
                offset += e.GetPosition(this) - prevMousePos.Value;
                prevMousePos = e.GetPosition(this);
                RecalcOffCanPos(null, null);
            }

            if (isPreview)
            {
                if (prevDot == null)
                {
                    prevDot = new Dot(players[activePlayer], dotDiameter);
                    offCan.Children.Add(prevDot.Shape);
                }

                Coords lastPrevCoords = prevCoords;
                prevCoords = ScreenToCoords((Vector)e.GetPosition(offCan));
                if (prevCoords != lastPrevCoords)
                {
                    Vector dotOffset = CoordsToScreen(prevCoords);
                    Canvas.SetLeft(prevDot.Shape, dotOffset.X - dotDiameter / 2);
                    Canvas.SetTop(prevDot.Shape, dotOffset.Y - dotDiameter / 2);
                }

                if (dots.ContainsKey(prevCoords))
                    prevDot.Shape.Visibility = Visibility.Hidden;
                else if (prevDot.Shape.Visibility == Visibility.Hidden)
                    prevDot.Shape.Visibility = Visibility.Visible;
                
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDrag && e.RightButton == MouseButtonState.Pressed)
            {
                isDrag = true;
                prevMousePos = e.GetPosition(this);
            }

            if (isPreview && e.ChangedButton == MouseButton.Left && !dots.ContainsKey(prevCoords))
            {
                dots.Add(prevCoords ,prevDot);


                prevDot = null;
                

                List<Coords> winRow = CheckWin(prevCoords, players[activePlayer]);
                if (winRow.Count > 4)
                {
                    //TODO mark winning row
                    //TODO end game
                    //TODO announce win
                    Console.WriteLine("Winner: " + players[activePlayer].Name + " " + winRow);
                    isPreview = false;
                    return;

                    
                }

                activePlayer = ++activePlayer % players.Count;
                PlayerChanged?.Invoke(this, players[activePlayer]);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag && e.ChangedButton == MouseButton.Right && e.ButtonState == MouseButtonState.Released)
            {
                isDrag = false;
                prevMousePos = null;
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                isDrag = false;
                prevMousePos = null;
            }
        }



        private List<Coords> GetNeighbours(Coords center, bool includeCenter)
        {
            List<Coords> ret = new();
            if (includeCenter)
                ret.Add(center);

            foreach (Coords offset in neighboursCoords)
                ret.Add(center + offset);

            return ret;
        }

        private List<Coords> CheckWin(Coords pos, Player actPl)
        {
            List<Coords> ret = new();
            foreach (Coords direction in directionCoords)
            {
                Coords[] row = GetCheckRows(pos, direction);
                for (int i = 0; i < row.Length; i++)
                {
                    Coords checkPos = row[i];
                    if (!dots.ContainsKey(checkPos))
                    {
                        ret.Clear();
                        continue;
                    }

                    if (dots[checkPos].Player == actPl)
                    {
                        ret.Add(checkPos);
                        if (ret.Count > 4)
                            return ret;
                    }
                    else
                        ret.Clear();
                }
            }

            ret.Clear();
            return ret;
        }

        private Coords[] GetCheckRows(Coords pos, Coords direction)
        {
            Coords[] ret = new Coords[9];
            for (int i = -4; i < 5; i++)
            {
                ret[i + 4] = pos + (direction * i);
            }

            return ret;
        }
 
    }
}
