using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace WWEngineCC
{
    public class WWtimerUnit
        :IComparable
    {
        public delegate void WWtimer(string str);
        WWtimer CallBack;
        string param;
        double time;
        public WWtimerUnit(WWtimer callback, string _param, double deltatime)
        {
            CallBack = callback;
            param = _param;
            time = WWTime.now + deltatime;
        }
        public int CompareTo(object obj)
        {
            return time < ((WWtimerUnit)obj).time ? -1 : 1;
        }
        public void Ontimer()
        {
            try
            {
                CallBack(param);
            }
            catch
            {

            }
        }
        public bool Check(double now)
        {
            if (now >= time) return true;
            return false;
        }
    }
    public static class WWTime
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(ref Int64 count);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(ref Int64 frequency);

        private static Int64 freq;
        private static Int64 beg;
        private static double lasttime;
        private static double deltatime;
        private static double framedelta = 1000.0 / 60.0;
        private static double secontfram = 0.0;
        private static SortedSet<WWtimerUnit> timerlist = new SortedSet<WWtimerUnit>();
        private static List<WWtimerUnit> timers = new List<WWtimerUnit>();

        public static void WWaddTimer(WWtimerUnit timer)
        {
            timers.Add(timer);
        }

        public static void WWtimerMove()
        {
            if(timers.Count!=0)
            {
                Monitor.Enter(timerlist);
                timerlist.Add(timers.Last());
                Monitor.Exit(timerlist);
                timers.Remove(timers.Last());
            }
        }

        public static double FPS
        {
            get
            {
                return secontfram;
            }
            set
            {
                framedelta = 1.0 / value;
            }
        }

        private static double WWgetTime()
        {
            Int64 now = new Int64();
            QueryPerformanceCounter(ref now);
            return ((double)(now - beg)) * 1000.0 / (double)freq;
        }

        public static void init()
        {
            QueryPerformanceFrequency(ref freq);
            QueryPerformanceCounter(ref beg);
            lasttime = WWgetTime();
        }

        public static void WWupdate()
        {
            double tmp = WWgetTime();
            deltatime = tmp - lasttime;
            lasttime = WWgetTime();
        }

        public static double DeltaTime
        {
            get
            {
                return deltatime;
            }
        }

        public static double now
        {
            get
            {
                return lasttime;
            }
        }

        public static void TimeCircle()
        {
            int cnt = 0;
            double lastsecond = WWgetTime();
            double nxtframe = lastsecond + framedelta;
            while (true)
            {
                double cur = WWgetTime();
                try
                {
                    Monitor.Enter(timerlist);
                    while (timerlist.Count > 0 && timerlist.First().Check(cur))
                    {
                        timerlist.First().Ontimer();
                        timerlist.Remove(timerlist.First());
                    }
                    Monitor.Exit(timerlist);
                }
                catch
                {

                }
                if (cur>=nxtframe)
                {
                    if (cur > nxtframe + 0.75 * framedelta) 
                    {
                        nxtframe = cur + framedelta;
                        continue;
                    }
                    nxtframe = cur + framedelta;
                    Program.update();
                    cnt++;
                    if(cnt>=60)
                    {
                        secontfram = 1000.0 / ((cur - lastsecond) / 60.0);
                        lastsecond = cur;
                        cnt = 0;
                    }
                }
            }

        }
    }
}
