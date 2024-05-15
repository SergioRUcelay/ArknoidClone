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

        public int Hit { get; set; }
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
            BrickBounce = content.Load<SoundEffect>("Sounds/HitBrickBounce");
            MetalBounce = content.Load<SoundEffect>("Sounds/MetalBounce");
            DestroyBounce = content.Load<SoundEffect>("Sounds/DestroyBrickBounce");

        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                    new() {end = Position+new Vector2 (Size.X,0),     ini = Position+new Vector2(Size.X,Size.Y), owner = this, ActiveSegment = true}, // Right
                    new() {end = Position+new Vector2(Size.X,Size.Y), ini = Position+new Vector2(0,Size.Y),      owner = this, ActiveSegment = true}, // Down
                    new() {end = Position+new Vector2(0,Size.Y),      ini = Position,                            owner = this, ActiveSegment = true}, // Left
                    new() {end = Position,                            ini = Position+new Vector2 (Size.X,0),     owner = this, ActiveSegment = true}, // Up
            };
        }

        public Segment[] GetScreenSegment(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            return new Segment[]
            {
                    new() {end = C+ new Vector2(0,20),                      ini = B + new Vector2(0,-20),                       owner = this, ActiveSegment = true}, // Right
                    new() {end = D+new Vector2 (-20,0)/* + new Vector2(0,200)*/, ini = C+ new Vector2(20,0)/*+ new Vector2(0,200)*/,  owner = this, ActiveSegment = true}, // Down
                    new() {end = A+new Vector2(0,-20),                      ini = D + new Vector2(0, +20),                       owner = this, ActiveSegment = true}, // Left
                    new() {end = B + new Vector2(+20,0),                      ini = A + new Vector2(-20,0),                       owner = this, ActiveSegment = true}, // Up
            };
        }
    }
}
