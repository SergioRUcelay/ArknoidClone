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
        private Vector2 InitPosition { get; set; }
        private Vector2 startDirection = new (10f, -52f);
        public Vector2 Direction;
        public CircleSprite Circle;


        public float Speed;
        public float Maxspeed;
        public bool Attach;

        public struct CircleSprite
        {
            public Vector2 Center;
            public readonly float Radius;

            public CircleSprite(Texture2D text)
            {
                Center = new Vector2(text.Width / 2, text.Height / 2);
                Radius = text.Height / 2;
                Debug.Assert(Radius > 0, "The value can´t be negative");
            }
        }
         
        public Ball(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            Circle = new CircleSprite(myTexture);
            startDirection.Normalize();
            Direction = startDirection;
            Speed = 500;
            Maxspeed = 800;
            InitPosition = pos + Circle.Center;
            Attach = true;
            IsActive = false;
        }

        public void StartUpAndReposition()
        {
            Debug.Assert(!IsActive);
            Position = InitPosition;
            IsActive = true;
            Direction = startDirection;
            Attach = true;
            Speed = 500f;
        }

        public void Draw(Vector2 pos)
        {
            spriteBatch.Draw(myTexture, new Vector2(pos.X - Circle.Radius, pos.Y - Circle.Radius), Color.White);
        }
    }
}
