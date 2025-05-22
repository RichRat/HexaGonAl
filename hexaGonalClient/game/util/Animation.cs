using System;
using System.Diagnostics;
using System.Windows.Threading;
using static hexaGonalClient.game.Animator;

namespace hexaGonalClient.game
{
    internal class Animation
    {
        public double Duration;
        protected Stopwatch sw = new();
        public Action<object, double> Action;
        public AnimationStyle Style;
        protected bool finEvoked = false;

        public Animation(long duration, Action<object, double> action, AnimationStyle style = AnimationStyle.EaseInOut)
        {
            sw.Start();
            Duration = duration;
            Action = action;
            Style = style;
        }

        public double GetRemainTime()
        {
            return (double)Duration - sw.Elapsed.TotalMilliseconds;
        }

        public double GetFactor()
        {
            double ret = sw.Elapsed.TotalMilliseconds / Duration;
            return ret > 1 ? 1 : ret;
        }

        public bool IsDone()
        {
            return GetRemainTime() <= 0;
        }

        public virtual void Invoke(object obj, double x)
        {
            Action(obj, x);
        }

        public Action AnimationFinished { get; set; }

        internal void OnFinished()
        {
            sw.Stop();
            if (AnimationFinished != null && !finEvoked)
            {
                finEvoked = true;
                AnimationFinished();
            }
        }
    }
}