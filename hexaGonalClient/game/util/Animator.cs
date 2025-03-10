﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace hexaGonalClient.game
{
    partial class Animator : IDisposable
    {
        private readonly Timer timer;
        private readonly ConcurrentDictionary<Object, Animation> animators = new();
        private readonly FrameworkElement elem;
        private readonly Random rnd = new();
        
        private long int_key = 0;

        public Animator(FrameworkElement elem)
        {
            this.elem = elem;

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
                Animation anim = kvp.Value;
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

                elem.Dispatcher.Invoke(anim.Action, kvp.Key, animFactor);
            }

            if (remFlag)
            {
                foreach (KeyValuePair<object, Animation> item in animators.Where(kvp => kvp.Value.IsDone()))
                {
                    item.Value.OnFinished(elem.Dispatcher);                        
                    RemoveAnimatee(item.Key);
                }
            }
        }

        public Animation RegisterAnimation(long millis, Action<Object, double> animate, AnimationStyle style = AnimationStyle.EaseInOut)
        {
            return RegisterAnimation(millis, animate, int_key++, style);
        }

        public Animation RegisterAnimation(long millis, Action<Object, double> animate, object key, AnimationStyle style = AnimationStyle.EaseInOut)
        {
            Animation anim = new(millis, animate, style);
            bool r = animators.TryAdd(key, anim);
            if (!r)
                Console.WriteLine("Failed adding animation " + key);

            if (!timer.Enabled)
                timer.Enabled = true;

            return anim;
        }

        public void UnregisterAnimation(Object key, bool callFinalState = false)
        {
            if (callFinalState)
                animators[key].Action(key, 1);

            RemoveAnimatee(key);
        }


        private void RemoveAnimatee(Object key)
        {
            if (key == null)
                return;

            animators.Remove(key, out _);
            if (animators.IsEmpty && timer.Enabled)
                timer.Enabled = false;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
