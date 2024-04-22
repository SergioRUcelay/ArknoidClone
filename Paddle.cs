using Game_Arka;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Threading;

namespace Arkanoid_02
{

    public class Paddle : SpriteArk
    {
        public override Action OnHit { get; set; }
        private Vector2 _movement;
        public readonly Animations playerAnimation;
        public Vector2 Ini { get; set; }

        private readonly SoundEffect _dead;
        public SoundEffect Bounce;
        public SoundEffect ExtraLife;

        private readonly Song _newlevel;

        public int Life { get; set; }
        public bool Rightmove { get; set; }
        public bool Leftmove { get; set; }
        
        private readonly float _speed;

        public Paddle(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            _movement = new Vector2(400f, 0);
            Ini = pos;
            Life = 3;
            _speed = 1.5f;
            _dead = content.Load<SoundEffect>("Sounds/PlayerDead");
            Bounce = content.Load<SoundEffect>("Sounds/PlayerBounce");
            ExtraLife = content.Load<SoundEffect>("Sounds/ExtraLife");
            _newlevel = content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");
            playerAnimation = new(content,"Animation/Animation_Player", 2, 1, 0.5f,1);
            
            blas_animation = true;
        }

        public void Update(GameTime time)
        {
            if (!visible)
                Start();

            if (visible)
            {
                Movement(time);
                //playerAnimation.Update(time);
            }

        }

        public void Movement(GameTime gameTime)
        {
            var KeyState = Keyboard.GetState();

            if (can_move && KeyState.IsKeyDown(Keys.Left) && Position.X > 35f)
            {
                Position.X -= _movement.X * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Rightmove = false;
                Leftmove = true;
            }

            if (can_move && KeyState.IsKeyDown(Keys.Right) && Position.X + Size.X < 810f)
            {
                Position.X += _movement.X * _speed *(float)gameTime.ElapsedGameTime.TotalSeconds;
                Rightmove = true;
                Leftmove = false;
            }
        }
       
        public void Death()
        {
            Life--;
            SetVisible(false);
            _dead.Play();
            Level.Maintext = true;
            Level.NextLevel = false;
            Level.Time_lifeleft = 0;
            Thread.Sleep(1500);
           
        }

        public void Start()
        {
            Position = Ini;
            MediaPlayer.Play(_newlevel);
            SetVisible(true);
           
        }
            
    }
}
