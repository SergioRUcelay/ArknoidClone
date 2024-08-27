﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public class Ball : SpriteArk
    {
        public override Action OnHit { get; set; }
        private Vector2 Ini { get; set; } // This is the initial position.
        private Vector2 StarDirection = new (10f, -52f);
        public Vector2 Direction; // The Vector Direction of the movement.
        public Circle _circle;        

        public float Speed;        
        public float Maxspeed;
        public bool Attach;

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
            Speed = 500;
            Maxspeed = 800;
            Ini = pos + _circle.Center;            
            Attach = true;
            Active = false;
        }

        public void StartUpAndReposition()
        {
            Debug.Assert(!Active);
            Position = Ini;
            Active = true;
            Direction = StarDirection;
            Attach = true;
            Speed = 500f;
        }

        public void Draw(Vector2 pos)
        {   
            _spritebatch.Draw(myTexture, new Vector2(pos.X - _circle.Radius, pos.Y - _circle.Radius), Color.White);
        }
    }
}
