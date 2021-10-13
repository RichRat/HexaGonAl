using hexaGoNal.game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace hexaGonalClient.game
{
    class ScreenScroller
    {
        private Canvas canv;
        private HexMatchGame game;
        private Vector offset;
        private Animator animator;

        
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

        public void SetOffset() => SetOffset(null, null);

        public void SetOffset(Object sender, EventArgs e)
        {
            Canvas.SetLeft(canv, offset.X + game.ActualWidth / 2);
            Canvas.SetTop(canv, offset.Y + game.ActualHeight / 2);
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
            SetOffset(null, null);
        }

        public void StopDrag(bool postAnimate = true)
        {
            prevMousePos = null;
            if (!postAnimate)
                return;

            Stopwatch sw = dragVelTimes[0];
            sw.Stop();
            postDragDir = new Vector();
            foreach (Vector v in dragVels)
                postDragDir += v;

            postDragDir *= 1000d / 60d * 10000d / (sw.ElapsedTicks + 1);
            if (postDragDir.Length > 0.01)
                animator.RegisterAnimation("after drag", Animator.AnimationStyle.EaseOut, 250, PostAnimateDrag);
        }
        private void PostAnimateDrag(object key, double x)
        {
            offset -= postDragDir * (1 - x);
            SetOffset(null, null);
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

        internal Animation AnimateScroll(Vector scrollTarget, int durationMs)
        {
            animator.UnregisterAnimation("animate scroll");
            Vector startOffset = offset;
            return animator.RegisterAnimation("animate scroll", Animator.AnimationStyle.EaseInOut, durationMs, (k, x) => {
                offset = x * scrollTarget + (1 - x) * startOffset;
                SetOffset();
            });
        }
    }
}
