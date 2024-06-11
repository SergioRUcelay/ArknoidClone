using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Threading;

namespace Arkanoid_02
{

    public class Paddle : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Vector2 _paddleDirection;
        public readonly Animations PlayerAnimation;
        public Vector2 Ini { get; set; }

        private readonly SoundEffect _dead;
        public SoundEffect Bounce;
        public SoundEffect ExtraLife;

        private readonly Song _newlevel;

        public int Life { get; set; }        
        public readonly float _paddleSpeed;

        public Paddle(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            _paddleDirection = new Vector2(400f, 0);
            Ini = pos;
            Life = 3;
            _paddleSpeed = 2f;
            _dead = content.Load<SoundEffect>("Sounds/PlayerDead");
            Bounce = content.Load<SoundEffect>("Sounds/PlayerBounce");
            ExtraLife = content.Load<SoundEffect>("Sounds/ExtraLife");
            _newlevel = content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");
            PlayerAnimation = new(content,"Animation/Animation_Player", 2, 1, 0.5f,1);
        }

        public void Start()
        {
            if (!Active)
            {
                Position = Ini;
                MediaPlayer.Play(_newlevel);
                Active = true;
            }
        }

        public void Death()
        {
            Life--;
            Active = false;
            _dead.Play();
            Level.Maintext = true;
            Level.NextLevel = false;
           // Level.Time_lifeleft = 0;
            Thread.Sleep(1500);
        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                 new() {Ini = new Vector2(Size.X, Size.Y),  End = new Vector2(Size.X,Size.Y/4), Owner = this, ActiveSegment = true}, // Flat Right
                 new() {Ini = new Vector2(Size.X,Size.Y/4), End = new Vector2(90,0),            Owner = this, ActiveSegment = true}, // Inclined Right.
                 new() {Ini = new Vector2(90,0),            End = new Vector2(20,0),            Owner = this, ActiveSegment = true}, // Up.
                 new() {Ini = new Vector2(20,0),            End = new Vector2 (0,Size.Y/4),     Owner = this, ActiveSegment = true}, // Inclined Left.
                 new() {Ini = new Vector2(0,Size.Y/4),      End = new Vector2 (0,Size.Y),       Owner = this, ActiveSegment = true}, // Flat Left.
            };
        }
    }
}
