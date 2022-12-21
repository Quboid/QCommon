using System;
using System.Diagnostics;

namespace QCommonLib
{
    public class QTimer
    {
        private readonly Stopwatch watch;

        public long MS => watch.ElapsedMilliseconds;
        public double Seconds => watch.Elapsed.TotalSeconds;
        public TimeSpan Elapsed => watch.Elapsed;

        public QTimer()
        {
            watch = new Stopwatch();
            Start();
        }

        public void Start()
        {
            watch.Reset();
            watch.Start();
        }

        public void Pause()
        {
            watch.Stop();
        }

        public void Unpause()
        {
            watch.Start();
        }
    }
}
