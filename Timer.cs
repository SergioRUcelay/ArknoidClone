using Microsoft.Xna.Framework;
using System;

namespace Arkanoid_02
{
    public class Timer
    {
        public event Action OnMatured;
        private readonly float delay;
        private double TimeCountE;

        public Timer(float timeDelay)
        {
            delay = timeDelay;
        }
        public void Reset(GameTime gametime)
        {
            TimeCountE = gametime.TotalGameTime.TotalSeconds;
        }

        public void CountDown(GameTime gametime)
        {
            if ((TimeCountE + delay) <= gametime.TotalGameTime.TotalSeconds)
            {
                Reset(gametime);
                OnMatured?.Invoke();
            }
        }
    }
}
