using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using hexaGonalClient.game;
using static hexaGoNal.Dot;
using static hexaGonalClient.game.Animator;
using hexaGonalClient;
using System.Windows.Media.Effects;

namespace hexaGoNal.game
{

    class HexMatchGame : Canvas
    {
        private readonly Dictionary<Coords, Dot> dots = new();
        private readonly Canvas offCan = new();
        
        private readonly List<Player> players = new();
        private int activePlayer;
        private GameState state = GameState.Turn;
        private readonly ScreenScroller scroller;
        private bool isDrag = false;

        // preview 
        private Coords prevCoords;
        private Dot prevDot;
        // highlight last placed dot
        private Dot lastPlacedDot = null;

        private double dotSpacing = 26;
        private double dotDiameter = 22;
        private static readonly double yAchisRad = Math.PI / 3;
        private Vector xAxsis = new(1, 0);
        private Vector yAxsis = new(Math.Cos(yAchisRad), Math.Sin(yAchisRad));

        private readonly Animator animator;
        public event EventHandler<Player> PlayerChanged;

        public enum GameState
        {
            Turn = 0,
            PlayerTransition = 1,
            RoundTransition = 2,
            WaitingForTurn = 4,
            GameFinished = 32
        }

        // coords for all possible directions
        private static readonly Coords[] neighboursCoords = {
            new Coords(1, 0),
            new Coords(-1, 0),
            new Coords(1, -1),
            new Coords(0, -1),
            new Coords(-1, 1),
            new Coords(0, -1)
        };

        // coord directions for every possible axis
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

            Children.Add(offCan);
            offCan.ClipToBounds = false;

            ClipToBounds = true;
            Background = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));
            SizeChanged += scroller.SetOffset;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseLeave += OnMouseLeave;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel; ; 

            scroller.SetOffset();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            
            //TODO https://stackoverflow.com/questions/33185482/how-to-programmatically-change-the-scale-of-a-canvas
        }

        public void StartGame()
        {
            //TODO fix bug: No Preview on reset ?!?
            Console.WriteLine("Start Game");
            //clear up previous game
            scroller.Offset = new Vector();
            scroller.SetOffset();

            players.Clear();
            //todo enable custom names and colors(?)
            players.Add(new Player(Colors.Orange, "Player 1"));
            players.Add(new Player(Colors.Cyan, "Player 2"));
            //players.Add(new Player(Color.FromRgb(255, 93, 0), "Master Blaster"));
            //players.Add(new Player(Color.FromRgb(0, 101, 255), "Max Power"));

            activePlayer = 0;
            PlayerChanged?.Invoke(this, players[activePlayer]);

            offCan.Children.Clear();
            state = GameState.Turn;
            dots.Clear();
            AnimatePlayerTurn(players[activePlayer]);
        }

        private void NextRound()
        {
            state = GameState.RoundTransition;
            double bottomDotY = dots.Keys.Select(CoordsToScreen).Select(v => v.Y).Max();
            scroller.AnimateScroll(new Vector(scroller.Offset.X, -(bottomDotY + ActualHeight + dotSpacing)), 1200)
                .AnimationFinished = () =>
                {
                    state = GameState.Turn;
                    dots.Clear();
                    offCan.Children.Clear();
                    scroller.Offset = new Vector();
                    scroller.SetOffset();
                };
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

            if (state == GameState.Turn)
            {
                if (prevDot == null)
                {
                    prevDot = new Dot(players[activePlayer], dotDiameter);
                    prevDot.State = DotState.Preview;
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

            if (state == GameState.RoundTransition && e.ChangedButton == MouseButton.Left)
            {
                NextRound();
                return;
            }

            if (state == GameState.Turn && e.ChangedButton == MouseButton.Left && !dots.ContainsKey(prevCoords))
            {
                dots.Add(prevCoords ,prevDot);
                prevDot.State = DotState.LastPlaced;
                AnimatePlaceDot(prevDot, prevCoords);
                if (lastPlacedDot != null)
                    lastPlacedDot.State = DotState.Default;

                lastPlacedDot = prevDot;
                prevDot = null;
                
                List<Coords> winRow = CheckWin(prevCoords, players[activePlayer]);
                if (winRow.Count > 4)
                {
                    foreach (Coords c in winRow)
                    {
                        if (dots[c] != null)
                            dots[c].State = DotState.Win;
                    }

                    players[activePlayer].Score++;
                    Console.WriteLine("Winner: " + players[activePlayer].Name);
                    state = GameState.RoundTransition;
                    AnimateWin(winRow, players[activePlayer]);
                    return;
                }

                activePlayer = ++activePlayer % players.Count;
                PlayerChanged?.Invoke(this, players[activePlayer]);
                AnimatePlayerTurn(players[activePlayer]);
            }
        }

        private void AnimatePlaceDot(Dot dot, Coords c)
        {
            Vector pos = CoordsToScreen(c);
            Ellipse disc = new()
            {
                Height = dot.Shape.Height,
                Width = dot.Shape.Width,
                Fill = new SolidColorBrush(Util.ModColBrightness(dot.Player.Color, 0.8)),
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

        private void AnimatePlayerTurn(Player p)
        {
            TextBlock text = new TextBlock
            {
                Text = p.Name + "'s Turn",
                FontSize = 25,
                Foreground = p.Brush,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.ExtraBold,
                Effect = new DropShadowEffect
                {
                    ShadowDepth = 0,
                    BlurRadius = 15,
                    Color = Colors.Black
                }
            };

            Children.Add(text);
            SetLeft(text, 0);
            SetTop(text, -300);
            animator.RegisterAnimation(AnimationStyle.EaseOut, 500, (_, x) =>
            {
                SetTop(text, (-text.ActualHeight - 16) * (1 - x));
            });

            animator.RegisterAnimation(AnimationStyle.EaseOut, 2500, (_, x) =>
            {
                if (x > 3 / 4)
                {
                    x = (x - 0.5) * 4;
                    text.Opacity = 1 - x;
                }
            }).AnimationFinished = () => Children.Remove(text);
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
