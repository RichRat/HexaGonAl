﻿using hexaGoNal.game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Vector = System.Windows.Vector;

namespace hexaGonalClient.game.util
{
    class ScreenScroller
    {
        private readonly Canvas canv;
        private readonly HexMatchGame game;
        private readonly Animator animator;
        private Vector offset;
        private static double zoom = 1;
        private double winZoom = 0;
        // dimensions of the canvas on a clean start
        private Vector? initSize = new Vector(884, 530);

        public double Zoom
        {
            get => zoom;
            set 
            {
                if (Math.Abs(zoom - value) > 0.01)
                {
                    zoom = value;
                    SetZoom();
                }
            }
        }

        public double WinScale { get => winZoom; }

        public double Scale { get => winZoom + zoom; }

        private Point? prevMousePos;
        private List<Vector> dragVels = new();
        private List<Stopwatch> dragVelTimes = new();
        private Vector postDragDir;

        public ScreenScroller(Canvas canv, HexMatchGame game, Animator animator)
        {
            this.animator = animator;
            this.game = game;
            this.canv = canv;
        }

        public void SetOffset(Vector _offset)
        {
            this.offset = _offset;
            SetOffset();
        }

        public void SetOffset()
        {
            Canvas.SetLeft(canv, offset.X + game.ActualWidth / 2);
            Canvas.SetTop(canv, offset.Y + game.ActualHeight / 2);
        }

        public void OnSizeChanged(object sender, EventArgs e)
        {
            SetOffset();
            double sfx = game.ActualWidth / initSize.Value.X;
            double sfy = game.ActualHeight / initSize.Value.Y;
            winZoom = Math.Min(sfx, sfy) - 1;
            SetZoom();
        }

        public void OnDrag(object sender, MouseEventArgs e)
        {
            if (prevMousePos == null)
                return;

            Point mousePosition = e.GetPosition(game);
            offset += mousePosition - prevMousePos.Value;
            dragVels.Add(prevMousePos.Value - mousePosition);
            dragVelTimes.Add(Stopwatch.StartNew());
            if (dragVels.Count > 5)
            {
                dragVels.RemoveAt(0);
                dragVelTimes.RemoveAt(0);
            }

            prevMousePos = mousePosition;
            SetOffset();
        }

        public void StopDrag(bool postAnimate = true)
        {
            prevMousePos = null;
            if (!postAnimate || dragVelTimes.Count < 2)
                return;

            Stopwatch sw = dragVelTimes[0];
            sw.Stop();
            postDragDir = new Vector();
            foreach (Vector v in dragVels)
                postDragDir += v;

            postDragDir *= 1000d / 60d * 10000d / (sw.ElapsedTicks + 1);
            if (postDragDir.Length > 0.01)
                animator.RegisterAnimation(250, PostAnimateDrag, "after drag", AnimationStyle.EaseOut);
        }
        private void PostAnimateDrag(object key, double x)
        {
            offset -= postDragDir * (1 - x);
            SetOffset();
        }

        public void StartDrag(object sender, MouseButtonEventArgs e)
        {
            prevMousePos = e.GetPosition(game);
            animator.UnregisterAnimation("after drag");
            dragVels.Clear();
            dragVelTimes.Clear();
        }

        public Vector Offset
        {
            get => offset;
            set => offset = value;
        }

        internal Animation AnimateScroll(int durationMs, Vector scrollTarget, AnimationStyle style = AnimationStyle.EaseInOut)
        {
            animator.UnregisterAnimation("animate scroll");
            Vector startOffset = offset;
            return animator.RegisterAnimation(durationMs, (k, x) =>
            {
                offset = x * scrollTarget + (1 - x) * startOffset;
                SetOffset();
            }, "animate scroll", style);
        }

        //example https://stackoverflow.com/questions/33185482/how-to-programmatically-change-the-scale-of-a-canvas
        public void OnZoomChange(object sender, MouseWheelEventArgs e)
        {
            zoom += (double)e.Delta / (120 * 4); ;
            SetZoom();
        }

        private void SetZoom()
        {
            double zf = zoom + winZoom;
            if (zf < 1)
            {
                zf = 1;
                zoom = 1 - winZoom;
            }
            
            canv.LayoutTransform = new ScaleTransform(zf, zf);
            canv.UpdateLayout();
        }
    }
}
