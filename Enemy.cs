using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Arkanoid_02
{
    public enum EnemyType { MOLECULE, TRIANGLE, CONE, CUBE, LAST }
    public class Enemy : SpriteArk
    {
        public override Action OnHit { get; set; }
        private readonly SpriteBatch spriteBatch;
        public Action Animation { get; set; }
        
        public Vector2 _direction;
        public Vector2 UperLimit;
        public Vector2 SpinCenter;
        private readonly string Enemytexture;
        private uint degrees;

        public readonly float RadiusWidth;
        public readonly Animations EnemyAnimation;
        public readonly Animations Blast;
        public readonly SoundEffect Dead;

        public Enemy(EnemyType clas, ContentManager content, SpriteBatch spriteBatch, int pos) : base(content, spriteBatch, pos)
        {
            Position = (pos == 0)? new Vector2(115, 40) : new Vector2(505, 40);
            this.spriteBatch = spriteBatch;
            Enemytexture = "Animation/"+clas.ToString()+"_enemy";
            
            _direction = new Vector2(0.05f,0.5f);
            UperLimit = new Vector2(25,150);
            RadiusWidth = 60f;
            degrees = 0;
            EnemyAnimation = new(content, Enemytexture, 25,1,0.12f);            
            Blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
            Dead = content.Load<SoundEffect>("Sounds/EnemyDestroy");

            SpinCenter = new(Position.X + EnemyAnimation.aniTexture.Width / EnemyAnimation.totalFrames / 2, Position.Y + EnemyAnimation.aniTexture.Height / 2);
            EnemyAnimation.AnimaActive = true;
        }

        public Segment[] GetSegments()
        {
            var EnemySegmentPosition = new Vector2(EnemyAnimation.aniTexture.Width / EnemyAnimation.totalFrames / 2, 0);
            return new Segment[]
            {
                new() {End = EnemySegmentPosition + new Vector2 (EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2,          EnemyAnimation.aniTexture.Height),
                       Ini = EnemySegmentPosition + new Vector2 (-(EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2),       EnemyAnimation.aniTexture.Height),
                    Owner = this, ActiveSegment = true},

                new() {End = EnemySegmentPosition + new Vector2 (-(EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2),       EnemyAnimation.aniTexture.Height),
                       Ini = EnemySegmentPosition + new Vector2 (-(EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2),0),
                    Owner = this, ActiveSegment = true},

                new() {End = EnemySegmentPosition + new Vector2 (-(EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2),0),
                       Ini = EnemySegmentPosition + new Vector2 (EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2,0),
                    Owner = this, ActiveSegment = true},

                new() {End = EnemySegmentPosition + new Vector2 (EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2,0),
                       Ini = EnemySegmentPosition + new Vector2 (EnemyAnimation.aniTexture.Width/EnemyAnimation.totalFrames/2,          EnemyAnimation.aniTexture.Height),
                    Owner = this, ActiveSegment = true},
            };
        }
              
        public void EnemyCircleMovement()
        {   
            float x = SpinCenter.X + RadiusWidth * (float)Math.Cos(degrees * (Math.PI / 180));
            float y = SpinCenter.Y + RadiusWidth * (float)Math.Sin(degrees * (Math.PI / 180));
            Position = new Vector2(x, y);
            degrees++;
            SpinCenter += _direction;
        }

        public void Draw(GameTime gameTime, List<Enemy> _enemies)
        {
            // Enemies
            foreach (var enemies in _enemies)
            {
                if (enemies != null && enemies.Active)
                {
                    enemies.EnemyAnimation.UpdateLoop(gameTime);
                    enemies.EnemyAnimation.Draw(spriteBatch, enemies.Position);
                }
                if (enemies != null && enemies.Blast.AnimaActive)
                {
                    enemies.Blast.Update(gameTime);
                    enemies.Blast.Draw(spriteBatch, enemies.Position);
                }
            }
        }
    }
}
