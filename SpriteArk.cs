using Arkanoid_02;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Game_Arka
{
    public abstract class SpriteArk
    {
        public class Animations
        {
            public readonly List<Rectangle> _frames = new();
            public readonly Texture2D aniTexture;

            public int currentFrame = 0;
            public readonly int totalFrames;
            public readonly double frameTime;
            public double currenTime = 0;

            public bool AnimaActive;

            public  Animations(ContentManager content, string text, int frameX, int frameY, double timeF, int row = 1)
            {
                //AnimaActive = true;
                aniTexture = content.Load<Texture2D>(text);
                totalFrames = frameX;
                frameTime = timeF;
                var frameWidth = aniTexture.Width / frameX;
                var frameHeight = aniTexture.Height / frameY;

                for (int i = 0; i < totalFrames; i++)
                {
                    _frames.Add(new(frameWidth * i, frameHeight * (row - 1), frameWidth, frameHeight));
                }
            }

            public void Start()
            {
                AnimaActive = true;
                currentFrame = 0;
                currenTime = 0;
            }

            public void Stop() => AnimaActive = false;

            public void Reset()
            {
                currentFrame = 0;
                currenTime = 0;
            }

            public void Update(GameTime gametime)
            {
                if (!AnimaActive) return;
                
                currenTime += gametime.ElapsedGameTime.TotalSeconds;
                if (currenTime >= frameTime)
                {
                    currenTime = 0;
                    currentFrame = (currentFrame + 1) % totalFrames;
                }

               if (currentFrame >= totalFrames - 1)
                {
                 Stop();                
                }
            }

            public void UpdateLoop(GameTime gametime)
            {
                if (!AnimaActive) return;

                currenTime += gametime.ElapsedGameTime.TotalSeconds;
                if (currenTime >= frameTime)
                {
                    currenTime = 0;
                    currentFrame = (currentFrame + 1) % totalFrames;
                }
            }

            public void Draw(SpriteBatch sprite, Vector2 pos)
            {
                if (!AnimaActive) return;

                sprite.Draw(aniTexture, pos, _frames[currentFrame], Color.White);
            }

            public void Draw(SpriteBatch sprite, Rectangle rect)
            {
                if (!AnimaActive) return;

                sprite.Draw(aniTexture, rect, _frames[currentFrame], Color.White);
            }
        }
    
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

        public SpriteBatch _spritebatch;
        public readonly Texture2D myTexture;
        public Vector2 position;        
        public Vector2 ani_position;
                
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
        public Rectangle R_blast
        {
            get
            {
                return new Rectangle((int)position.X-32,
                    (int)position.Y-30, myTexture.Width +55, myTexture.Height+55);
            }
        }

        public Dictionary<int, Animations> ani_manager = new();
        public ContentManager content;
        public Vector2 velocity;
        public int ani_key;
        public bool visible;
        public bool can_move;
        public bool blas_animation;
        public bool glin_animation;
        
        public SpriteArk(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos)
        {   
            this.content = content;
            _spritebatch = spriteBatch;
            myTexture = content.Load<Texture2D>(texture);
            position = pos;
            visible = true;
            can_move = false;
        }

        public void AnimationAdd(int key, Animations ani)
        {   
            ani_manager.Add(key, ani);
        }

        public virtual void Draw(GameTime gameTime)
        {
            if(visible)
                _spritebatch.Draw(myTexture, position, Color.White);
        }

        public void SetVisible(bool v) { visible = v; }

    }
}