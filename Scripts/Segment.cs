using Microsoft.Xna.Framework;

namespace Arkanoid_02
{
    public class Segment
    {   
        private Vector2 _ini, _end;
        
        public Vector2 Ini
        {
            get => _ini + Owner.Position;
            set => _ini = value;
        }
        public Vector2 End
        {
            get => _end + Owner.Position;
            set => _end = value;
        }
        public Vector2 Normal => Vector2.Normalize((_end - _ini).Orthogonal());
        public SpriteArk Owner;
        public bool IsActiveSegment;
    }
}
