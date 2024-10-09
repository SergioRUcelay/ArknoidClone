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
        public Animations Blast, Glint;
        public SoundEffect BrickBounce, MetalBounce, DestroyBounce;

        public int Hit { get; set; }
        public readonly Hard Hardness;

        public bool IsDestructible;

        public Brick(Hard ColorHitValue, ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            Hardness       = ColorHitValue;
            IsDestructible = Hardness != Hard.Metal;

            switch (Hardness)
            {
                case Hard.Metal:
                    Glint = new Animations(content, "Animation/Animation_MetalBlock_7", 7, 1, 0.03f);
                    break;

                case Hard.Blue:
                    Hit = 1;
                    Blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                    break;

                case Hard.Green:
                    Hit = 1;
                    Blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                    break;

                case Hard.Yellow:
                    Hit = 2;
                    Blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                    Glint = new Animations(content, "Animation/Animation_YelowBlock_7", 7, 1, 0.05f);
                    break;

                case Hard.Pink:
                    Hit = 3;
                    Blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                    Glint = new Animations(content, "Animation/Animation_PinkBlock_7", 7, 1, 0.05f);
                    break;
            }
            BrickBounce   = content.Load<SoundEffect>("Sounds/HitBrickBounce");
            MetalBounce   = content.Load<SoundEffect>("Sounds/MetalBounce");
            DestroyBounce = content.Load<SoundEffect>("Sounds/DestroyBrickBounce");
        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                new() {End = new Vector2 (Size.X,0),      Ini = new Vector2 (Size.X,Size.Y), Owner = this, IsActiveSegment = true}, // Right
                new() {End = new Vector2 (Size.X,Size.Y), Ini = new Vector2 (0,Size.Y),      Owner = this, IsActiveSegment = true}, // Down
                new() {End = new Vector2 (0,Size.Y),      Ini = Vector2.Zero,                Owner = this, IsActiveSegment = true}, // Left
                new() {End = Vector2.Zero,                Ini = new Vector2 (Size.X,0),      Owner = this, IsActiveSegment = true}, // Up
            };
        }

        public Segment[] GetScreenSegment(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            return new Segment[]
            {
                new() {End = C + new Vector2 ( 20,0),                       Ini = B + new Vector2 (20,0),                        Owner = this, IsActiveSegment = true}, // Right
                new() {End = D + new Vector2 (-20,0) + new Vector2(0,200),  Ini = C + new Vector2 (20, 0)  + new Vector2(0,200), Owner = this, IsActiveSegment = true}, // Down
                new() {End = A + new Vector2 ( 0,-20),                      Ini = D + new Vector2 (0,+20),                       Owner = this, IsActiveSegment = true}, // Left
                new() {End = B + new Vector2 (+20,0),                       Ini = A + new Vector2 (-20,0),                       Owner = this, IsActiveSegment = true}, // Up
            };
        }
    }
}
