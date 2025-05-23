﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using hexaGonalClient.game;
using static hexaGonalClient.game.Dot;
using static hexaGonalClient.game.Animator;
using hexaGonalClient;
using System.Windows.Media.Effects;
using hexaGonalClient.game.bot;
using hexaGonalClient.game.util;
using System.Xml;
using System.Net.WebSockets;
using System.Runtime.InteropServices.Swift;
using System.Threading.Tasks;
using System.Diagnostics;

namespace hexaGoNal.game
{

    class HexMatchGame : Canvas
    {
        private readonly Dictionary<Coords, Dot> dots = [];
        private readonly Canvas offCan = new();
        
        private readonly List<Player> players = [];
        private int playerIndex;

        private GameState state = GameState.Initialized;
        private readonly ScreenScroller scroller;
        private bool isDrag = false;

        private WinRoundScreen wrs;
         
        // preview coordinates
        private Coords previewCoords;
        // preview dot
        private Dot previewDot;
        // highlight last placed dot
        private Dot lastPlacedDot = null;

        private readonly double dotSpacing = 26;
        private readonly double dotDiameter = 22;
        private static readonly double yAchisRad = Math.PI / 3;
        private Vector xAxsis = new(1, 0);
        private Vector yAxsis = new(Math.Cos(yAchisRad), Math.Sin(yAchisRad));

        private static readonly int BOT_WAIT_TIME = 100;

        private readonly Animator animator;
        public event EventHandler<Player> PlayerChanged;
        public event EventHandler<Player> RoundWon;
        public event EventHandler Exit;
        public event EventHandler Reset;
        public event EventHandler<Player> GameFinished;

        private readonly HexaBot bot = new();

        public bool BotEnabled { get; set; } = false;
        public int GameLength { get; set; } = 0;

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
            new(1, 0),
            new(-1, 0),
            new(1, -1),
            new(0, -1),
            new(-1, 1),
            new(0, -1)
        };

        // coord directions for every possible axis
        private static readonly Coords[] directionCoords =
        {
            new(1, 0),
            new(0, 1),
            new(-1, 1)
        };

        public Player ActivePlayer 
        { 
            get => players[playerIndex]; 
        }

        public HexMatchGame()
        {
            animator = new();
            scroller = new(offCan, this, animator);

            Children.Add(offCan);
            offCan.ClipToBounds = false;

            ClipToBounds = true;
            Background = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));
            SizeChanged += scroller.OnSizeChanged;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseLeave += OnMouseLeave;
            MouseMove += OnMouseMove;
            MouseWheel += scroller.OnZoomChange;
            

            scroller.SetOffset();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
#if DEBUG
                case Key.Divide:
                    // debug feature to check bot cloud scores
                    if (state == GameState.Turn)
                        state = GameState.WaitingForTurn;
                    else
                        state = GameState.Turn;
                    break;
#endif
                case Key.Space:
                    scroller.SetOffset(new Vector());
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// start game with given players
        /// </summary>
        /// <param name="players">list of players</param>
        public void StartGame(List<Player> players)
        {
            if (state != GameState.Initialized)
                return;

            //Console.WriteLine("Start Game");
            
            //clear up previous game
            scroller.SetOffset(new Vector());

            this.players.Clear();
            this.players.AddRange(players);

            playerIndex = 0;
            PlayerChanged?.Invoke(this, ActivePlayer);

            offCan.Children.Clear();
            state = GameState.Turn;
            dots.Clear();
            AnimatePlayerTurn(ActivePlayer);
            previewDot = null;

            var diffs = players.Select(p => p.Difficulty);
            if (diffs.Where(d => d > 0).Any())
            {
                BotEnabled = true;
                bot.Player = players[1];
                bot.Players = players;
                bot.Difficulty = diffs.Max();
                Console.WriteLine("diff " + bot.Difficulty);
            }
        }

        private void NextRound()
        {
            state = GameState.RoundTransitionAnimation;
            double bottomDotY = dots.Keys.Select(CoordsToScreen).Max(v => v.Y);
            playerIndex = (playerIndex + 1) % players.Count;
            PlayerChanged?.Invoke(this, ActivePlayer);
            AnimatePlayerTurn(ActivePlayer);
            double scrollUpBy = -Math.Max(wrs.GetScrollHeight(), bottomDotY + dotSpacing);
            double wrsPosY = GetTop(wrs);
            animator.RegisterAnimation(1200, (_, x) => SetTop(wrs, wrsPosY + x * scrollUpBy), AnimationStyle.EaseIn)
                .AnimationFinished = () => this.Children.Remove(wrs);

            scroller.AnimateScroll(1200, new Vector(scroller.Offset.X, scroller.Offset.Y + scrollUpBy), AnimationStyle.EaseIn)
                .AnimationFinished = () =>
                {
                    state = GameState.Turn;
                    dots.Clear();
                    offCan.Children.Clear();
                    scroller.SetOffset(new Vector());
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
                if (candidates[i] == previewCoords)
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
                if (previewDot == null)
                {
                    previewDot = new Dot(ActivePlayer, dotDiameter);
                    previewDot.State = DotState.Preview;
                    offCan.Children.Add(previewDot.Shape);
                }

                Coords lastPrevCoords = previewCoords;
                previewCoords = ScreenToCoords((Vector)e.GetPosition(offCan));
                if (previewCoords != lastPrevCoords)
                {
                    Vector dotOffset = CoordsToScreen(previewCoords);
                    SetLeft(previewDot.Shape, dotOffset.X - dotDiameter / 2);
                    SetTop(previewDot.Shape, dotOffset.Y - dotDiameter / 2);
                }

                if (dots.ContainsKey(previewCoords))
                    previewDot.Shape.Visibility = Visibility.Hidden;
                else if (previewDot.Shape.Visibility == Visibility.Hidden)
                    previewDot.Shape.Visibility = Visibility.Visible;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDrag && e.RightButton == MouseButtonState.Pressed && dots.Count > 0)
            {
                isDrag = true;
                scroller.StartDrag(sender, e);
            }

            if (state == GameState.RoundTransition && e.ChangedButton == MouseButton.Left)
            {
                NextRound();
                return;
            }

            if (state == GameState.Turn && e.ChangedButton == MouseButton.Left && !dots.ContainsKey(previewCoords))
            {
                PlaceDot();
            }
        }

        private void PlaceDot()
        {
            if (dots.Count == 0)
            {
                // the first dot will always have the coordinates 0 0 which allows for better zooming
                // and helps to root recorded games (planned)
                Vector v = CoordsToScreen(previewCoords);
                Console.WriteLine("click screen pos " + v);
                scroller.SetOffset(v * scroller.Scale);
                previewCoords = new Coords();
                SetLeft(previewDot.Shape, 0 - dotDiameter / 2);
                SetTop(previewDot.Shape, 0 - dotDiameter / 2);
            }
            dots.Add(previewCoords, previewDot);
            previewDot.State = DotState.LastPlaced;
            AnimatePlaceDot(previewDot, previewCoords);

            if (lastPlacedDot != null)
                lastPlacedDot.State = DotState.Default;

            lastPlacedDot = previewDot;
            previewDot = null;

            if (BotEnabled)
                bot.AddCoord(previewCoords, ActivePlayer);

            List<Coords> winRow = CheckWin(previewCoords, ActivePlayer);
            if (winRow.Count > 4)
            {
                foreach (Coords c in winRow)
                {
                    if (dots[c] != null)
                        dots[c].State = DotState.Win;
                }

                ActivePlayer.Score++;
                Console.WriteLine("Winner: " + ActivePlayer.Name);
                if (GameLength > 0 && players.Max(p => p.Score) >= GameLength / 2 + 1)
                {
                    state = GameState.GameFinished;
                    GameFinished?.Invoke(this, ActivePlayer);
                }
                else
                    state = GameState.RoundTransition;

                RoundWon?.Invoke(this, ActivePlayer);
                AnimateWin(winRow, ActivePlayer);
                return;
            }

            if (BotEnabled && state == GameState.WaitingForTurn)
                state = GameState.Turn;

            playerIndex = (playerIndex + 1) % players.Count;
            PlayerChanged?.Invoke(this, ActivePlayer);
            AnimatePlayerTurn(ActivePlayer);
            BotMove();

#if DEBUG    
            DebugBotLogicDisplay();
#endif
        }

        private void DebugBotLogicDisplay()
        {
            if (!ActivePlayer.IsBot)
                return;

            foreach (Dot d in debugRemove)
                offCan.Children.Remove(d.Shape as UIElement);

            debugRemove.Clear();
            Player tmp = new(Colors.Gray, "debug");
            int max = bot.getCloud().Max(o => o.Value.Score);


            foreach (KeyValuePair<Coords, BotVal> c in bot.getCloud())
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
            Ellipse splash = new()
            {
                Height = dot.Shape.Height,
                Width = dot.Shape.Width,
                Fill = new SolidColorBrush(Util.ModColBrightness(dot.Player.Color, 0.7)),
                Opacity = 0
            };

            offCan.Children.Insert(0, splash);

            double maxSize = dotDiameter * 3;
            Animation anim = animator.RegisterAnimation(500, (k, x) =>
            {
                double diam = dotDiameter + dotDiameter * 2 * x;
                splash.Height = diam;
                splash.Width = diam;
                SetLeft(splash, pos.X - splash.Width / 2);
                SetTop(splash, pos.Y - splash.Height / 2);
                splash.Opacity = 1 - x;
            }, splash, AnimationStyle.EaseInOut);
            anim.AnimationFinished = () => offCan.Children.Remove(splash);
        }


        private void AnimateWin(List<Coords> winRow, Player winPlayer)
        {
            Vector winPos = new();
            foreach (Vector v in winRow.Select(CoordsToScreen))
                winPos += v;

            winPos /= winRow.Count;
            scroller.AnimateScroll(1200, -winPos * scroller.Scale - new Vector(0, ActualHeight / 4));

            wrs = new(this);
            wrs.Opacity = 0;
            wrs.EnableScreen(winPlayer, players, state == GameState.GameFinished);
            this.Children.Add(wrs);
            wrs.SetZoom(scroller.WinScale + 1);
            wrs.InnitPos();
            if (state == GameState.GameFinished)
                wrs.ButtonClick += OnGameFinished;

            animator.RegisterAnimation(1500, (k, x) => wrs.Opacity = x, AnimationStyle.EaseIn);

            IEnumerable<Dot> nonWinList = from v in dots where !v.Value.IsWinDot() select v.Value;
            animator.RegisterAnimation(1200, (_, x) =>
            {
                foreach (Dot d in nonWinList)
                    d.Shape.Opacity = (1 + 1 - x) / 2;
            }, AnimationStyle.EaseInOut);
        }

        private void OnGameFinished(object sender, WinRoundScreen.Response r)
        {
            if (r == WinRoundScreen.Response.Restart)
            {
                foreach (Player p in players)
                    p.Score = 0;

                Reset?.Invoke(this, EventArgs.Empty);
                NextRound();
            }
            else
                Exit?.Invoke(this, EventArgs.Empty);
            
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
            animator.RegisterAnimation(500, (_, x) =>
            {
                SetTop(text, (-text.ActualHeight - 16) * (1 - x));
            }, AnimationStyle.EaseOut);

            animator.RegisterAnimation(2500, (_, x) =>
            {
                if (x > 3 / 4)
                {
                    x = (x - 0.5) * 4;
                    text.Opacity = 1 - x;
                }
            }, AnimationStyle.EaseOut).AnimationFinished = () => Children.Remove(text);
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

        private async void BotMove()
        {
            if (!ActivePlayer.IsBot)
                return;

            //TODO dont immediately execute. Either dispatch or thread because otherwise player changed is called within the eventhandler.
            state = GameState.WaitingForTurn;
            //TODO async calculate bot turn
            previewCoords = await Task.Run(() => {
                Stopwatch sw = Stopwatch.StartNew();
                Coords ret = bot.CalcTurn(ActivePlayer);
                sw.Stop();
                if (sw.ElapsedMilliseconds < BOT_WAIT_TIME)
                    Task.Delay(BOT_WAIT_TIME - (int)sw.ElapsedMilliseconds).Wait();

                return ret;
                
            });
            previewDot = new Dot(bot.Player, dotDiameter);
#if DEBUG
            if (bot.getCloud().ContainsKey(previewCoords))
                previewDot.Shape.ToolTip = "Score: " + bot.getCloud()[previewCoords];
#endif
            Vector dotOffset = CoordsToScreen(previewCoords);
            SetLeft(previewDot.Shape, dotOffset.X - dotDiameter / 2);
            SetTop(previewDot.Shape, dotOffset.Y - dotDiameter / 2);
            offCan.Children.Add(previewDot.Shape);
            PlaceDot();
        }
 
    }
}
