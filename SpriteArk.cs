using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game_Arka
{
    public abstract class SpriteArk
    {
        public struct Circle
        {
            public Vector2 Center;
            public float Radius;

            public Circle( Vector2 center, int r)
            {
               Center = center;
               Radius = r;
            }
        }

        public readonly Texture2D myTexture;
        public Vector2 position;        
        
        public Vector2 Size
        {
            get
            {
                return new Vector2(myTexture.Width, myTexture.Height);
            }
        }

        public Point CenterPoint
        {
            get
            {
                return new Point(myTexture.Width / 2, myTexture.Height / 2);
            }
        }

        public Rectangle R_Collider
        {
            get
            {
                return new Rectangle((int)position.X,
                    (int)position.Y, myTexture.Width, myTexture.Height);
            }
        }

        public Vector2 velocity;
        public bool visible;
        public bool canmove;

        public SpriteArk(ContentManager content, string texture, Vector2 pos)
        {
            content.RootDirectory = "Content";
            myTexture = content.Load<Texture2D>(texture);
            position = pos;
            visible = true;
            canmove = false;
           
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (visible)
            {
                spriteBatch.Draw(myTexture, position, Color.White);
            }
        }

        public void SetVisible(bool v) { visible = v; }
        

    }
}