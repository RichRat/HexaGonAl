using System;
using System.Diagnostics;
using System.Windows.Threading;
using static hexaGonalClient.game.Animator;

namespace hexaGonalClient.game
{
    internal class Animation
    {
        public double Duration;
        private Stopwatch sw = new();
        public Action<object, double> Action;
        public AnimationStyle Style;
        private bool finEvoked = false;

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

        public bool IsDone()
        {
            return GetRemainTime() <= 0;
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