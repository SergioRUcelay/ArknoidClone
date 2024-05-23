using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public class Ball : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Vector2 Ini { get; set; } // This is the initial position.
        private Vector2 StarDirection = new (10f, -52f);
        public Vector2 Direction; // The Vector Direction of the moviment.
        public Circle _circle;        

        public float Speed;        
        public float Maxspeed;
        public float Incrementspeed;

        public bool Play {  get; set; }  // Flag to keep the ball attached to the paddle.

        public struct Circle
        {
            public Vector2 Center;
            public readonly float Radius;

            public Circle(Texture2D text)
            {
                Center = new Vector2(text.Width / 2, text.Height / 2);
                Radius = text.Height / 2;
                Debug.Assert(Radius > 0, "The value can´t be negative");
            }
        }
         
        public Ball(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {   
            _circle = new Circle(myTexture);
            StarDirection.Normalize();
            Direction = StarDirection;            
            Speed = 2000;
            Maxspeed = 1000;
            Ini = pos + _circle.Center;  
            Play = false;            
        }
        
        public void Death()
        {
            visible = false;
        }

        public void Start()
        {
            if (!visible)
            {
                Position = Ini;
                visible = true;
                Direction = StarDirection;
                Play = false;
                Speed = 500f;
            }
        }

        public void Draw(Vector2 pos)
        {   
            _spritebatch.Draw(myTexture, new Vector2(pos.X - _circle.Radius, pos.Y - _circle.Radius), Color.White);
        }
    }
}
