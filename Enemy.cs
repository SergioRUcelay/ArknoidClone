using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Arkanoid_02
{
    public enum EnemyType { Molecule, Triangle, Cone,Cube, LAST }
    public class Enemy : SpriteArk
    {
        public override Action OnHit { get; set; }
        public Action<GameTime> Animation { get; set; }
        
        public Vector2 _direction;

        private readonly int _clas;
        private readonly string Enemytexture;
        private uint degrees;

        public readonly float _velocity;
        private readonly Animations _enemyAny;
        public readonly SoundEffect Dead;

        public bool EnemyExist;

        public Enemy(EnemyType clas, ContentManager content, SpriteBatch spriteBatch, int pos) : base(content, spriteBatch, pos)
        {
            
            switch (pos)
            {
                case 1:
                    Position = new Vector2(195, 40);
                    break;
                 case 2:
                    Position = new Vector2(590, 40);
                    break;
            }
            Enemytexture = "Animation/"+clas.ToString()+"_enemy";

            //switch (clas)
            //{
            //    case 0:
            //        Enemytexture = "Animation/molecule_enemy";
            //        break;
            //    case 1:
            //        Enemytexture = "Animation/triangle_enemy";
            //        break;
            //    case 2:
            //        Enemytexture = "Animation/cone_enemy";
            //        break;
            //    case 3:
            //        Enemytexture = "Animation/cube_enemy";
            //        break;

            //}



            _direction = new Vector2(0,1);
            _direction.Normalize();
            _velocity = 200f;
            degrees = 0;
            _enemyAny = new(content, Enemytexture, 25,1,0.12f);
            Dead = content.Load<SoundEffect>("Sounds/EnemyDestroy");
            AnimationAdd(1, _enemyAny);

            _enemyAny.AnimaActive = true;
        }

        public Segment[] GetSegments()
        {
            return new Segment[]
            {
                    new() {end = Position+new Vector2 (Size.X,0),     ini = Position+new Vector2(Size.X,Size.Y), owner = this, ActiveSegment = true}, // Right
                    new() {end = Position+new Vector2(Size.X,Size.Y), ini = Position+new Vector2(0,Size.Y),      owner = this, ActiveSegment = true}, // Down
                    new() {end = Position+new Vector2(0,Size.Y),      ini = Position,                            owner = this, ActiveSegment = true}, // Left
                    new() {end = Position,                            ini = Position+new Vector2 (Size.X,0),     owner = this, ActiveSegment = true}, // Up
            };
        }

        //public void EnemyHorizontalWaveMovement(GameTime gameTime)
        //{
        //    foreach (var enemies in _enemies)
        //    {
        //        if (degrees <= 360)
        //        {
        //            enemies.Position += new Vector2((float)Math.Cos(degrees * (Math.PI / 180)), 0) * enemies._velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //            degrees += 2;// +;
        //        }
        //        if (degrees > 360)
        //        {
        //            degrees = 0;
        //        }
        //    }
        //}

        //public void EnemyVerticalWaveMovement(GameTime gameTime)
        //{
        //    foreach (var enemies in _enemies)
        //    {
        //        if (degrees <= 360)
        //        {
        //            enemies.Position += new Vector2(0, (float)Math.Sin(degrees * (Math.PI / 180))) * enemies._velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //            degrees += 2;
        //        }
        //        if (degrees > 360)
        //        {
        //            degrees = 0;
        //        }
        //    }
        //}

        public void EnemyCircleMovement(GameTime gameTime)
        {
            Position += new Vector2((float)Math.Cos(degrees * (Math.PI / 180)), (float)Math.Sin(degrees * (Math.PI / 180))) * _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            degrees += 2;
        }
    }
}
