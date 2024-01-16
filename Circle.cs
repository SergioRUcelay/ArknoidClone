using Microsoft.Xna.Framework;
using System;

namespace Arkanoid_02
{
    public struct Circle
    {
        public Vector2 Center;
        public float Radius;

        //public Circle(Vector2 vect, int radius)
        //{

        //}

        public float GetSize()
        {
            return Radius; 
        }
                      
        public void SetSize(float value)
        {
            if (value <= 0) throw new NotSupportedException("The value can´t be negative");

            Radius = value;
        }

        public readonly bool Collide(Circle other)
        {
            return (other.Center - Center).Length() < other.Radius + Radius;
        }
    }
}
