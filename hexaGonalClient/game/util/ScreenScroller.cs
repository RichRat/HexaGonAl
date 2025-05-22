using hexaGoNal.game;
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
        //private readonly double MAX_DIST = 300;
        private readonly int DRAG_HIST = 10;

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
        private List<Vector> dragHist = new();
        private List<Stopwatch> dragVelTimes = new();
        private Vector postDragVector;

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
            dragHist.Add((Vector)mousePosition);
            dragVelTimes.Add(Stopwatch.StartNew());
            if (dragHist.Count > DRAG_HIST)
            {
                dragHist.RemoveAt(0);
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
            postDragVector = (dragHist.Last() - dragHist.First()) / sw.Elapsed.TotalSeconds;

            //TODO maybe add max dist check back in after testing

            if (postDragVector.Length > 0.01)
                animator.RegisterAnimation(new TimedAnimation(250, AfterDragAnimate, AnimationStyle.EaseOut), "after drag");
        }

        private void AfterDragAnimate(object obj, double x, double dt)
        {
            // move the scroll position based on the elapsed time
            offset += postDragVector * dt * (1 - x);
            SetOffset();
        }

        public void StartDrag(object sender, MouseButtonEventArgs e)
        {y
            prevMousePos = e.GetPosition(game);
            animator.UnregisterAnimation("after drag");
            dragHist.Clear();
            dragVelTimes.Clear();
        }

        public Vector Offset
        {
            get => offset;
            set => offset = value;
        }

        internal Animation AnimateScroll(int durationMs, Vector scrollTarget, AnimationStyle style = AnimationStyle.EaseInOut, string aname = "animate scroll")
        {
            animator.UnregisterAnimation(aname);
            Vector startOffset = offset;
            return animator.RegisterAnimation(durationMs, (k, x) =>
            {
                offset = x * scrollTarget + (1 - x) * startOffset;
                SetOffset();
            }, aname, style);
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
