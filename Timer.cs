using Microsoft.Xna.Framework;
using System;

namespace Arkanoid_02
{
    public static class Timer
    {
        private static double TimeCountE;
       
        public static void CountDown(GameTime gametime, float time, Action cast)
        {
            TimeCountE += gametime.ElapsedGameTime.TotalSeconds;

            if (time <= TimeCountE)
            {
                TimeCountE = 0;
                cast();
            }
        }

    }
}
