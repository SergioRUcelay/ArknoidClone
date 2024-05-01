using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;


namespace Arkanoid_02
{
    public class Door : SpriteArk
    {
        public override Action OnHit { get; set; }
        private Texture2D _doorText;
        private Animations _openDoor;
        private Animations _closeDoor;

        private  Vector2 _position;

        public Door(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {
            _position = pos;
            //_openDoor=new Animations(content:"")
        }
    }
}
