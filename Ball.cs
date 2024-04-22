using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Game_Arka;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public class Ball : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Vector2 Ini { get; set; } // This is the initial position.
        public Vector2 StarDirection = new (100f, -520f);
        public Vector2 B_point, A_point, C_point, D_point, CD_point, AC_point;
        
        private readonly SoundEffect _ballWallBounce;
        private Circle _circle;

        public float Speed;        
        public float Maxspeed;
        public float Incrementspeed;

        // This variable manages time.
        public float TimeCount;
        public float ElapsedTime;

        public bool Play {  get; set; }  // Flag to keep the ball attached to the player.


        public Ball(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {

            Debug.Assert(myTexture.Width == myTexture.Height,"height width should be the same");
            var r = (myTexture.Height / 2);
            _circle=new Circle( pos + new Vector2(r), r);
            base.Direction = StarDirection;
            Speed = 1f;
            Ini = pos;
            ElapsedTime = 20f;
            Maxspeed = 3f;
            Play = false;
            _ballWallBounce = content.Load<SoundEffect>("Sounds/WallBounce");
           
            B_point = new Vector2(800, 100);
            A_point = new Vector2(25, 100);
            C_point = new Vector2(25, 875);
            D_point = new Vector2(800, 875);
            CD_point = C_point - D_point;
            AC_point = A_point - C_point;
        }
        
        public void Update(GameTime gameTime, ref Vector2 p)
        {
           
            if (!visible)
                Start();

            KeyBoard(); // To beging the ball moviment.
           
            if (!Play)
            {
                Position.Y = p.Y - Size.Y + 3;
                Position.X = p.X + 60;
                _circle.Center = Position + new Vector2(_circle.Radius);
            }

            if (Play)
                Animation(gameTime);
        }

        public void KeyBoard()
        {
            var Pushkey = Keyboard.GetState();

            if (can_move && Pushkey.IsKeyDown(Keys.Space)) Play = true;
            
        }

        /// <summary>
        /// Which causes the ball to increase its speed over time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Animation(GameTime gameTime)
        {   
            TimeCount += (float)gameTime.ElapsedGameTime.TotalSeconds;
          
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += base.Direction * Speed * deltaTime;
            _circle.Center = Position + new Vector2(_circle.Radius);

            if (TimeCount > ElapsedTime && Speed < Maxspeed)
            {
                Speed += 0.2f;
                ElapsedTime += 20f;
            }
        }

        public void FrameBounce(Vector2 frame)
        {
            Vector2 normal, reflex, orthogonal;
                        
            orthogonal = frame.Orthogonal();
            normal = Vector2.Normalize(orthogonal);
            reflex = Vector2.Reflect(base.Direction, normal);
            base.Direction = reflex;
        }

        public void Bounce(Vector2 normal)
        {
            Vector2 reflex, plus;

            plus = new Vector2(2f, 0f);
            reflex = Vector2.Reflect(base.Direction, normal);
            base.Direction = reflex + plus;
        }

        public bool WallBounce()
        {
            // TopWall
            if (Position.Y <= A_point.Y)
            { FrameBounce(CD_point); _ballWallBounce.Play(); }

            // RightWall
            if (Position.X > D_point.X)
            { FrameBounce(AC_point);}

            // LeftWall
            if (Position.X < C_point.X)
            { FrameBounce(AC_point);}

            // BottomWall
            if (Position.Y >= D_point.Y + 150)
                return false;

            return true;
        }

        public void Death()
        {
            SetVisible(false);
        }

        public void Start()
        {
            Position = Ini;
            SetVisible(true);
            base.Direction = StarDirection;
            Play = false;
            Speed = 1f;
        }
    }
}
