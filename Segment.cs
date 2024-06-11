using Microsoft.Xna.Framework;

namespace Arkanoid_02
{
    public class Segment
    {   
        private Vector2 ini;
        private Vector2 end;
        public Vector2 Ini
        {
            get => ini + Owner.Position;
            set => ini = value;
        }
        public Vector2 End
        {
            get => end + Owner.Position;
            set => end = value;
        }
        public Vector2 Normal => Vector2.Normalize((end - ini).Orthogonal());
        public SpriteArk Owner;
        public bool ActiveSegment;
    }
}
