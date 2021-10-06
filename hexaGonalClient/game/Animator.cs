using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace hexaGonalClient.game
{
    partial class Animator : IDisposable
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
            timer.Enabled = false; //is enabled on first animatee
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            long curTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            bool remFlag = false;
            foreach (var kvp in animators)
            {
                Animatee anim = kvp.Value;
                //x is the linear progress of the animation
                double x = 1 - (double)(anim.GetRemainTime()) / (double)anim.Duration;
                if (anim.IsDone())
                {
                    x = 1;
                    remFlag = true;
                }

                //convert linear factor to animation progress 0 to 1
                double animFactor = anim.Style switch
                {
                    AnimationStyle.Linear => x,
                    AnimationStyle.EaseIn => x * x,
                    AnimationStyle.EaseOut => Math.Sqrt(x),
                    AnimationStyle.EaseInOut => (Math.Sin((x - 0.5) * Math.PI) + 1) / 2,
                    _ => x
                };

                anim.Action.Invoke(kvp.Key, animFactor);
            }

            if (remFlag)
                foreach (KeyValuePair<object, Animatee> item in animators.Where(kvp => kvp.Value.IsDone()).ToArray())
                    RemoveAnimatee(item.Key);
        }

        public void RegisterAnimation(object key, Action<Object, double> animate, long millis, AnimationStyle style = AnimationStyle.Linear)
        {
            animators.Add(key, new Animatee(millis, animate, style));
            if (!timer.Enabled)
                timer.Enabled = true;
        }

        public void UnregisterAnimation(Object key, bool callFinalState)
        {
            if (callFinalState)
                animators[key].Action(key, 1);

            RemoveAnimatee(key);
        }


        private void RemoveAnimatee(Object key)
        {
            if (key == null)
                return;

            animators.Remove(key);

            if (animators.Count == 0 && timer.Enabled)
                timer.Enabled = false;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
