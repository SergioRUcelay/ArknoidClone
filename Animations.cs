using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Arkanoid_02
{
    public class OldAnimations
    {

        public static Dictionary<int, OldAnimations> bricks_animations;
        private readonly List<Rectangle> _frames = new();
        private readonly Texture2D aniTexture;
        
        private int currentFrame;
        private readonly int totalFrames;
        private readonly double frameTime;
        private double currenTime;

        private bool active;

        public OldAnimations(Texture2D text, int totalframes, int frameX, int frameY, double timeF, int row=1)
        {
            active = true;
            aniTexture = text;
            totalFrames = totalframes;
            frameTime = timeF;
            var frameWidth = aniTexture.Width / frameX;
            var frameHeight = aniTexture.Height / frameY;

            for( int i = 0; i < totalFrames; i++)
            {
                _frames.Add(new (frameWidth * i, frameHeight * (row - 1), frameWidth, frameHeight));
            }
        }

        public void Start() => active = true;

        public void Stop() => active= false;

        public void Reset()
        {
            currentFrame = 0;
            currenTime = 0;
        }

        public void Update(GameTime gametime)
        {
            if (!active) return;
                        
            currenTime += gametime.ElapsedGameTime.TotalSeconds;
            if (currenTime >= frameTime)
            {
                currenTime = 0;
                currentFrame = (currentFrame + 1) % totalFrames;                
            }

        }

        public void Draw(SpriteBatch sprite,Vector2 pos)
        {
            sprite.Draw(aniTexture,pos,_frames[currentFrame],Color.White);
        }
       
    }
}   
