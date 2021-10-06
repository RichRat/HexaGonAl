using System;

namespace hexaGonalClient.game
{
    partial class Animator
    {
        private class Animatee
        {
            public long Duration;
            public long TargetTime;
            public Action<object, double> Action;
            public AnimationStyle Style;

            public Animatee(long duration, Action<object, double> action, AnimationStyle style)
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
        }
    }
}
