﻿using System;
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

        public enum GameState
        {
            Preview = 0,
            PlayerTransition = 1,
            GameTransition = 2,
            GameFinished = 0xF0
        }

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
            offCan.Width = 25;
            offCan.Height = 25;
            offCan.Background = new SolidColorBrush(Colors.Orange);
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
            players.Add(new Player(Colors.Red, "Player Red"));
            players.Add(new Player(Colors.Blue, "Player Blue"));
            activePlayer = 0;
        }

        private Vector CoordsToScreen(Coords c)
        {
            return xAxsis * c.X * dotSpacing + yAxsis * c.Y * dotSpacing;
        }

        private Coords ScreenToCoords(Vector pos)
        {

            //y axis is just y component of yAchsis vector 
            int y = (int)Math.Round(pos.Y / yAxsis.Y / dotSpacing);
            //now that y is known subtract the distance from pos to create a vector of only the x component

            Vector xPos = pos - (yAxsis * y * dotSpacing);
            int x = (int)Math.Round(xPos.X / xAxsis.X / dotSpacing);

            Coords estim = new(x, y);
            List<Coords> candidates = getNeighbours(estim, true);

            Coords min = estim;
            double minDist = Double.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                double dist = (pos - CoordsToScreen(candidates[i])).Length;
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
                activePlayer = ++activePlayer % players.Count;
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

        private static Coords[] neighboursCoords = { 
            new Coords(1, 0), 
            new Coords(-1, 0), 
            new Coords(1, -1), 
            new Coords(0, -1), 
            new Coords(-1, 1), 
            new Coords(0, -1)
        };

        private List<Coords> getNeighbours(Coords center, bool includeCenter)
        {
            List<Coords> ret = new();
            if (includeCenter)
                ret.Add(center);

            foreach (Coords offset in neighboursCoords)
                ret.Add(center + offset);

            return ret;
        }
 
    }
}
