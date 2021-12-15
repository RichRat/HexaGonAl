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
using hexaGonalClient.game.util;

namespace hexaGoNal.game
{

    class HexMatchGame : Canvas
    {
        private readonly Dictionary<Coords, Dot> dots = new();
        private readonly Canvas offCan = new();
        
        private readonly List<Player> players = new();
        private int playerIndex;

        private GameState state = GameState.Initialized;
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
        public event EventHandler<Player> RoundWon;

        private HexaBot bot = new();

        public bool P2Bot { get; set; }

        public enum GameState
        {
            Turn = 0,
            PlayerTransition = 1,
            RoundTransition = 2,
            WaitingForTurn = 4,
            GameFinished = 32,
            Initialized = 64,
            RoundTransitionAnimation = 128
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

        public Player ActivePlayer 
        { 
            get => players[playerIndex]; 
        }

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
            MouseWheel += OnMouseWheel;

            scroller.SetOffset();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
#if DEBUG
            if (state == GameState.Turn)
                state = GameState.WaitingForTurn;
            else
                state = GameState.Turn;
#endif
            //TODO https://stackoverflow.com/questions/33185482/how-to-programmatically-change-the-scale-of-a-canvas
        }

        /// <summary>
        /// start game with previous settings
        /// </summary>
        public void StartGame() => StartGame(new List<Player>(players));

        /// <summary>
        /// start game with given players
        /// </summary>
        /// <param name="players">list of players</param>
        public void StartGame(List<Player> pl)
        {
            if (state != GameState.Initialized)
                return;

            //Console.WriteLine("Start Game");
            
            //clear up previous game
            scroller.Offset = new Vector();
            scroller.SetOffset();

            players.Clear();
            players.AddRange(pl);

            playerIndex = 0;
            PlayerChanged?.Invoke(this, ActivePlayer);

            offCan.Children.Clear();
            state = GameState.Turn;
            dots.Clear();
            AnimatePlayerTurn(ActivePlayer);
            prevDot = null;

            if (players[1].Name.ToLower().StartsWith("bot"))
            {
                P2Bot = true;
                bot.Player = pl[1];
                bot.Opponent = pl[0];
            }
        }

        private void NextRound()
        {
            state = GameState.RoundTransitionAnimation;
            double bottomDotY = dots.Keys.Select(CoordsToScreen).Select(v => v.Y).Max();
            playerIndex = (playerIndex + 1) % players.Count;
            PlayerChanged?.Invoke(this, ActivePlayer);
            AnimatePlayerTurn(ActivePlayer);
            scroller.AnimateScroll(new Vector(scroller.Offset.X, -(bottomDotY + ActualHeight + dotSpacing)), 1200)
                .AnimationFinished = () =>
                {
                    state = GameState.Turn;
                    dots.Clear();
                    offCan.Children.Clear();
                    scroller.Offset = new Vector();
                    scroller.SetOffset();
                    bot.Clear();
                    BotMove();
                };
        }

        /// <summary>
        /// converts a coordinate to the screen position relative to offCanv
        /// </summary>
        /// <param name="c">Coordinate to convert</param>
        /// <returns>Vector of the screen position relative to offCanv</returns>
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
            //this results in a jumpy experience when moving the mouse since y is calcualted first
            //get all neigbours of the estimation and get the closest hex grid position
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
                    prevDot = new Dot(ActivePlayer, dotDiameter);
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
                PlaceDot();
            }
        }

        private void PlaceDot()
        {
            dots.Add(prevCoords, prevDot);
            prevDot.State = DotState.LastPlaced;
            AnimatePlaceDot(prevDot, prevCoords);
            if (lastPlacedDot != null)
                lastPlacedDot.State = DotState.Default;

            lastPlacedDot = prevDot;
            prevDot = null;

            if (P2Bot)
                bot.AddCoord(prevCoords, ActivePlayer);

            List<Coords> winRow = CheckWin(prevCoords, ActivePlayer);
            if (winRow.Count > 4)
            {
                foreach (Coords c in winRow)
                {
                    if (dots[c] != null)
                        dots[c].State = DotState.Win;
                }

                ActivePlayer.Score++;
                Console.WriteLine("Winner: " + ActivePlayer.Name);
                state = GameState.RoundTransition;
                RoundWon?.Invoke(this, ActivePlayer);
                AnimateWin(winRow, ActivePlayer);
                return;
            }

            if (P2Bot && state == GameState.WaitingForTurn)
                state = GameState.Turn;

            playerIndex = (playerIndex + 1) % players.Count;
            PlayerChanged?.Invoke(this, ActivePlayer);
            AnimatePlayerTurn(ActivePlayer);
            if (P2Bot)
                BotMove();

            //FIXME DEBUG remove code after test or at least make toggleable
#if DEBUG
            DebugBotLogicDisplay();
#endif
        }

        private void DebugBotLogicDisplay()
        {
            foreach (Dot d in debugRemove)
                offCan.Children.Remove(d.Shape as UIElement);

            debugRemove.Clear();
            Player tmp = new(Colors.Gray, "debug");
            int max = 1;
            foreach (var c in bot.getCloud())
                if (c.Value.Score > max)
                    max = c.Value.Score;

            foreach (KeyValuePair<Coords, BotMoveVal> c in bot.getCloud())
            {
                Dot d = new(tmp, dotDiameter);
                d.Shape.Opacity = 0.3;
                Vector v = CoordsToScreen(c.Key);
                Canvas.SetLeft(d.Shape, v.X - dotDiameter / 2);
                Canvas.SetTop(d.Shape, v.Y - dotDiameter / 2);
                offCan.Children.Add(d.Shape);
                d.Shape.ToolTip = "Score: " + c.Value;
                d.Shape.Fill = new SolidColorBrush(Util.ModColBrightness(
                    Color.FromRgb(0x11, 0x11, 0x11), (double)c.Value.Score / (double)max));
                debugRemove.Add(d);
            }
        }

        //FIXME DEBUG remove list
        private List<Dot> debugRemove = new();

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
            offCan.Children.Add(winText);

            animator.RegisterAnimation(AnimationStyle.EaseIn, 1500, (k, x) => winText.Opacity = x);
            
            Vector textPos = winPos + new Vector(-winText.ActualWidth / 2, 50);
            winText.DisplWidthOffset = winText.ActualWidth > 1;
            SetLeft(winText, textPos.X);
            SetTop(winText, textPos.Y);

            IEnumerable<Dot> nonWinList = from v in dots where !v.Value.IsWinDot() select v.Value;
            animator.RegisterAnimation(AnimationStyle.EaseInOut, 1200, (_, x) =>
            {
                foreach (Dot d in nonWinList)
                    d.Shape.Opacity = (1 + 1 - x) / 2;
            });
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
            List<Coords> winRow = new();
            foreach (Coords direction in directionCoords)
            {
                winRow.Clear();
                Coords[] row = GetCheckRows(pos, direction);
                for (int i = 0; i < row.Length; i++)
                {
                    Coords checkPos = row[i];
                    if (dots.ContainsKey(checkPos) && dots[checkPos].Player == actPl)
                    {
                        winRow.Add(checkPos);
                        if (winRow.Count > 4)
                            return winRow;
                    }
                    else
                        winRow.Clear();
                }
            }

            return winRow;
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

        private void BotMove()
        {
            if (!P2Bot)
                return;

            //TODO dont immediately execute. Either dispatch or thread because otherwise player changed is called within the eventhandler.
            if (ActivePlayer == bot.Player)
            {
                state = GameState.WaitingForTurn;
                //TODO async calculate bot turn
                prevCoords = bot.CalcTurn();
                prevDot = new Dot(bot.Player, dotDiameter);
#if DEBUG
                if (bot.getCloud().ContainsKey(prevCoords))
                    prevDot.Shape.ToolTip = "Score: " + bot.getCloud()[prevCoords];
#endif
                Vector dotOffset = CoordsToScreen(prevCoords);
                SetLeft(prevDot.Shape, dotOffset.X - dotDiameter / 2);
                SetTop(prevDot.Shape, dotOffset.Y - dotDiameter / 2);
                offCan.Children.Add(prevDot.Shape);
                PlaceDot();
            }
        }
 
    }
}
