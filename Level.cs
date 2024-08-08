using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace Arkanoid_02
{
    public class Level
    {
        private readonly ContentManager _content;

        private readonly SpriteBatch _spriteBatch;
        private Texture2D _backGround;
        private Texture2D _scoreZone;
        private Texture2D _arkaLogo;

        public readonly List<Brick> _brickList = new();
        public readonly string[][] _currentLevel = new string[30][];

        public int _levelNumber;
        //static double Level_Time_lifeleft;
        public double Total_TimeLevel;
        private const int _maxlevelNumber = 4;
        public int NumberBricks { set; get; }
        public bool NextLevel;

        // Manage enemy
        private Random Random { get; set; }
       
        public Level(IServiceProvider serviceProvider, SpriteBatch spriteBatch, ContentManager content)
        {
            Random = new Random();
            _content = content;            
            _spriteBatch = spriteBatch;
            NextLevel = false;
            _levelNumber = 1;
            NumberBricks = 0;

        }

        public void Iniciate()
        {
            LoadBackground();

            //Textures:
            _arkaLogo = _content.Load<Texture2D>("Items/ArkaLogo");
            BrickLayout(_content);
        }

        public void Update(GameTime gametime)
        {
            //Level_Time_lifeleft += gametime.ElapsedGameTime.TotalSeconds;
            //if (_iniOn) Iniciate();

            //// Manage paddle.
            //_paddle.Start();
            //if (_paddle.Active) PaddleMovement(gametime);

            //// Manage ball.
            //ReleaseBall(); _ball.Start();
            //if (!_ball.Play)
            //{
            //    _ball._circle.Center.Y = _paddle.Position.Y - _ball._circle.Radius;
            //    _ball._circle.Center.X = _paddle.Position.X + 70;
            //}
            //if (_ball._circle.Center.Y >= D_Limit.Y + 150) // Controle if tha ball fall of, and kill it.
            //{
            //    _ball.Death(); _paddle.Death(); _ball.Can_move = false;
            //    _paddle.Can_move = false;
            //}

            //// Old-Code for control the segment collisions with a Ball.
            ////(float mindistance, Segment collider) = ArkaMath.Collision(_segments, _ball.Direction, _ball._circle.Center);

            ////if (mindistance < _ball.Speed * (float)gametime.ElapsedGameTime.TotalSeconds)
            ////    Bounces(mindistance, collider);
            ////else
            ////    IncreaseBallSpeedOverTime(gametime);
            //_ball._circle.Center += _ball.Speed * _ball.Direction * (float)gametime.ElapsedGameTime.TotalSeconds;
            //// Code for control the segment collision with a Circle/Ball.
            //var Collide = ArkaMath.CollideWithWorld(_segments, _ball.Direction, _ball._circle.Center, _ball._circle.Radius);
            //if (Collide.depth > 0)
            //{
            //    _ball._circle.Center -= _ball.Direction * Collide.depth;// ((_ball.Speed * (float)gametime.ElapsedGameTime.TotalSeconds) - ) ;
            //    _ball.Direction = Vector2.Reflect(_ball.Direction, Collide.Normal);
            //    Collide.seg.Owner.OnHit();
            //}
            //else
            //{
               
            //    IncreaseBallSpeedOverTime(gametime);
            //}

            //// Manage Enemy.
            //if (_ball.Play == true && EnemyNumber < 5)
            //{
            //    Timer.CountDown(gametime, 2, DoorManage);
            //    //Timer.CountDown(gametime, 4, EnemyCreate);
            //}

            //foreach (var enemies in _enemies)
            //{
            //    enemies.Animation();
            //    if (enemies.Position.Y >= D_Limit.Y + 200) EnemyNumber--;
            //}

            //// Manage MaxPoints
            //if (Points >= MaxPoints) MaxPoints = Points;
            //Extralife();

            //// Manage AchiveLvel.
            //if (NumberBricks <= 0) { Thread.Sleep(500); AchievedLevel(); }

            //return (_paddle.Life > 0);
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_backGround, new Vector2(0, 0), Color.White);
            _spriteBatch.Draw(_scoreZone, new Vector2(0, 0), Color.White);

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the objet.
                brick.Draw(gameTime);
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet.
                if (brick.Blast != null && brick.Blast.AnimaActive)
                {
                    brick.Blast.Draw(_spriteBatch, brick.R_blast);
                    brick.Blast.Update(gameTime);
                }
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet.
                if (brick.Glint != null && brick.Glint.AnimaActive)
                {
                    brick.Glint.Update(gameTime);
                    brick.Glint.Draw(_spriteBatch, brick.R_Collider);
                }
            }
            
            //// Enemies
            //foreach (var enemies in _enemies)
            //{
            //    if (enemies != null && enemies.Active)
            //    {
            //        enemies.EnemyAnimation.UpdateLoop(gameTime);
            //        enemies.EnemyAnimation.Draw(_spriteBatch, enemies.Position);
            //    }
            //    if (enemies != null && enemies.Blast.AnimaActive)
            //    {
            //        enemies.Blast.Update(gameTime);
            //        enemies.Blast.Draw(_spriteBatch, enemies.Position);
            //    }
            //}
        }

        private void LoadBackground()
        {
            _scoreZone = _content.Load<Texture2D>("Items/ScoresZone");
            if (_levelNumber <= _maxlevelNumber)
            {
                string backGroundPath = string.Format("Levels/Level0{0}", _levelNumber);
                _backGround = _content.Load<Texture2D>(backGroundPath);
            }
            else { throw new NotSupportedException("Level number exceeds content."); }
        }

        private void BrickLayout(ContentManager content)
        {
            Vector2 iniposition = new(0, 0);
            Vector2 position = new(0, 0);
            Vector2 bricksizeX = new(61, 0); // new (_brick.Size.X,0);
            Vector2 bricksizeY = new(0, 30); // new (0, _brick.Size.Y);

            if (_levelNumber <= _maxlevelNumber)
            {
                string brickLayoutPath = string.Format(@"D:\cODEX\LaysCarpGameStudios\Arkanoid\Content\Levels\BlockLevel0" + _levelNumber + ".txt");//, _levelNumber);
                using StreamReader blockLine = new (brickLayoutPath);
                int i = 0;
                while (blockLine.Peek() > -1)
                {
                    _currentLevel[i] = new string[] { blockLine.ReadLine() };
                    i++;
                }
            }
            else { throw new NotSupportedException("Level number exceeds content."); }

            // Line ( Y )
            for (int i = 0; i < _currentLevel.Length; i++)
            {
                // Colums ( X )
                for (int j = 0; j < _currentLevel[i].Length; j++)
                {
                    // Char of the  X
                    for (int k = 0; k < _currentLevel[i][j].Length; k++)
                    {
                        switch (_currentLevel[i][j][k])
                        {
                            case ',':
                            case '.':
                                position += bricksizeX;
                                break;

                            //all the following cases using just one
                            ///hits:
                            ///use         case default
                            ///use a map (string, tuple) (C# Dictionary is equivalent to C++ unordered_map. Furthermore, C# SortedDictionary is equivalent to C++ map.)
                            ///map(string, (string, int))
                            ///use tuples
                            ///profit!
                            //                              break;
                            //
                            case '1':
                                var _brick = new Brick(Hard.Blue, _content, _spriteBatch, "Items/BlueBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '2':
                                _brick = new Brick(Hard.Yellow, _content, _spriteBatch, "Items/YellowBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '3':
                                _brick = new Brick(Hard.Green, _content, _spriteBatch, "Items/GreenBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '4':
                                _brick = new Brick(Hard.Pink, _content, _spriteBatch, "Items/PinkBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '5':
                                _brick = new Brick(Hard.Metal, _content, _spriteBatch, "Items/MetalBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                break;
                        }
                    }
                    position.X = iniposition.X;
                }
                position.Y += bricksizeY.Y;
            }
        }

        /// <summary>
        /// Manages the paddle`s movement
        /// </summary>
        /// <param name="gameTime"></param>
        //public void PaddleMovement(GameTime gameTime)
        //{
        //    var keyState = Keyboard.GetState();
        //    int movement = 0;
        //    if (_paddle.Can_move && keyState.IsKeyDown(Keys.Left) && _paddle.Position.X > 35f)
        //        movement = -1;
        //    if (_paddle.Can_move && keyState.IsKeyDown(Keys.Right) && _paddle.Position.X + _paddle.Size.X < 810f)
        //        movement = 1;

        //    _paddle.Position.X += movement * _paddle._paddleDirection.X * _paddle._paddleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //}

        ///// <summary>
        ///// To beging the ball movement.
        ///// </summary>
        //public void ReleaseBall()
        //{
        //    var pushkey = Keyboard.GetState();
        //    if (_ball.Can_move && pushkey.IsKeyDown(Keys.Space)) _ball.Play = true;
        //}

        //public void OutEnemy(int enemyPos)
        //{
        //    var enemyT = Random.Next((int)EnemyType.LAST - 1);
        //    Enemy _enemy = new((EnemyType)enemyT, _content, _spriteBatch, enemyPos);
        //    _enemies.Add(_enemy);
        //    _enemy.OnHit = () => DestroyEnemy(_enemy);
        //    _enemy.Animation = _enemy.EnemyCircleMovement;
        //    ArkaGame._segments.AddRange(_enemy.GetSegments());
        //    EnemyNumber++;
        //}

        public void DestroyBrick(Brick brick)
        {
            if (brick.destructible)
            {
                brick.Hit--;
                if (brick.Hit >= 1)
                {
                    brick.BrickBounce.Play();
                    switch (brick.hardness)
                    {
                        case Hard.Yellow:
                        case Hard.Pink:
                            brick.Glint.Start();
                            break;
                    }
                }

                if (brick.Hit <= 0)
                {
                    foreach (var segment in ArkaGame._segments)
                        segment.ActiveSegment &= segment.Owner != brick;

                    brick.Active = false;
                    brick.Blast.Start();
                    brick.DestroyBounce.Play();
                    NumberBricks--;

                    switch (brick.hardness)
                    {
                        case Hard.Blue:
                        case Hard.Green:
                            ArkaGame.Points += 50;
                            break;

                        case Hard.Yellow:
                            ArkaGame.Points += 100;
                            break;

                        case Hard.Pink:
                            ArkaGame.Points += 300;
                            break;
                    }
                }
            }
            if (!brick.destructible)
            {
                brick.Glint.Start();
                brick.MetalBounce.Play();
            }
        }

        //public void DestroyEnemy(Enemy enemy)
        //{
        //    if (enemy.Active)
        //    {
        //        foreach (var segment in ArkaGame._segments)
        //            segment.ActiveSegment &= segment.Owner != enemy;
        //        enemy.Blast.Start();
        //        enemy.Dead.Play();
        //        enemy.Active = false;
        //        EnemyNumber--;
        //    }
        //}

        //public void FallEnemy(Enemy enemy)
        //{
        //    if(enemy.Active)
        //    {
        //        EnemyNumber--;
        //        enemy.Active = false;
        //        foreach (var segment in ArkaGame._segments)
        //            segment.ActiveSegment &= segment.Owner != enemy;
        //    }
        //}

        //public void Bounces(float minDistance, Segment segment)
        //{
        //    _ball._circle.Center += minDistance * _ball.Direction;
        //    _ball.Direction = Vector2.Reflect(_ball.Direction, segment.Normal);
        //    segment.Owner.OnHit();
        //}

        //public void Bounces(Collision collide)
        //{
        //    //_ball._circle.Center += minDistance * _ball.Direction;
        //    _ball.Direction = Vector2.Reflect(_ball.Direction, collide.Normal);
        //    collide.seg.Owner.OnHit();
        //}

        //public bool TheCollider()
        //{
        //    foreach(var frames in Animations._frames)
        //    foreach (Enemy enemy in _enemies)
        //    {
        //        foreach (Brick brick in _brickList)
        //        {
        //            if (enemy.R_Collider.Intersects(brick.R_Collider))
        //                return true;
        //        }

        //    }
        //    return false;
        //}
    }
}       
        
