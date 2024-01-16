using Game_Arka;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Arkanoid_02
{
    public enum Hard { Metal, Blue, Green, Yellow, Pink };

    public class Brick : SpriteArk 
    { 
        
        public int hit {get; set;} 

        public readonly Hard hardness;
        public bool destructible;

        public SoundEffect BrickBounce;
        public SoundEffect MetalBounce;
        public SoundEffect DesdtroyBounce;

        public Brick(Hard ColorHitValue, ContentManager content, string texture, Vector2 pos) : base(content, texture, pos)
        {
            hardness = ColorHitValue;
            
            switch (hardness)
            {
                case Hard.Metal:                    
                    destructible = false;
                    break;

                case Hard.Blue:
                    destructible = true;
                    hit = 1;
                    break;

                case Hard.Green:
                    destructible = true;
                    hit = 1;
                    break;

                case Hard.Yellow:
                    destructible = true;
                    hit = 2;
                    break;

                case Hard.Pink:
                    destructible = true;
                    hit = 3;
                    break;
            }
            BrickBounce = content.Load<SoundEffect>("Sounds/HitBrickBounce");
            MetalBounce = content.Load<SoundEffect>("Sounds/MetalBounce");
            DesdtroyBounce = content.Load<SoundEffect>("Sounds/DestroyBrickBounce");
                                    
        }

        public bool GetDestruc()
        {
            return destructible;
        }

        public void SetDestruc()
        {
            if (!destructible)
                destructible = true;
            else
                destructible = false;
        }
        

    }
}
