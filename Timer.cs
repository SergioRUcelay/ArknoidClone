using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Arkanoid_02
{
    public static class Timer
    {
        private readonly static float Timeleft;
        private static float ActualTime;
        
        public static void CountDown(GameTime gametime, int CountDown)
        {
                ActualTime = (int)gametime.TotalGameTime.TotalSeconds;
                //int Timeleft = CountDown - ActualTime;

           if (ActualTime >= Timeleft)
            {
                //return Action;
            }
        }
    }
}
