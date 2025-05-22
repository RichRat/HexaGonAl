using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hexaGonalClient.game.util
{
    internal class TimedAnimation : Animation
    {

        Action<object, double, double> Action;
        protected double lastInvokeTime = 0;

        public TimedAnimation(long duration, Action<object, double, double> action, AnimationStyle style = AnimationStyle.EaseInOut) : 
            base(duration, null, style)
        {
            Action = action;
        }

        public override void Invoke(object obj, double x)
        {
            double delta = sw.Elapsed.TotalSeconds - lastInvokeTime;
            lastInvokeTime = sw.Elapsed.TotalSeconds;
            Action(obj, x, delta);
        }
    }
}
