using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Arkanoid_02
{
    public class Animations
    {
        public readonly List<Rectangle> _frames = new();
        public readonly Texture2D aniTexture;

        public readonly int totalFrames;
        private readonly double frameTime;
        private int currentFrame  = 0;
        private double currenTime = 0;
        
        public bool AnimaActive;

        public Animations(ContentManager content, string texture, int frameX, int frameY, double timeF, int row = 1)
        {
            aniTexture  = content.Load<Texture2D>(texture);
            totalFrames = frameX;
            frameTime   = timeF;
            var frameWidth  = aniTexture.Width  / frameX;
            var frameHeight = aniTexture.Height / frameY;

            for (int i = 0; i < totalFrames; i++)
            {
                _frames.Add(new(frameWidth * i, frameHeight * (row - 1), frameWidth, frameHeight));
            }
        }

        public void Start()
        {
            AnimaActive  = true;
            currentFrame = 0;
            currenTime   = 0;
        }

        public void Stop() => AnimaActive = false;

        public void Reset()
        {
            currentFrame = 0;
            currenTime   = 0;
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
                Stop();            
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
  }   
