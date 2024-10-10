using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Threading;

namespace Arkanoid_02
{

    public class Paddle : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Vector2 PaddleDirection;
        public readonly Animations PlayerAnimation;
        private Vector2 init { get; set; }

        private readonly SoundEffect dead;
        public SoundEffect Bounce;
        public SoundEffect ExtraLife;
        public bool canMove;

        public int Life { get; set; }        
        public readonly float PaddleSpeed;

        public Paddle(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            PaddleDirection  = new Vector2(400f, 0);
            dead             = content.Load<SoundEffect>("Sounds/PlayerDead");
            Bounce           = content.Load<SoundEffect>("Sounds/PlayerBounce");
            ExtraLife        = content.Load<SoundEffect>("Sounds/ExtraLife");
            PlayerAnimation  = new (content,"Animation/Animation_Player", 2, 1, 0.5f,1);
            init             = pos;
            PaddleSpeed      = 2f;
            canMove          = true;
            IsActive         = false;
            PlayerAnimation.IsAnimaActive = true;
        }

        public void Start()
        {
            Debug.Assert(!IsActive);
            Position = init;
            IsActive = true;
        }

        public void Death()
        {
            Life--;
            IsActive = false;
            dead.Play();
            Thread.Sleep(1500);
        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                 new() {Ini = new Vector2(Size.X, Size.Y),  End = new Vector2(Size.X,Size.Y/4), Owner = this, IsActiveSegment = true}, // Flat Right
                 new() {Ini = new Vector2(Size.X,Size.Y/4), End = new Vector2(75,0),            Owner = this, IsActiveSegment = true}, // Inclined Right.
                 new() {Ini = new Vector2(75,0),            End = new Vector2(35,0),            Owner = this, IsActiveSegment = true}, // Up.
                 new() {Ini = new Vector2(35,0),            End = new Vector2 (0,Size.Y/4),     Owner = this, IsActiveSegment = true}, // Inclined Left.
                 new() {Ini = new Vector2(0,Size.Y/4),      End = new Vector2 (0,Size.Y),       Owner = this, IsActiveSegment = true}, // Flat Left.
            };
        }
    }
}
