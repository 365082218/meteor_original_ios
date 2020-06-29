using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Idevgame.Common.Util
{
    //移植laya的timer
    class Timer<T>
    {
        List<Action<T>> TimerHandler = new List<Action<T>>();
        public void once(int delay, Action<T> method, bool coverBefore = true)
        {

        }

        public void loop(int delay, Action<T> method, bool coverBefore = true)
        {

        }

        public void frameonce(int delay, Action<T> method, bool coverBefore = true)
        {

        }

        public void frameloop(int delay, Action<T> method, bool coverBefore = true)
        {

        }

        public void remove(Action<T> method)
        {

        }

        public void clearAll()
        {

        }

        public void Update()
        {

        }
    }
}
