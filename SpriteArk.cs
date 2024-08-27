using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public abstract class SpriteArk
    {
        protected readonly SpriteBatch _spritebatch;
        protected readonly Texture2D myTexture;
        public Vector2 Position;
        public Vector2 anima_position;
        protected ContentManager content;
        public bool Active;

        public abstract Action OnHit { get; set; }
        public Vector2 Size =>  new (myTexture.Width, myTexture.Height);
        public Point CenterPoint => new (myTexture.Width / 2, myTexture.Height / 2);
        public Rectangle R_Collider => new ((int)Position.X,(int)Position.Y, myTexture.Width, myTexture.Height);
        public Rectangle R_blast => new ((int)Position.X-32,(int)Position.Y-30, myTexture.Width +55, myTexture.Height+55);

        public SpriteArk(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos)
        {
            this.content = content;
            _spritebatch = spriteBatch;
            myTexture = content.Load<Texture2D>(texture);
            Position = pos;
            Active = true;
        }
        public SpriteArk(ContentManager content, SpriteBatch spriteBatch,int pos)
        {
            this.content = content;
            _spritebatch = spriteBatch;
            Active = true;
        }
        public virtual void Draw(GameTime gameTime)
        {
            if(Active)
                _spritebatch.Draw(myTexture, Position, Color.White);
        }
    }
}
