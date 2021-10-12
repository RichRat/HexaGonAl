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
using System.Diagnostics;
using static hexaGoNal.Dot;
using static hexaGonalClient.game.Animator;
using hexaGonalClient;

namespace hexaGoNal.game
{

    class HexMatchGame : Canvas
    {
        private readonly Dictionary<Coords, Dot> dots = new();
        private readonly Canvas offCan = new();
        
        private readonly List<Player> players = new();
        private int activePlayer;

        private bool isDrag = false;
        ScreenScroller scroller;

        private bool isPreview = true;
        private Coords prevCoords;
        private Dot prevDot;

        private double dotSpacing = 26;
        private double dotDiameter = 22;
        private static readonly double yAchisRad = Math.PI / 3;
        private Vector xAxsis = new(1, 0);
        private Vector yAxsis = new(Math.Cos(yAchisRad), Math.Sin(yAchisRad));

        private GameState state = GameState.Preview;

        private readonly Animator animator;

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
            animator = new(this);
            scroller = new(offCan, this, animator);

            this.ClipToBounds = true;
            this.Children.Add(offCan);
            this.Background = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));
            
            
            this.SizeChanged += scroller.SetOffset;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.MouseLeave += OnMouseLeave;
            this.MouseMove += OnMouseMove;


            offCan.ClipToBounds = false;
            scroller.SetOffset(null, null);
        }

        public void StartGame()
        {
            System.Console.WriteLine("Start Game");
            //clear up previous game
            scroller.Offset = new Vector();
            scroller.SetOffset(null, null);

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

        private void NextRound()
        {
            //TODO move offset away form current game
            //TODO previous winner has to play first
            //TODO disable old dots from counting as a win or clear board entirely
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

        

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
                scroller.OnDrag(sender, e);

            if (isPreview)
            {
                if (prevDot == null)
                {
                    prevDot = new Dot(players[activePlayer], dotDiameter);
                    prevDot.State = DotState.PREVIEW;
                    offCan.Children.Add(prevDot.Shape);
                }

                Coords lastPrevCoords = prevCoords;
                prevCoords = ScreenToCoords((Vector)e.GetPosition(offCan));
                if (prevCoords != lastPrevCoords)
                {
                    Vector dotOffset = CoordsToScreen(prevCoords);
                    SetLeft(prevDot.Shape, dotOffset.X - dotDiameter / 2);
                    SetTop(prevDot.Shape, dotOffset.Y - dotDiameter / 2);
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
                scroller.StartDrag(sender, e);
            }

            if (isPreview && e.ChangedButton == MouseButton.Left && !dots.ContainsKey(prevCoords))
            {
                dots.Add(prevCoords ,prevDot);
                prevDot.State = DotState.DEFALUT;
                AnimatePlaceDot(prevDot, prevCoords);
                prevDot = null;
                
                List<Coords> winRow = CheckWin(prevCoords, players[activePlayer]);
                if (winRow.Count > 4)
                {
                    foreach (Coords c in winRow)
                        if (dots[c] != null)
                            dots[c].State = DotState.WIN;

                    //TODO end game
                    players[activePlayer].Score++;
                    Console.WriteLine("Winner: " + players[activePlayer].Name);
                    isPreview = false;
                    AnimateWin(winRow, players[activePlayer]);
                    // game state! 
                    //TODO game state which waitTODOs for next left click to continue while displaying round results and score
                    return;
                }

                activePlayer = ++activePlayer % players.Count;
                PlayerChanged?.Invoke(this, players[activePlayer]);
            }
        }

        private void AnimatePlaceDot(Dot dot, Coords c)
        {
            Vector pos = CoordsToScreen(c);
            Ellipse disc = new()
            {
                Height = dot.Shape.Height,
                Width = dot.Shape.Width,
                Fill = new SolidColorBrush(Util.ChangeColorBrightness(dot.Player.Color, 0.8)),
                Opacity = 0
            };

            offCan.Children.Insert(0, disc);

            double maxSize = dotDiameter * 3;
            Animation anim = animator.RegisterAnimation(disc, AnimationStyle.EaseInOut, 500, (k, x) =>
            {
                double diam = dotDiameter + dotDiameter * 2 * x;
                disc.Height = diam;
                disc.Width = diam;
                SetLeft(disc, pos.X - disc.Width / 2);
                SetTop(disc, pos.Y - disc.Height / 2);
                disc.Opacity = 1 - x;
            });
            anim.AnimationFinished = () => offCan.Children.Remove(disc);
        }


        private void AnimateWin(List<Coords> winRow, Player winPlayer)
        {
            Vector winPos = new();
            foreach (Vector v in winRow.Select(CoordsToScreen))
                winPos += v;

            winPos /= winRow.Count;
            scroller.AnimateScroll(-winPos - new Vector(0, ActualHeight / 4), 1200);

            WinRoundScreen winText = new();
            winText.Opacity = 0;
            winText.EnableScreen(winPlayer, players);

            if (!offCan.Children.Contains(winText))
                offCan.Children.Add(winText);

            animator.RegisterAnimation(AnimationStyle.EaseIn, 1500, (k, x) => winText.Opacity = x);
            
            Vector textPos = winPos + new Vector(-winText.ActualWidth / 2, 50);
            winText.DisplWidthOffset = winText.ActualWidth > 1;
            SetLeft(winText, textPos.X);
            SetTop(winText, textPos.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag && e.ChangedButton == MouseButton.Right && e.ButtonState == MouseButtonState.Released)
            {
                isDrag = false;
                scroller.StopDrag();
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                isDrag = false;
                scroller.StopDrag(false);
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
