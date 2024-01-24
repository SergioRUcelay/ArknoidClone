using Game_Arka;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace Arkanoid_02
{

    public class Player : SpriteArk
    {
        private Vector2 movement;
        public readonly Animations playerAnimation;
        public Vector2 Ini { get; set; }

        private readonly SoundEffect Dead;
        public SoundEffect Bounce;
        public SoundEffect ExtraLife;

        private readonly Song Newlevel;

        public int Life { get; set; }
        public bool Rightmove { get; set; }
        public bool Leftmove { get; set; }
        
        private readonly float speed;

        public Player(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            movement = new Vector2(400f, 0);
            Ini = pos;
            Life = 3;
            speed = 1.5f;
            Dead = content.Load<SoundEffect>("Sounds/PlayerDead");
            Bounce = content.Load<SoundEffect>("Sounds/PlayerBounce");
            ExtraLife = content.Load<SoundEffect>("Sounds/ExtraLife");
            Newlevel = content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");
            playerAnimation = new(content,"Animation/Animation_Player", 2, 1, 0.5f,1);
            
            _animation = true;
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

            if (can_move && KeyState.IsKeyDown(Keys.Left) && position.X > 35f)
            {
                position.X -= movement.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Rightmove = false;
                Leftmove = true;
            }

            if (can_move && KeyState.IsKeyDown(Keys.Right) && position.X + Size.X < 810f)
            {
                position.X += movement.X * speed *(float)gameTime.ElapsedGameTime.TotalSeconds;
                Rightmove = true;
                Leftmove = false;
            }
        }
       
        public void Death()
        {
            Life--;
            SetVisible(false);
            Dead.Play();
            Level.Maintext = true;
            Level.NextLevel = false;
            Level.Time_lifeleft = 0;
            Thread.Sleep(1500);
           
        }

        public void Start()
        {
            position = Ini;
            MediaPlayer.Play(Newlevel);
            SetVisible(true);
           
        }
            
    }
}
