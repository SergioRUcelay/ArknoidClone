using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Arkanoid_02
{    
    public class Enemy : SpriteArk
    {
        public override Action OnHit { get; set; }
        private Vector2 _direction;
        public readonly Vector2 EnemyPosition;

        private readonly int _clas;
        private readonly int _pos;
        private readonly string Enemytexture;

        private readonly float _velocity;
        private readonly Animations _enemyAny;
        public readonly SoundEffect Dead;

        public bool EnemyExist;

        public Enemy(int clas, ContentManager content, SpriteBatch spriteBatch, int pos) : base(content, spriteBatch, pos)
        {
            _clas = clas;
            _pos = pos;

            switch (_clas)
            {
                case 0:
                    Enemytexture = "Animation/molecule_enemy";
                    break;
                case 1:
                    Enemytexture = "Animation/triangle_enemy";
                    break;
                case 2:
                    Enemytexture = "Animation/cone_enemy";
                    break;
                case 3:
                    Enemytexture = "Animation/cube_enemy";
                    break;
                
            }

            switch (_pos)
            {
                case 1:
                    EnemyPosition = new Vector2(195, 40);
                    break;
                 case 2:
                    EnemyPosition = new Vector2(590, 40);
                    break;
            }
            
            _direction = new Vector2(0,1);
            _velocity = 0.5f;
            _enemyAny = new(content, Enemytexture, 25,1,0.12f);
            Dead = content.Load<SoundEffect>("Sounds/EnemyDestroy");
            AnimationAdd(1, _enemyAny);

            _enemyAny.AnimaActive = true;
        }

        public void Behavior()
        {
            //SpriteArk.Position += Vector2.One;
        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                    new() {end = EnemyPosition+new Vector2 (Size.X,0),     ini = EnemyPosition+new Vector2(Size.X,Size.Y), owner = this, ActiveSegment = true}, // Right
                    new() {end = EnemyPosition+new Vector2(Size.X,Size.Y), ini = EnemyPosition+new Vector2(0,Size.Y),      owner = this, ActiveSegment = true}, // Down
                    new() {end = EnemyPosition+new Vector2(0,Size.Y),      ini = EnemyPosition,                            owner = this, ActiveSegment = true}, // Left
                    new() {end = EnemyPosition,                            ini = EnemyPosition+new Vector2 (Size.X,0),     owner = this, ActiveSegment = true}, // Up
            };
        }
    }
}
