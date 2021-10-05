using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace hexaGonalClient.game
{
    class Animator : IDisposable
    {
        Timer timer;
        Dictionary<Object, Animatee> animators = new();

        public enum AnimationStyle
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut
        }

        public Animator()
        {
            timer = new();
            timer.Interval = 1000 / 60;
            timer.Elapsed += OnTimerElapsed;
            timer.Start();

        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (Animatee anim in animators.Values)
            {
         
            }
        }

        private double calcAnimFactor(AnimationStyle anim, double linear) => anim switch
        {
            AnimationStyle.Linear => linear,
            AnimationStyle.EaseIn => throw new NotImplementedException(),
            AnimationStyle.EaseOut => throw new NotImplementedException(),
            AnimationStyle.EaseInOut => throw new NotImplementedException(), 
        }

        public void RegisterAnimation(Object key, Action<double> animate, long millis, AnimationStyle style = AnimationStyle.Linear)
        {
            animators.Add(key, new Animatee(millis, animate, style));
        }

        public void UnregisterAnimation(Object key, bool callFinalState)
        {
            if (callFinalState)
                animators[key].Action(1);

            animators.Remove(key);
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        private class Animatee
        {
            public long StartTime;
            public long TargetTime;
            public Action<double> Action;
            public AnimationStyle Style;

            public Animatee(long timeSpan, Action<double> action, AnimationStyle style)
            {
                TargetTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + timeSpan;
                StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Action = action;
                Style = style;
            }
        }
    }
}
