using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public class Enemy : SpriteArk
    {
        public override Action OnHit { get; set; }
        private Vector2 _position;
        private Vector2 _velocity;

        public readonly Animations EnemyAny;
        public readonly SoundEffect Dead;

        public Enemy(ContentManager content, SpriteBatch spriteBatch, string texture, Vector2 pos) : base(content, spriteBatch, texture, pos)
        {

        }

        public void Behavior()
        {

        }
    }
}
