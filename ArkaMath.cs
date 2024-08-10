using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Arkanoid_02
{
    public static class Vector2Ex
    {
        // This is a extension of Vector2 class.
        public static Vector2 Orthogonal(this Vector2 orth)
        {
            float copy = orth.Y;
            orth.Y = orth.X;
            orth.X = -1 * copy;

            return orth;
        }
    }

    static class ArkaMath
    {
        /// <summary> Scroll throught a list of segments to find the closest collision. </summary>
        /// <param name="segment"> List of segments </param>
        /// <param name="direction"> Direction that follow the point on moviment </param>
        /// <param name="position"> The position of the point</param>
        /// <param name="gameTime"> Holds the time state of a Game. (MonoGame -Microsoft.Xna.FrameWork-) </param>
        /// <returns> Segment, distance tuple </returns>
        public static (float, Segment) Collision(List<Segment> segment, Vector2 direction, Vector2 position)
        {
            float minDistance = float.PositiveInfinity;            
            Segment collider = null;

            // Seeking the nearest segment.
            foreach (Segment _segment in segment)
            {
                // Cheking active segment
                if (_segment.ActiveSegment == false)
                    continue;
                // Evaluate if the normal and direction have the same value.
                if (Vector2.Dot(direction, _segment.Normal) > 0)
                    continue;

                float newestDistance = DistancePointLineAlongDir(position, direction, _segment.End, _segment.Ini);

                // This is the future position.
                Vector2 c = position + newestDistance * direction;

                // Evaluate if the point it`s in the segment.
                if (Ifbetween(_segment.Ini, _segment.End, c))
                {
                    if (newestDistance < minDistance)
                    {
                        collider = _segment;
                        minDistance = newestDistance;
                    }
                }
            }
            return (minDistance, collider);
        }

        public static Collision CollideWithWorld(List<Segment> segments, Vector2 direction, Vector2 position, float radius)
        {
            Collision col = new();

            foreach (Segment _segment in segments)
            {
                //  Cheking active segment
                if (_segment.ActiveSegment == false)
                    continue;

                // Evaluate if the normal and direction have the same value
                if (Vector2.Dot(direction, _segment.Normal) > 0 )
                    continue;

                Collision newCollition = CollideDiskSegment(position, radius, _segment);

                if (newCollition.depth > col.depth)
                    col = newCollition;
            }
            return col;

        }
        public static Vector2 NearestPointOnSegment(Vector2 p, Segment seg)
        {
            var tangent = (seg.End - seg.Ini);

            if (Vector2.Dot((p - seg.Ini), tangent) <= 0) return seg.Ini;
            if (Vector2.Dot((p - seg.End), tangent) >= 0) return seg.End;

            tangent.Normalize();
            var relativePos = p - seg.Ini;
            return seg.Ini + tangent * Vector2.Dot(tangent, relativePos);
        }

        public static Collision CollideDiskSegment(Vector2 center, float radius, Segment seg)
        {
            var delta = center - NearestPointOnSegment(center, seg);

            if (delta.LengthSquared() > radius * radius) { return new Collision(); }

            var distance = delta.Length();
            return new()
            {
                Normal = delta / distance,
                depth = radius - distance,
                seg = seg
            };
        }

        /// <summary> Calculate the distance projection through the direction vector to the "line". </summary>
        /// <param name="point">The position of the object</param>
        /// <param name="direction">The direction through the object move</param>
        /// <param name="vectorPoint1">The initial point the segment</param>
        /// <param name="vectorPoint2">The end point of the segment</param>
        /// <returns>The distance between point and the segment</returns>
        public static float DistancePointLineAlongDir(Vector2 point, Vector2 direction, Vector2 vectorPoint1, Vector2 vectorPoint2)
        {
            var dir = Vector2.Normalize(vectorPoint2 - vectorPoint1);         //line vector
            var pointOnLine = NearestPointOnLine(vectorPoint1, dir, point);   //vector from point to line
            var alingment = Vector2.Dot((pointOnLine - point), direction);

            // First catches whether the direction is towards of away from the line, the second catches wether we ARE in the line
            if (alingment > 0)// || (pointOnLine - point).Length() <= 0.000000001)
                return (pointOnLine - point).Length() / Vector2.Dot(direction, Vector2.Normalize(pointOnLine - point)); //There is a finite distance from that point to the line along the vector
            else
                return float.PositiveInfinity;  //Nope, we are pointing the wrong way around
        }

        /// <summary> Calculate the proyection the point on the line. </summary>
        /// <param name="linePnt"> The starting point of the line </param>
        /// <param name="lineDir"> The direction that follow the point </param>
        /// <param name="point"> Proyected point on the line </param>
        /// <returns></returns>
        public static Vector2 NearestPointOnLine(Vector2 linePnt, Vector2 lineDir, Vector2 point)
        {
            var v = point - linePnt;
            var d = Vector2.Dot(v, lineDir); // This vector "d" have been send Normalized.
            return linePnt + lineDir * d;
        }

        /// <summary> This method claculate if the point is on the ini-end segment. </summary>
        /// <param name="point">The position of the object</param>
        /// <param name="initpoint">The initial point the segment</param>
        /// <param name="endpoint">The end point of the segment</param>
        /// <returns>true if witin</returns>
        public static bool Ifbetween(Vector2 initpoint, Vector2 endpoint, Vector2 point)
        {
            //(X - a) / (d - b) or (y - b) / (c - a); The max direrence must be between 0 - 1.
            float leftEpsilon, rightEpsilon;

            if ((endpoint.X - initpoint.X) != 0)
            {
                leftEpsilon = (point.X - initpoint.X) / (endpoint.X - initpoint.X);
                if (0 <= leftEpsilon && leftEpsilon <= 1) return true;
            }
            if ((endpoint.Y - initpoint.Y) != 0)
            {
                rightEpsilon = (point.Y - initpoint.Y) / (endpoint.Y - initpoint.Y);
                if (0 <= rightEpsilon && rightEpsilon <= 1) return true;
            }
            return false;
        }
    }
}
    

