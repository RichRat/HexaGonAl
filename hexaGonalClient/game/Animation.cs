using System;
using System.Windows.Threading;
using static hexaGonalClient.game.Animator;

namespace hexaGonalClient.game
{
    internal class Animation
    {
        public long Duration;
        public long TargetTime;
        public Action<object, double> Action;
        public AnimationStyle Style;

        public Animation(long duration, Action<object, double> action, AnimationStyle style)
        {
            TargetTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + duration;
            Duration = duration;
            Action = action;
            Style = style;
        }

        public long GetRemainTime()
        {
            return TargetTime - DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public bool IsDone()
        {
            return GetRemainTime() <= 0;
        }

        public Action AnimationFinished { get; set; }

        internal void OnFinished(Dispatcher disp)
        {
            if (AnimationFinished != null)
                disp.Invoke(AnimationFinished);
        }
    }
}
