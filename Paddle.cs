using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;

namespace Arkanoid_02
{

    public class Paddle : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Vector2 _paddleDirection;
        public readonly Animations PlayerAnimation;
        private Vector2 Ini { get; set; }

        private readonly SoundEffect _dead;
        public SoundEffect Bounce;
        public SoundEffect ExtraLife;

        public int Life { get; set; }        
        public readonly float _paddleSpeed;

        public Paddle(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            _paddleDirection = new Vector2(400f, 0);
            _dead           = content.Load<SoundEffect>("Sounds/PlayerDead");
            Bounce          = content.Load<SoundEffect>("Sounds/PlayerBounce");
            ExtraLife       = content.Load<SoundEffect>("Sounds/ExtraLife");
            PlayerAnimation = new (content,"Animation/Animation_Player", 2, 1, 0.5f,1);
            Ini = pos;
            _paddleSpeed = 2f;
            Can_move = true;
            PlayerAnimation.AnimaActive = true;
        }

        public void Start()
        {
            if (!Active)
            {
                Position = Ini;
                Active = true;
            }
        }

        public void Death()
        {
            Life--;
            Active = false;
            _dead.Play();
            Thread.Sleep(1500);
        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                 new() {Ini = new Vector2(Size.X, Size.Y),  End = new Vector2(Size.X,Size.Y/4), Owner = this, ActiveSegment = true}, // Flat Right
                 new() {Ini = new Vector2(Size.X,Size.Y/4), End = new Vector2(75,0),            Owner = this, ActiveSegment = true}, // Inclined Right.
                 new() {Ini = new Vector2(75,0),            End = new Vector2(35,0),            Owner = this, ActiveSegment = true}, // Up.
                 new() {Ini = new Vector2(35,0),            End = new Vector2 (0,Size.Y/4),     Owner = this, ActiveSegment = true}, // Inclined Left.
                 new() {Ini = new Vector2(0,Size.Y/4),      End = new Vector2 (0,Size.Y),       Owner = this, ActiveSegment = true}, // Flat Left.
            };
        }
    }
}
