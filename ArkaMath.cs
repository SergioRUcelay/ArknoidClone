//using Microsoft.Xna.Framework;
using System;
using System.Numerics;

namespace Arkanoid_02
{
    static class ArkaMath
    {
        public struct Line
        {
            public float a;
            public float b;
            public float c;
        }

        public static Line LineFromPoints(Vector2 p1, Vector2 p2)
        {
            //(y1 – y2)a + (x2 – x1)b + --c--(x1y2 – x2y1) = 0
            Line ret;
            ret.a = (p1.Y - p2.Y);
            ret.b = (p2.X - p1.X);
            ret.c = p1.X * p2.Y - p2.X * p1.Y;
            return ret;
        }
        public static float DistancePointLine(Vector2 p, Line l)
        {
            return MathF.Abs((l.a * p.X) + (l.b * p.Y) + l.c) / MathF.Sqrt(l.a * l.a + l.b * l.b);
        }

        public static Vector2 NearestPointOnLine(Vector2 linePnt, Vector2 lineDir, Vector2 pnt)
        {
            lineDir /= lineDir.Length();//this needs to be a unit vector
            var v = pnt - linePnt;
            var d = Vector2.Dot(v, lineDir);
            return linePnt + lineDir * d;
        }

        public static float DistancePointLineAlongDir(Vector2 p, Vector2 d, Line l)
        {
            var vp1 = new Vector2(0, -l.c / l.b);            //optimization of y = (l.c - l.a.x)/l.b for x = 0
            var vp2 = new Vector2(1, (-l.c - l.a) / l.b);    //optimization of y = (l.c - l.a.x)/l.b for x = 1
            var v = vp2 - vp1;                          //line vector
            var pil = NearestPointOnLine(vp1, v, p);    //vector from point to line

            //Amo a ve... first catches whether the direction is towards of away from the line, the second catches wether we ARE in the line
            if ((Vector2.Dot((pil - p), d) > 0) || (pil - p).Length() <= 0.000000001)
                return DistancePointLine(p, l) / Vector2.Dot(d, v); //There is a finite distance from that point to the line along the vector
            else
                return float.PositiveInfinity;                      //Nope, we are pointing the wrong way around
        }

        static void Main()
        {
            Vector2 a = new Vector2(0f, 0f);
            Vector2 b = new Vector2(1f, 0f);
            Vector2 c = new Vector2(1f, 1f);

            Line AB = LineFromPoints(a, b);
            //		Console.WriteLine(DistancePointLine(new Vector2(0f, 1f), AB));
            Console.WriteLine(DistancePointLineAlongDir(new Vector2(100f, 0.00001f), new Vector2(0.7f, -0.7f), AB));

        }
    }

}
