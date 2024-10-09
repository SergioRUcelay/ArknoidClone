using Microsoft.Xna.Framework;
using System;

namespace Arkanoid_02
{
    public class Timer
    {
        public event Action OnMatured;
        private readonly float delay;
        private double timeCountE;

        public Timer(float timeDelay)
        {
            delay = timeDelay;
        }
        public void Reset(GameTime gametime)
        {
            timeCountE = gametime.TotalGameTime.TotalSeconds;
        }

        public void CountDown(GameTime gametime)
        {
            if ((timeCountE + delay) <= gametime.TotalGameTime.TotalSeconds)
            {
                Reset(gametime);
                OnMatured?.Invoke();
            }
        }
    }
}
