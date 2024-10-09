using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public class Door : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Texture2D Tex;
        public Animations OpenDoor, CloseDoor;
        public Vector2 DoorPosition;

        public Door(ContentManager content, SpriteBatch sprite, string  texture, Vector2 pos) : base (content, sprite, texture, pos)
        {   
            Tex          = content.Load<Texture2D>(texture);
            OpenDoor     = new (content, "Animation/Open_door", 5, 1, 0.12f);
            CloseDoor    = new (content, "Animation/Close_door", 5, 1, 0.12f);
            DoorPosition = pos;
            IsActive     = true;
        }

        public static void DoorOpenCast(Door door)
        {
            if (door.IsActive)
            {
                door.OpenDoor.Start();
                door.IsActive = false;
            }
        }

        public static void DoorCloseCast(Door door)
        {
            if (!door.IsActive)
            {                
                door.CloseDoor.Start();
                door.IsActive = true;
            }
        }
    }
}
