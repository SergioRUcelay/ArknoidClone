using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public enum Hard { Metal, Blue, Green, Yellow, Pink };

    public class Brick : SpriteArk 
    {
        public override Action OnHit { get; set; }

        public int Hit {get; set;}
        public readonly Hard hardness;
        public bool destructible;

        public SoundEffect BrickBounce;
        public SoundEffect MetalBounce;
        public SoundEffect DestroyBounce;

        public Brick(Hard ColorHitValue, ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            hardness = ColorHitValue;
            
            switch (hardness)
            {
                case Hard.Metal:                    
                    destructible = false;
                    break;

                case Hard.Blue:
                    destructible = true;
                    Hit = 1;
                    break;

                case Hard.Green:
                    destructible = true;
                    Hit = 1;
                    break;

                case Hard.Yellow:
                    destructible = true;
                    Hit = 2;
                    break;

                case Hard.Pink:
                    destructible = true;
                    Hit = 3;
                    break;
            }
            BrickBounce   = content.Load<SoundEffect>("Sounds/HitBrickBounce");
            MetalBounce   = content.Load<SoundEffect>("Sounds/MetalBounce");
            DestroyBounce = content.Load<SoundEffect>("Sounds/DestroyBrickBounce");
            
        }
      
        public bool GetDestruc()
        {
            return destructible;
        }

        public void SetDestruc()
        {
            if (!destructible)
                destructible = true;
            else
                destructible = false;
        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                    new() {end = Position+new Vector2 (Size.X,0),     ini = Position+new Vector2(Size.X,Size.Y), owner = this, ActiveSegment = true}, // Right
                    new() {end = Position+new Vector2(Size.X,Size.Y), ini = Position+new Vector2(0,Size.Y),      owner = this, ActiveSegment = true}, // down
                    new() {end = Position+new Vector2(0,Size.Y),      ini = Position,                            owner = this, ActiveSegment = true}, // Left
                    new() {end = Position,                            ini = Position+new Vector2 (Size.X,0),     owner = this, ActiveSegment = true}, // up
            };
        }
    }
}
