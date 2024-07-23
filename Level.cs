using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Arkanoid_02
{
    public class Level
    {
        public ContentManager content;
        public GameTime gametime;
        public Action EnemyCreate { get; set; }
        public Action DoorManage { get; set; }

        private Ball _ball;
        private Paddle _paddle;
        private Brick _screen;
        private Door _doorLeft;
        private Door _doorRight;

        private readonly SpriteBatch SpriteBatch;
        private SpriteFont _numberPointFont;
        private SpriteFont _lifeLeft;
        private Texture2D _backGround;
        private Texture2D _scoreZone;
        private Texture2D _arkaLogo;

        private Song _newlevel;
        private readonly SoundEffect _ballWallBounce;

        private readonly List<Brick> _brickList = new();
        public static readonly List<Segment> _segments = new();
        public static readonly List<Enemy> _enemies = new();
        public static readonly Door [] _doors = new Door[2];
        private readonly string[][] _currentLevel = new string[30][];

        // Vectors describing the boundaries of the screen.
        private Vector2 B_Limit, A_Limit, C_Limit, D_Limit;

        private int _levelNumber;
        static double Level_Time_lifeleft;
        public double Total_TimeLevel;
        private const int _maxlevelNumber = 4;
        private int NumberBricks { set; get; }
        private int Points { get; set; }
        private int ExtraLifePoints { get; set; }
        private int MaxPoints { get; set; }

        // Manage enemy
        private Random Random { get; set; }
        private int EnemyNumber { get; set; }
        private float TimeCountE;

        // This variable manages time for speed increment of ball.
        private float TimeCount;
        private float ElapsedTime;

        // Flag for load Iniciate method only the first time call the Update method
        private bool _iniOn;
        // Flag for print Maintext first load level, or wen died. Print the live less on screen.
        public static bool Maintext;
        // Flag for print NextLevel every load new level.
        public static bool NextLevel;

        public Level(IServiceProvider serviceProvider, GameTime gametime, SpriteBatch spriteBatch)
        {
            Random = new Random();
            content = new ContentManager(serviceProvider, "Content");
            SpriteBatch = spriteBatch;
            this.gametime = gametime;
            _levelNumber = 1;
            NumberBricks = 0;
            ExtraLifePoints = 6000;
            Points = 0;
            MaxPoints = 50000;
            _iniOn = true;
            Maintext = true;
            NextLevel = false;
            _ballWallBounce = content.Load<SoundEffect>("Sounds/WallBounce");
            ElapsedTime = 10f;
            EnemyNumber = 0;

            A_Limit = new Vector2(25, 100);
            B_Limit = new Vector2(800, 100);
            C_Limit = new Vector2(800, 875);
            D_Limit = new Vector2(25, 875);
        }

        private void Iniciate()
        {
            LoadBackgraund();

            // Sounds:
            _newlevel = content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");

            //Textures:
            _arkaLogo = content.Load<Texture2D>("Items/ArkaLogo");

            // Fonts:
            _numberPointFont = content.Load<SpriteFont>("Fonts/Points");
            _lifeLeft = content.Load<SpriteFont>("Fonts/Points");

            // GameObjets creation:
            _paddle = new Paddle(content, SpriteBatch, "Items/Player", new Vector2(365, 810));
            _paddle.OnHit = () => _paddle.Bounce.Play();
            _segments.AddRange(_paddle.GetSegments());            
            _ball = new Ball(content, SpriteBatch, "Items/ball", new Vector2(380, 840));
            _doorLeft = new Door(content, SpriteBatch, "Items/door", new Vector2(182, 75));
            _doors[0] =  _doorLeft;
            _doorRight = new Door(content, SpriteBatch, "Items/door", new Vector2(573, 75));
            _doors[1] =  _doorRight;
            DoorManage = () => Doors();
            //(int EnemyT, int EnemyPos) = CastDoor();
            //EnemyCreate = () => OutEnemy(EnemyPos);

            // Screen`s limits
            _screen = new(Hard.Blue, content, SpriteBatch, "Items/BlueBlock", Vector2.Zero);
            _segments.AddRange(_screen.GetScreenSegment(A_Limit, B_Limit, C_Limit, D_Limit));
            _screen.OnHit = () => _ballWallBounce.Play();

            BrickLayout(content);

            MediaPlayer.Play(_newlevel);

            Level_Time_lifeleft = 0;
            Total_TimeLevel = 0;

            _iniOn = false;
        }

        public bool Update(GameTime gametime)
        {
            Level_Time_lifeleft += gametime.ElapsedGameTime.TotalSeconds;
            if (_iniOn) Iniciate();

            // Manage paddle.
            _paddle.Start();
            if (_paddle.Active) PaddleMovement(gametime);

            // Manage ball.
            ReleaseBall(); _ball.Start();
            if (!_ball.Play)
            {
                _ball._circle.Center.Y = _paddle.Position.Y - _ball._circle.Radius;
                _ball._circle.Center.X = _paddle.Position.X + 70;
            }
            if (_ball._circle.Center.Y >= D_Limit.Y + 150) // Controle if tha ball fall of, and kill it.
            {
                _ball.Death(); _paddle.Death(); _ball.Can_move = false;
                _paddle.Can_move = false;
            }

            // Old-Code for control the segment collisions with a Ball.
            //(float mindistance, Segment collider) = ArkaMath.Collision(_segments, _ball.Direction, _ball._circle.Center);

            //if (mindistance < _ball.Speed * (float)gametime.ElapsedGameTime.TotalSeconds)
            //    Bounces(mindistance, collider);
            //else
            //    IncreaseBallSpeedOverTime(gametime);
            _ball._circle.Center += _ball.Speed * _ball.Direction * (float)gametime.ElapsedGameTime.TotalSeconds;
            // Code for control the segment collision with a Circle/Ball.
            var Collide = ArkaMath.CollideWithWorld(_segments, _ball.Direction, _ball._circle.Center, _ball._circle.Radius);
            if (Collide.depth > 0)
            {
                _ball._circle.Center -= _ball.Direction * Collide.depth;// ((_ball.Speed * (float)gametime.ElapsedGameTime.TotalSeconds) - ) ;
                _ball.Direction = Vector2.Reflect(_ball.Direction, Collide.Normal);
                Collide.seg.Owner.OnHit();
            }
            else
            {
               
                IncreaseBallSpeedOverTime(gametime);
            }

            // Manage Enemy.
            if (_ball.Play == true && EnemyNumber < 5)
            {
                Timer.CountDown(gametime, 2, DoorManage);
                //Timer.CountDown(gametime, 4, EnemyCreate);
            }

            foreach (var enemies in _enemies)
            {
                enemies.Animation();
                if (enemies.Position.Y >= D_Limit.Y + 200) EnemyNumber--;
            }

            // Manage MaxPoints
            if (Points >= MaxPoints) MaxPoints = Points;
            Extralife();

            // Manage AchiveLvel.
            if (NumberBricks <= 0) { Thread.Sleep(500); AchievedLevel(); }

            return (_paddle.Life > 0);
        }

        public void Draw(GameTime gameTime)
        {
            SpriteBatch.Draw(_backGround, new Vector2(0, 0), Color.White);
            SpriteBatch.Draw(_scoreZone, new Vector2(0, 0), Color.White);

            // Score
            string points = Points.ToString();
            SpriteBatch.DrawString(_numberPointFont, points, new Vector2(125, 20), Color.WhiteSmoke);

            // High Socre
            string maxpoints = MaxPoints.ToString();
            SpriteBatch.DrawString(_numberPointFont, maxpoints, new Vector2(400, 20), Color.WhiteSmoke);

            // Doors
            foreach (var door in _doors)
            {
                if (door.Active) door.Draw(gameTime);
                if (door._openDoor.AnimaActive)
                {
                    door._openDoor.Draw(SpriteBatch, door.DoorPosition);
                    door._openDoor.Update(gameTime);
                }
            }

            // Round
            string l_Number = _levelNumber.ToString();
            SpriteBatch.DrawString(_numberPointFont, l_Number, new Vector2(635, 34), Color.WhiteSmoke);

            // Lives
            string l_life = _paddle.Life.ToString();
            SpriteBatch.DrawString(_numberPointFont, l_life, new Vector2(775, 10), Color.WhiteSmoke);

            if (Maintext)
            {
                if (Level_Time_lifeleft < 2.5)
                    MainText();
            }

            if (NextLevel)
            {
                if (Level_Time_lifeleft < 2.5)
                    LevelText();
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the objet.
                brick.Draw(gameTime);
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet. We need call a Dictionary that containt all the animation created.
                if (brick.Blast.AnimaActive)
                {
                    brick.Blast.Draw(SpriteBatch, brick.R_blast);
                    brick.Blast.Update(gameTime);
                }
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet. We need call a Dictionary that containt all the animation created.
                if (brick.Glint != null && brick.Glint.AnimaActive)
                {
                    brick.Glint.Update(gameTime);
                    brick.Glint.Draw(SpriteBatch, brick.R_Collider);
                }
            }

            if (Level_Time_lifeleft > 2.6)
            {   
                _paddle.PlayerAnimation.UpdateLoop(gameTime);
                _paddle.PlayerAnimation.Draw(SpriteBatch, _paddle.Position);
                _ball.Draw(_ball._circle.Center);
                _ball.Can_move = true;
                _paddle.Can_move = true;
            }

            foreach (var enemies in _enemies)
            {
                if (enemies != null && enemies.Active)
                {
                    enemies.EnemyAnimation.UpdateLoop(gameTime);
                    enemies.EnemyAnimation.Draw(SpriteBatch, enemies.Position);
                }
                if (enemies != null && enemies.Blast.AnimaActive)
                {
                    enemies.Blast.Update(gameTime);
                    enemies.Blast.Draw(SpriteBatch, enemies.Position);
                }
            }
        }

        private void LoadBackgraund()
        {
            _scoreZone = content.Load<Texture2D>("Items/ScoresZone");
            if (_levelNumber <= _maxlevelNumber)
            {
                string backGroundPath = string.Format("Levels/Level0{0}", _levelNumber);
                _backGround = content.Load<Texture2D>(backGroundPath);
            }
            else { throw new NotSupportedException("Level number exceeds content."); }
        }

        private void MainText()
        {
            SpriteBatch.Draw(_arkaLogo, new Vector2(320, 620), Color.White);
            SpriteBatch.DrawString(_lifeLeft, _paddle.Life + "  LIVES  LEFT", new Vector2(290, 700), Color.White);
            SpriteBatch.DrawString(_lifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        private void LevelText()
        {
            SpriteBatch.Draw(_arkaLogo, new Vector2(320, 620), Color.White);
            SpriteBatch.DrawString(_lifeLeft, _levelNumber + "   ROUND", new Vector2(345, 700), Color.White);
            SpriteBatch.DrawString(_lifeLeft, "READY!!", new Vector2(345, 750), Color.White);
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
                                var _brick = new Brick(Hard.Blue, content, SpriteBatch, "Items/BlueBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '2':
                                _brick = new Brick(Hard.Yellow, content, SpriteBatch, "Items/YellowBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '3':
                                _brick = new Brick(Hard.Green, content, SpriteBatch, "Items/GreenBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '4':
                                _brick = new Brick(Hard.Pink, content, SpriteBatch, "Items/PinkBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '5':
                                _brick = new Brick(Hard.Metal, content, SpriteBatch, "Items/MetalBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
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
        public void PaddleMovement(GameTime gameTime)
        {
            var KeyState = Keyboard.GetState();
            int movement = 0;
            if (_paddle.Can_move && KeyState.IsKeyDown(Keys.Left) && _paddle.Position.X > 35f)
                movement = -1;
            if (_paddle.Can_move && KeyState.IsKeyDown(Keys.Right) && _paddle.Position.X + _paddle.Size.X < 810f)
                movement = 1;

            _paddle.Position.X += movement * _paddle._paddleDirection.X * _paddle._paddleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// To beging the ball movement.
        /// </summary>
        public void ReleaseBall()
        {
            var Pushkey = Keyboard.GetState();
            if (_ball.Can_move && Pushkey.IsKeyDown(Keys.Space)) _ball.Play = true;
        }

        public (int, int) Doors()
        {            
            var EnemyT = Random.Next((int)EnemyType.LAST - 1);
            var EnemyPos = Random.Next(0, 2);            
            Door.CastDoor(_doors[EnemyPos]);            
            int Opendor = EnemyPos;
            return (EnemyT, EnemyPos);
            //Enemy _enemy = new((EnemyType)EnemyT, content, SpriteBatch, EnemyPos);
            //_enemies.Add(_enemy);
            //_enemy.OnHit = () => DestroyEnemy(_enemy);
            //_enemy.Animation = _enemy.EnemyCircleMovement;
            //_segments.AddRange(_enemy.GetSegments());
            //EnemyNumber++;
        }

        public void OutEnemy(int EnemyPos)
        {
            var EnemyT = Random.Next((int)EnemyType.LAST - 1);
            Enemy _enemy = new((EnemyType)EnemyT, content, SpriteBatch, EnemyPos);
            _enemies.Add(_enemy);
            _enemy.OnHit = () => DestroyEnemy(_enemy);
            _enemy.Animation = _enemy.EnemyCircleMovement;
            _segments.AddRange(_enemy.GetSegments());
            EnemyNumber++;
        }

        public void AchievedLevel()
        {
            _levelNumber++;
            if (_levelNumber > 4)
                _levelNumber = 1;

            _brickList.Clear();
            _segments.Clear();
            _enemies.Clear();
            NumberBricks = 0;
            EnemyNumber = 0;
            Maintext = false;
            NextLevel = true;
            _ball.Can_move = false;
            _paddle.Can_move = false;

            Iniciate();
        }

        public void Extralife()
        {
            if (Points >= ExtraLifePoints)
            {
                ExtraLifePoints += Points;
                _paddle.Life += 1;
                _paddle.ExtraLife.Play();
            }
        }

        public void GameOver()
        {
            _brickList.Clear();
            _segments.Clear();
            _enemies.Clear();
            _iniOn = true;
            Maintext = true;
            NextLevel = false;
            _ball.Can_move = false;
            _paddle.Can_move = false;
            Level_Time_lifeleft = 0;
            _levelNumber = 1;
            EnemyNumber = 0;
            Points = 0;
            NumberBricks = 0;
            ExtraLifePoints = 3000;
        }

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
                    foreach (var segment in _segments)
                        segment.ActiveSegment &= segment.Owner != brick;

                    brick.Active = false;
                    brick.Blast.Start();
                    brick.DestroyBounce.Play();
                    NumberBricks--;

                    switch (brick.hardness)
                    {
                        case Hard.Blue:
                        case Hard.Green:
                            Points += 50;
                            break;

                        case Hard.Yellow:
                            Points += 100;
                            break;

                        case Hard.Pink:
                            Points += 300;
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

        public void DestroyEnemy(Enemy enemy)
        {
            foreach (var segment in _segments)
                segment.ActiveSegment &= segment.Owner != enemy;
            //enemy.blas_animation = true;
            //enemy.Blast.animation_key = 2;
            enemy.Blast.Start();
            if (enemy.Active)
                enemy.Dead.Play();
            EnemyNumber--;
            enemy.Active = false;
        }

        /// <summary>
        /// Animate the ball and the increasing his speed over time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void IncreaseBallSpeedOverTime(GameTime gameTime)
        {
            if (_ball.Play)
            {
                TimeCount += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (TimeCount > ElapsedTime && _ball.Speed < _ball.Maxspeed)
                {
                    _ball.Speed += 30f;
                    ElapsedTime += 5f;
                }
            }
        }

        public void Bounces(float minDistance, Segment segment)
        {
            _ball._circle.Center += minDistance * _ball.Direction;
            _ball.Direction = Vector2.Reflect(_ball.Direction, segment.Normal);
            segment.Owner.OnHit();
        }

        public void Bounces(Collision collide)
        {
            //_ball._circle.Center += minDistance * _ball.Direction;
            _ball.Direction = Vector2.Reflect(_ball.Direction, collide.Normal);
            collide.seg.Owner.OnHit();
        }

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
        
