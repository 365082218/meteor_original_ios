using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Idevgame.Common.Util
{
    public enum Loops {
        Once = 1,
        Infinite = 2,
    }
    public delegate void TimerHandler();
    //同线程定时器
    public class Timer
    {
        static List<Timer> timers = new List<Timer>();
        static List<Timer> deleted = new List<Timer>();
        static int indexs = 0;
        
        public static void Update(float delta) {
            for (int i = 0; i < deleted.Count; i++) {
                timers.Remove(deleted[i]);
            }
            deleted.Clear();
            for (int i = 0; i < timers.Count; i++) {
                timers[i].update(delta);
                if (timers[i].delete)
                    deleted.Add(timers[i]);
            }
        }

        Timer(float delay, TimerHandler handler = null, Loops loop = Loops.Once) {
            interval = delay;
            method = handler;
            loops = loop;
            tick = interval;
            index = indexs++;
        }

        Loops loops;
        float interval;
        float tick;
        TimerHandler method;
        int index;

        public static Timer once(float delay, TimerHandler method)
        {
            if (method == null)
                return null;
            if (delay <= 0)
                return null;
            Timer t = new Timer(delay, method, Loops.Once);
            timers.Add(t);
            return t;
        }

        public static Timer loop(float delay, TimerHandler method)
        {
            if (method == null)
                return null;
            if (delay <= 0)
                return null;
            Timer t = new Timer(delay, method, Loops.Infinite);
            timers.Add(t);
            return t;
        }
        
        public void update(float delta)
        {
            if (delete)
                return;
            tick -= delta;
            if (tick <= 0) {
                if (loops == Loops.Once)
                    delete = true;
                tick = interval;
                if (method != null)
                    method.Invoke();
            }
        }

        public bool delete;
        public void Stop() {
            delete = true;
        }
    }
}
