using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
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
        private Vector2 StarDirection = new (100f, -520f);
        public Vector2 Direction; // The Vector Direction of the moviment.
        public Vector2 B_point, A_point, C_point, D_point, CD_point, AC_point;
        
        private readonly SoundEffect _ballWallBounce;
        public Circle _circle;

        public float Speed;        
        public float Maxspeed;
        public float Incrementspeed;

        // This variable manages time.
        public float TimeCount;
        public float ElapsedTime;

        public bool Play {  get; set; }  // Flag to keep the ball attached to the paddle.


        public Ball(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {

            Debug.Assert(myTexture.Width == myTexture.Height,"height width should be the same");
            var r = (myTexture.Height / 2);
            _circle = new Circle( pos + new Vector2(r), r);
            StarDirection.Normalize();
            Direction = StarDirection;
            Speed = 600f;
            Ini = pos;
            ElapsedTime = 20f;
            Maxspeed = 600f;
            Play = false;
            _ballWallBounce = content.Load<SoundEffect>("Sounds/WallBounce");
           
            B_point = new Vector2(800, 100);
            A_point = new Vector2(25, 100);
            C_point = new Vector2(25, 875);
            D_point = new Vector2(800, 875);
            CD_point = C_point - D_point;
            AC_point = A_point - C_point;
        }
        
        /// <summary>
        /// Animate the ball and the increasing his speed over time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void IncreaseBallSpeedOverTime(GameTime gameTime)
        {   
            TimeCount += (float)gameTime.ElapsedGameTime.TotalSeconds;
          
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Direction * Speed * deltaTime;
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
            reflex = Vector2.Reflect(Direction, normal);
            Direction = reflex;
        }

        public void Bounce(Vector2 normal)
        {
            Vector2 reflex, plus;

            plus = new Vector2(2f, 0f);
            reflex = Vector2.Reflect(Direction, normal);
            Direction = reflex + plus;
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
            if (!visible)
            {
                Position = Ini;
                SetVisible(true);
                Direction = StarDirection;
                Play = false;
                //Speed = 1f;
            }
        }
    }
}
