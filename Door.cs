using Game_Arka;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Arkanoid_02
{
    public class Door : SpriteArk
    {
        private Texture2D _doorText;
        private Animations _openDoor;
        private Animations _closeDoor;

        private  Vector2 _position;

        public Door(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {

        }
    }
}
