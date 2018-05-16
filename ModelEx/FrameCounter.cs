using System;
using System.Runtime.InteropServices;

namespace ModelEx
{
    public class FrameCounter
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        #region Singleton Pattern
        private static FrameCounter instance = null;
        public static FrameCounter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FrameCounter();
                }
                return instance;
            }
        }
        #endregion

        #region Constructor
        private FrameCounter()
        {
            msPerTick = (float)MillisecondsPerTick;
        }
        #endregion

        float msPerTick = 0.0f;

        long frequency;
        public long Frequency
        {
            get
            {
                QueryPerformanceFrequency(out frequency);
                return frequency;
            }
        }

        long counter;
        public long Counter
        {
            get
            {
                QueryPerformanceCounter(out counter);
                return counter;
            }
        }

        public double MillisecondsPerTick
        {
            get
            {
                return (1000L) / (double)Frequency;
            }
        }

        public delegate void FPSCalculatedHandler(string fps);
        public event FPSCalculatedHandler FPSCalculatedEvent;

        long now;
        long last;
        long dc;
        float dt;
        float elapsedMilliseconds = 0.0f;
        int numFrames = 0;
        float msToTrigger = 1000.0f;

        public float Count()
        {
            last = now;
            now = Counter;
            dc = now - last;
            numFrames++;

            dt = dc * msPerTick;

            elapsedMilliseconds += dt;

            if (elapsedMilliseconds > msToTrigger)
            {
                float seconds = elapsedMilliseconds / 1000.0f;
                float fps = numFrames / seconds;

                if (FPSCalculatedEvent != null)
                    FPSCalculatedEvent("FPS: " + fps.ToString("0.00"));

                elapsedMilliseconds = 0.0f;
                numFrames = 0;
            }

            return dt;
        }
    }
}