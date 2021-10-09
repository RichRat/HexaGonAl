using System;
using static hexaGonalClient.game.Animator;

namespace hexaGonalClient.game
{
    class Animation
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
    }
}
