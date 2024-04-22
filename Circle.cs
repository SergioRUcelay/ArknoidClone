using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Arkanoid_02
{
    public struct Circle
    {
        public Vector2 Center;
        public readonly float Radius;
        
        public Circle(Vector2 center, int r)
        {
            Center = center;
            Radius = r;
            Debug.Assert(Radius > 0, "The value can´t be negative");
        }

        public readonly float GetSize()
        {
            return Radius; 
        }
    }
}