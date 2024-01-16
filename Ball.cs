using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Game_Arka;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace Arkanoid_02
{
    public static class Vector2Ex
    {
        // This is a extension of Vector2 class.
        public static Vector2 Orthogonal(this Vector2 orth)
        {
            float copy = orth.Y;
            orth.Y = orth.X;
            orth.X = -1 * copy;

            return orth;
        }
    }

    public class Ball : SpriteArk
    {
        public Vector2 Ini { get; set; } // This is the initial position.
        public Vector2 startVelocity = new (100f, -520f);
        public Vector2 B_point, A_point, C_point, D_point, CD_point, AC_point;

        private SoundEffect BallWallBounce;
        private Circle circle;

        public float speed;
        public float maxspeed;
        public float incrementspeed;

        // This variable manages time.
        public float timeCount;
        public float elapsedTime;

        public bool Play {  get; set; }  // Flag to keep the ball attached to the player.


        public Ball(ContentManager content, string texture, Vector2 pos) : base(content, texture, pos)
        {

            Debug.Assert(myTexture.Width == myTexture.Height,"height width should be the same");
            var r = (myTexture.Height / 2);
            circle=new Circle( pos + new Vector2(r), r);
            velocity = startVelocity;
            speed = 1f;
            Ini = pos;
            elapsedTime = 20f;
            maxspeed = 3f;
            Play = false;
            BallWallBounce = content.Load<SoundEffect>("Sounds/WallBounce");
           
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
                position.Y = p.Y - Size.Y + 3;
                position.X = p.X + 60;
                circle.Center = position + new Vector2(circle.Radius);
            }

            if (Play)
                Animation(gameTime);
        }

        public void KeyBoard()
        {
            var Pushkey = Keyboard.GetState();

            if (canmove && Pushkey.IsKeyDown(Keys.Space)) Play = true;
            
        }

        public void Animation(GameTime gameTime)
        {   
            timeCount += (float)gameTime.ElapsedGameTime.TotalSeconds;
          
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += base.velocity * speed * deltaTime;
            circle.Center = position + new Vector2(circle.Radius);

            if (timeCount > elapsedTime && speed < maxspeed)
            {
                speed += 0.1f;
                elapsedTime += 20f;
            }
        }

        public void FrameBounce(Vector2 frame)
        {
            Vector2 normal, reflex, orthogonal;

            
            orthogonal = frame.Orthogonal();
            normal = Vector2.Normalize(orthogonal);
            reflex = Vector2.Reflect(base.velocity, normal);
            base.velocity = reflex;
        }

        public void Bounce(Vector2 normal)
        {
            Vector2 reflex, plus;

            plus = new Vector2(2f, 0f);
            reflex = Vector2.Reflect(base.velocity, normal);
            base.velocity = reflex + plus;
        }

        public bool WallBounce()
        {
            // TopWall
            if (position.Y <= A_point.Y)
            { FrameBounce(CD_point); BallWallBounce.Play(); }

            // RightWall
            if (position.X > D_point.X)
            { FrameBounce(AC_point);}

            // LeftWall
            if (position.X < C_point.X)
            { FrameBounce(AC_point);}

            // BottomWall
            if (position.Y >= D_point.Y + 150)
                return false;

            return true;
        }

        public void Death()
        {
            SetVisible(false);
        }

        public void Start()
        {
            position = Ini;
            SetVisible(true);
            velocity = startVelocity;
            Play = false;
            speed = 1f;
        }
    }
}
