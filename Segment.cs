using Microsoft.Xna.Framework;

namespace Arkanoid_02
{
    public class Segment
    {
        public Vector2 ini;
        public Vector2 end;
        public Vector2 Normal => Vector2.Normalize((end - ini).Orthogonal());
        public SpriteArk owner;
        public bool ActiveSegment;

    }
}
