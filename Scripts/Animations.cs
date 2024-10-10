using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace Arkanoid_02
{
    public class Animations
    {
        private Action animationEnd;
        public readonly List<Rectangle> framesList = new();
        public readonly Texture2D AnimaTexture;

        public readonly int TotalFrames;
        private readonly double frameTime;
        private int currentFrame  = 0;
        private double currenTime = 0;
        
        public bool IsAnimaActive;

        public Animations(ContentManager content, string texture, int frameX, int frameY, double timeF, int row = 1)
        {
            AnimaTexture  = content.Load<Texture2D>(texture);
            TotalFrames = frameX;
            frameTime   = timeF;
            var frameWidth  = AnimaTexture.Width  / frameX;
            var frameHeight = AnimaTexture.Height / frameY;

            for (int i = 0; i < TotalFrames; i++)
            {
                framesList.Add(new(frameWidth * i, frameHeight * (row - 1), frameWidth, frameHeight));
            }
        }

        public void Start() => Start(() => { });
        
        public void Start(Action action)
        {
            animationEnd += () => { IsAnimaActive = false; action(); };
            IsAnimaActive  = true;
            currentFrame = 0;
            currenTime   = 0;
        }

        public void Stop() => animationEnd?.Invoke();

        public void Reset()
        {
            currentFrame = 0;
            currenTime   = 0;
        }

        public void Update(GameTime gametime)
        {
            if (!IsAnimaActive) return;

            currenTime += gametime.ElapsedGameTime.TotalSeconds;
            if (currenTime >= frameTime)
            {
                currenTime = 0;
                currentFrame = (currentFrame + 1) % TotalFrames;
            }

            if (currentFrame >= TotalFrames - 1)
               Stop();
        }

        public void UpdateLoop(GameTime gametime)
        {
            if (!IsAnimaActive) return;

            currenTime += gametime.ElapsedGameTime.TotalSeconds;
            if (currenTime >= frameTime)
            {
                currenTime = 0;
                currentFrame = (currentFrame + 1) % TotalFrames;
            }
        }

        public void Draw(SpriteBatch sprite, Vector2 pos)
        {
            if (!IsAnimaActive) return;
            sprite.Draw(AnimaTexture, pos, framesList[currentFrame], Color.White);
        }

        public void Draw(SpriteBatch sprite, Rectangle rect)
        {
            if (!IsAnimaActive) return;
            sprite.Draw(AnimaTexture, rect, framesList[currentFrame], Color.White);
        }
    }
  }