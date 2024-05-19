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
        public readonly Animations playerAnimation;
        public Vector2 Ini { get; set; }

        private readonly SoundEffect _dead;
        public SoundEffect Bounce;
        public SoundEffect ExtraLife;

        private readonly Song _newlevel;

        public int Life { get; set; }
        //public bool Rightmove { get; set; }
        //public bool Leftmove { get; set; }
        
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
            playerAnimation = new(content,"Animation/Animation_Player", 2, 1, 0.5f,1);
            
            blas_animation = true;
        }

        public void Start()
        {
            if (!visible)
            {
                Position = Ini;
                MediaPlayer.Play(_newlevel);
                visible = true;
            }
        }

        public void Death()
        {
            Life--;
            visible = false;
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
                 new() {ini = Position + new Vector2(Size.X, Size.Y),       end = Position + new Vector2(Size.X,Size.Y/4), owner = this, ActiveSegment = true}, // Flat Right
                 new() {ini = Position + new Vector2(Size.X,Size.Y/4), end = Position + new Vector2(90,0),            owner = this, ActiveSegment = true}, // Inclined Right.
                 new() {ini = Position + new Vector2(90,0),            end = Position + new Vector2(20,0),            owner = this, ActiveSegment = true}, // Up.
                 new() {ini = Position + new Vector2(20,0),            end = Position + new Vector2 (0,Size.Y/4),     owner = this, ActiveSegment = true}, // Inclined Left.
                 new() {ini = Position + new Vector2(0,Size.Y/4),      end = Position + new Vector2 (0,Size.Y),  owner = this, ActiveSegment = true}, // Flat Left.
            };
        }

    }
}
