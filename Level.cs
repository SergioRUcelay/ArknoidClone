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
using static Arkanoid_02.SpriteArk;

namespace Arkanoid_02
{
    public class Level : IDisposable
    {
        public ContentManager content;
        public Action create { get; set; }

        private Ball _ball;
        private Paddle _paddle;
        private Brick _screen;
        private Enemy _enemy;
        public  Animations blast;
        public  Animations glint;

        private readonly SpriteBatch SpriteBatch;
        private SpriteFont _numberPointFont;
        private SpriteFont _lifeLeft;
        private Texture2D _backGround;
        private Texture2D _arkaLogo;

        private Song _newlevel;
        private readonly SoundEffect _ballWallBounce;

        private readonly List<Brick> _brickList = new();
        public readonly List<Segment> _segments= new();
        public readonly List<Enemy> _enemies = new();
        private readonly string[][] _currentLevel = new string [30][];

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
        private Random random { get; set; }
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
        // Flag to indicate that there was a collision, and call the glow animation.
        private bool glowOnn;

        public Level(IServiceProvider serviceProvider, SpriteBatch spriteBatch)
        {
            content = new ContentManager(serviceProvider, "Content");
            SpriteBatch = spriteBatch;
            _levelNumber = 1;
            NumberBricks = 0;
            ExtraLifePoints = 6000;
            Points = 0;
            MaxPoints = 50000;
            _iniOn = true;
            Maintext = true;
            NextLevel = false;
            glowOnn = false;
            _ballWallBounce = content.Load<SoundEffect>("Sounds/WallBounce");
            ElapsedTime = 10f;
            EnemyNumber = 0;
            create = () => CreateEnemy();

            A_Limit = new Vector2(25, 100);
            B_Limit = new Vector2(800, 100);
            C_Limit = new Vector2(800, 875);
            D_Limit = new Vector2(25, 875);

            random = new Random();
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
            _paddle = new Paddle(content, SpriteBatch,"Items/Player", new Vector2(365, 810));
            _paddle.AnimationAdd(1, _paddle.playerAnimation);
            _paddle.OnHit = ()=>_paddle.Bounce.Play();
            _segments.AddRange(_paddle.GetSegments());
            _paddle.AnimationManager[1].Start();
            _ball = new Ball(content, SpriteBatch, "Items/ball", new Vector2(380, 840));

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
            if (_iniOn)
                Iniciate();
                        
            Level_Time_lifeleft += gametime.ElapsedGameTime.TotalSeconds;

            // Manage paddle.
            _paddle.Start();

            if (_paddle.visible)
            {
                PaddleMovement(gametime);
            }

            // Manage ball.
            ReleaseBall();
            _ball.Start();

            if (!_ball.Play)
            {
                _ball._circle.Center.Y = _paddle.Position.Y - _ball._circle.Radius;
                _ball._circle.Center.X = _paddle.Position.X + 70;
            }
            // Manage enemy.
            if (_ball.Play == true && EnemyNumber <= 4 ) CountDown(gametime, 5);

            if (Points >= MaxPoints) MaxPoints = Points;

            if (_ball._circle.Center.Y >= D_Limit.Y + 150)
            {
                _ball.Death(); _paddle.Death(); _ball.can_move = false;
                _paddle.can_move = false;
            } 

            // Code for control the segment collisions.
            (float mindistance, Segment collider) = ArkaMath.Collision(_segments, _ball.Direction, _ball._circle.Center);

            if (mindistance < _ball.Speed * (float)gametime.ElapsedGameTime.TotalSeconds)
            {
                Bounces(mindistance, collider, gametime);
            }
            else
            {
                IncreaseBallSpeedOverTime(gametime);                
            }

            Extralife();
                        
            if (NumberBricks <= 0)
            {
                Thread.Sleep(500);
                AchievedLevel();
            }            
            return (_paddle.Life > 0);
        }

        private void LoadBackgraund()
        { 
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
            Vector2 iniposition = new (0,0);
            Vector2 position    = new (0,0);
            Vector2 bricksizeX  = new (61,0); // new (_brick.Size.X,0);
            Vector2 bricksizeY  = new (0,30); // new (0, _brick.Size.Y);

            if (_levelNumber <= _maxlevelNumber)
            {
                string brickLayoutPath = string.Format(@"D:\cODEX\LaysCarpGameStudios\Arkanoid\Content\Levels\BlockLevel0{0}.txt", _levelNumber);
                using StreamReader blockLine = new StreamReader(brickLayoutPath);
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
                    for(int k = 0;k < _currentLevel[i][j].Length; k++)
                    {
                        switch (_currentLevel[i][j][k])
                        {
                            case ',':
                                position += bricksizeX;
                                break;

                            case '.':
                                position += bricksizeX;
                                break;

//all the following cases using just one
///hits:
///use         case default
///use a map (string, tuple)
///map(string, (string, int))
///use tuples
///profit!
  //                              break;
  //
                            case '1':
                                var _brick= new Brick(Hard.Blue, content, SpriteBatch, "Items/BlueBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                _brick.AnimationAdd(1, blast);
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '2':
                                _brick = new Brick(Hard.Yellow, content, SpriteBatch, "Items/YellowBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                glint = new Animations(content, "Animation/Animation_YelowBlock_7", 7, 1, 0.05f);
                                _brick.AnimationAdd(1, blast);
                                _brick.AnimationAdd(2,glint);
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '3':
                                _brick = new Brick(Hard.Green, content, SpriteBatch, "Items/GreenBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                _brick.AnimationAdd(1, blast);
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '4':
                                _brick = new Brick(Hard.Pink, content, SpriteBatch, "Items/PinkBlock", position);
                                _brickList.Add(_brick);
                                _segments.AddRange(_brick.GetSegments());
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                glint = new Animations(content, "Animation/Animation_PinkBlock_7", 7, 1, 0.05f);
                                _brick.AnimationAdd(1, blast);
                                _brick.AnimationAdd(2, glint);
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '5':
                                _brick = new Brick(Hard.Metal, content, SpriteBatch, "Items/MetalBlock", position);
                                glint = new Animations(content, "Animation/Animation_MetalBlock_7", 7, 1, 0.03f);
                                _brick.AnimationAdd(2, glint);
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
            if (_paddle.can_move && KeyState.IsKeyDown(Keys.Left) && _paddle.Position.X > 35f)
                movement = -1;
            if (_paddle.can_move && KeyState.IsKeyDown(Keys.Right) && _paddle.Position.X + _paddle.Size.X < 810f)
                movement = 1;

           _paddle.Position.X += movement * _paddle._paddleDirection.X * _paddle._paddleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            int c = 0;
            foreach (var segment in _paddle.GetSegments())
            {
                _segments[c].ini = segment.ini;
                _segments[c].end = segment.end;
                c++;
            }
        }

        /// <summary>
        /// To beging the ball movement.
        /// </summary>
        public void ReleaseBall()
        {
            var Pushkey = Keyboard.GetState();

            if (_ball.can_move && Pushkey.IsKeyDown(Keys.Space)) _ball.Play = true;

        }

        public void CreateEnemy()
        {
            var text = random.Next(4);
            var pos = random.Next(1,3);
            _enemy = new(text, content, SpriteBatch, pos);
            _enemies.Add(_enemy);
            blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
            _enemy.AnimationAdd(2, blast);
            _enemy.OnHit = () => DestroyEnemy(_enemy);           
            _enemy.EnemyExist = true;
            EnemyNumber++;
        }

        public void AchievedLevel()
        {   
            Dispose();
            
            _levelNumber++;
            if (_levelNumber > 4)
                _levelNumber = 1;

            _brickList.Clear();
            _segments.Clear();
            _enemies.Clear();
            //_enemy = null;
            NumberBricks = 0;
            EnemyNumber = 0;
            Maintext = false;
            NextLevel = true;
            _ball.can_move = false;
            _paddle.can_move = false;

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
            Dispose();
            _brickList.Clear();
            _segments.Clear();
            _enemies.Clear();
            _iniOn = true;
            Maintext = true;
            NextLevel = false;
            _ball.can_move = false;
            _paddle.can_move = false;
            Level_Time_lifeleft = 0;
            _levelNumber = 1;
            EnemyNumber = 0;
            Points = 0;
            NumberBricks = 0;
            ExtraLifePoints = 3000;
        }


        public void Draw(GameTime gameTime)
        {
            
            SpriteBatch.Draw(_backGround, new Vector2(0, 0), Color.White);

            // Score
            string points = Points.ToString();
            SpriteBatch.DrawString(_numberPointFont, points, new Vector2(125,20), Color.WhiteSmoke);

            // High Socre
            string maxpoints = MaxPoints.ToString();
            SpriteBatch.DrawString(_numberPointFont, maxpoints, new Vector2(400,20), Color.WhiteSmoke);
            
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
                if (brick.blas_animation)
                {
                    brick.AnimationManager[brick.animation_key].Update(gameTime);
                    brick.AnimationManager[brick.animation_key].Draw(SpriteBatch, brick.R_blast);
                }
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet. We need call a Dictionary that containt all the animation created.
                if (brick.glin_animation)
                {
                    brick.AnimationManager[brick.animation_key].Update(gameTime);
                    brick.AnimationManager[brick.animation_key].Draw(SpriteBatch, brick.R_Collider);
                }
            }

            if (Level_Time_lifeleft > 2.6)
            {
                _paddle.AnimationManager[1].UpdateLoop(gameTime);
                _paddle.AnimationManager[1].Draw(SpriteBatch, _paddle.Position);             
                _ball.Draw(_ball._circle.Center);
                _ball.can_move = true;
                _paddle.can_move = true;
            }

            foreach (var enemies in _enemies)
            {
                if (enemies!=null)
                {
                    enemies.AnimationManager[1].UpdateLoop(gameTime);
                    enemies.AnimationManager[1].Draw(SpriteBatch, enemies.EnemyPosition);
                }
            }
            
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
                            brick.glin_animation = true;
                            brick.animation_key = 2;
                            brick.AnimationManager[2].Start();
                            break;

                        case Hard.Pink:
                            brick.glin_animation = true;
                            brick.animation_key = 2;
                            brick.AnimationManager[2].Start();
                            break;
                    }
                }

                if (brick.Hit <= 0)
                {
                    foreach (var segment in _segments)
                        segment.ActiveSegment &= segment.owner != brick;

                    brick.visible = false;
                    brick.blas_animation = true;
                    brick.animation_key = 1;
                    brick.AnimationManager[1].Start();
                    brick.DestroyBounce.Play();
                    NumberBricks--;

                    switch (brick.hardness)
                    {
                        case Hard.Blue:
                            Points += 50;
                            break;

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
                brick.glin_animation = true;
                brick.animation_key = 2;
                brick.AnimationManager[2].Start();
                brick.MetalBounce.Play();
            }
        }

        public void DestroyEnemy(Enemy enemy)
        {
            foreach (var segment in _segments)
                segment.ActiveSegment &= segment.owner != enemy;
            enemy.AnimationManager[2].Start();
            enemy.Dead.Play();
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

                _ball._circle.Center += _ball.Speed * _ball.Direction * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (TimeCount > ElapsedTime && _ball.Speed < _ball.Maxspeed)
                {
                    _ball.Speed += 15f;
                    ElapsedTime += 5f;
                }                
            }
        }

        public void Bounces(float minDistance, Segment segment, GameTime gameTime)
        {
            _ball._circle.Center += minDistance * _ball.Direction;
            _ball.Direction = Vector2.Reflect(_ball.Direction, segment.Normal);
            segment.owner.OnHit();
        }

        public void Dispose()
        {
            //Dispose(true);
            GC.SuppressFinalize(this);
        } /*=> content.Unload();*/

        public void CountDown(GameTime gametime, float time)
        {
            TimeCountE += (float)gametime.ElapsedGameTime.TotalSeconds;
            if (time <= TimeCountE)
            {   
                create();
                TimeCountE = 0;
                Console.WriteLine(EnemyNumber);
            }
        }
    }
}       
        
