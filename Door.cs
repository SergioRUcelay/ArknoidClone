using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public class Door : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Texture2D tex;
        public Animations _openDoor;
        public Animations _closeDoor;
        public Vector2 DoorPosition;
        public bool Open;

        public Door(ContentManager content, SpriteBatch sprite, string  texture, Vector2 pos) : base (content, sprite, texture, pos)
        {   
            tex = content.Load<Texture2D>(texture);
            DoorPosition = pos;
            _openDoor = new Animations(content, "Animation/Open_door", 5, 1, 0.12f);
            _closeDoor = new Animations(content, "Animation/Close_door", 5, 1, 0.12f);
            Active = true;
            Open = false;
        }

        public static void CastDoor(Door door)
        {
            if (!door.Open)
                DoorOpenCast(door);
            else
                DoorCloseCast(door);
        }

        public static void DoorOpenCast(Door door)
        {
            if (door.Active)
            {
                door._openDoor.Start();
                door.Active = false;
                door.Open = true;
            }
        }

        public static void DoorCloseCast(Door door)
        {
            if (!door.Active && door.Open)
            {
                
                door._closeDoor.Start();
                //door.Active = true;
                door.Open = false;
            }
        }
    }
}
