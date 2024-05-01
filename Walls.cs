using Microsoft.Xna.Framework;
using System;

namespace Arkanoid_02
{
    public  class Walls
    {
        public  Action OnHit { get; set; }
        public Vector2 wIni;
        public Vector2 wEnd;
        public SpriteArk owner;
    }
    
}
