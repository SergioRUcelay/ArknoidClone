using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static Arkanoid_02.SpriteArk;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Arkanoid_02
{
    public class Level : IDisposable
    {
        public ContentManager content;             

        private Ball _ball;
        private Paddle _paddle;
        public  Animations blast;
        public  Animations glint;
        public Screen screen;

        private readonly SpriteBatch SpriteBatch;
        private SpriteFont _numberPointFont;
        private SpriteFont _lifeLeft;
        private Texture2D _backGround;
        private Texture2D _arkaLogo;
      
        private Song _newlevel;

        private readonly List<Brick> _brickList = new List<Brick>();
        private readonly List<Segment> _segments= new List<Segment>();
        private readonly string[][] _currentLevel = new string [30][];

        private int _levelNumber;
        static double Level_Time_lifeleft;
        public double Total_TimeLevel;
        private const int _maxlevelNumber = 4;
        public int NumberBricks { set; get; }
        private int Points { get; set; }
        private int ExtraLifePoints { get; set; }
        private int MaxPoints { get; set; }

        // Flag for load Iniciate method only the first time call the Update method
        private bool _iniOn;
        // Flag for print Maintext first load level, or wen died. Print the live less on screen.
        public static bool Maintext;
        // Flag for print NextLevel every load new level.
        public static bool NextLevel;
        // Flag to indicate that there was a collision, and call the glow animation.
        public bool glowOnn;

        public Level(IServiceProvider serviceProvider, SpriteBatch spriteBatch)
        {
            content = new ContentManager(serviceProvider, "Content");
            SpriteBatch = spriteBatch;
            _levelNumber = 2;            
            NumberBricks = 0;
            ExtraLifePoints = 6000;
            Points = 0;
            MaxPoints = 50000;            
            _iniOn = true;
            Maintext = true;
            NextLevel = false;
            glowOnn = false;
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

            // GameObjets creation.
            _paddle = new Paddle(content, SpriteBatch,"Items/Player", new Vector2(365, 810));
            _paddle.AnimationAdd(1, _paddle.playerAnimation);
            _paddle.OnHit = ()=>_paddle.Bounce.Play();
            _segments.AddRange(_paddle.GetSegments());
            _paddle.AnimationManager[1].Start();
            _ball = new Ball(content, SpriteBatch, "Items/ball", new Vector2(380, 840));
            
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
                //playerAnimation.Update(time);
            }

            // Manage ball.
            ReleaseBall();
            _ball.Start();

            if (!_ball.Play)
            {
                _ball.Position.Y = _paddle.Position.Y - _ball.Size.Y;
                _ball.Position.X = _paddle.Position.X + 60;
            }

            //PointAnimation(gametime);

            if (Points >= MaxPoints) MaxPoints = Points;

            if (!_ball.WallBounce()) { _ball.Death(); _paddle.Death(); _ball.can_move = false;
                _paddle.can_move = false;}

           // if (PlayerCollision() && _ball.Play) { _paddle.Bounce.Play(); }

            (float mindistance, Segment collider) = ArkaMath.Collision(_segments, _ball.Direction, _ball.Position);

            Bounces(mindistance, collider, gametime);            

            //BrickCollision(_brickList);
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
            Vector2 position = new (0,0);
            Vector2 bricksizeX = new(61, 0); // new (_brick.Size.X,0);
            Vector2 bricksizeY = new(0, 30); // new (0, _brick.Size.Y);

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
        /// To beging the ball moviment.
        /// </summary>
        public void ReleaseBall()
        {
            var Pushkey = Keyboard.GetState();

            if (_ball.can_move && Pushkey.IsKeyDown(Keys.Space)) _ball.Play = true;

        }

        public bool PlayerCollision()
        {
            if (_paddle.visible == false) return false;

            if (_paddle.Position.X + _paddle.Size.X > _ball.Position.X && (_ball.Position.X + _ball.Size.X) > _paddle.Position.X &&
                    (_paddle.Position.Y + _paddle.Size.Y) > _ball.Position.Y && (_ball.Position.Y + _ball.Size.Y) > _paddle.Position.Y)
            {
                // Right
                if (_ball.Position.X > _paddle.Position.X && _ball.Position.X + _ball.Size.X > _paddle.Position.X + _paddle.Size.X && _ball.Direction.X < 0)
                {
                    _ball.Bounce(new Vector2(1, 0));
                }

                // Left
                if (_ball.Position.X < _paddle.Position.X && _ball.Position.X + _ball.Size.X < _paddle.Position.X + _paddle.Size.X && _ball.Direction.X > 0)
                {
                    _ball.Bounce(new Vector2(1, 0));
                }

                // Top
                if (_ball.Position.Y < _paddle.Position.Y && _ball.Position.Y + _ball.Size.Y < _paddle.Position.Y + _paddle.Size.Y && _ball.Direction.Y > 0)
                {
                    _ball.Bounce(new Vector2(0, 1));
                }

                // Botton
                if (_ball.Position.Y > _paddle.Position.Y && _ball.Position.Y + _ball.Size.Y > _paddle.Position.Y + _paddle.Size.Y && _ball.Position.X < _paddle.Position.X + _paddle.Size.X && _ball.Direction.Y < 0)
                {
                    _ball.Bounce(new Vector2(0, 1));
                }
                return true;
            }
            return false;
        }
        public bool BrickCollision(List<Brick> bricksOrder)
        {
            foreach (Brick brick in bricksOrder)
            {
                bool on = true;
                if (brick.visible)
                {
                    if (_ball.R_Collider.Intersects(brick.R_Collider))
                    {
                     
                        // Right
                        if (_ball.Position.X > brick.Position.X && _ball.Position.X + _ball.Size.X > brick.Position.X + brick.Size.X && _ball.Direction.X < 0 && on)
                        {
                            _ball.Bounce(new Vector2(1, 0));
                            on = false;
                        }

                        // Left
                        if (_ball.Position.X < brick.Position.X && _ball.Position.X + _ball.Size.X < brick.Position.X + brick.Size.X && _ball.Direction.X > 0 && on)
                        {
                            _ball.Bounce(new Vector2(1, 0));
                            on = false;
                        }

                        // Top
                        if (_ball.Position.Y < brick.Position.Y && _ball.Position.Y + _ball.Size.Y < brick.Position.Y + brick.Size.Y && _ball.Direction.Y > 0 && on)
                        {
                            _ball.Bounce(new Vector2(0, 1));
                            on = false;
                        }

                        // Botton
                        if (_ball.Position.Y > brick.Position.Y && _ball.Position.Y + _ball.Size.Y > brick.Position.Y + brick.Size.Y && _ball.Position.X < brick.Position.X + brick.Size.X && _ball.Direction.Y < 0 && on)
                        {
                            _ball.Bounce(new Vector2(0, -1));
                           
                        }
                        
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
                                        brick.ani_key = 2;
                                        brick.AnimationManager[2].Start();
                                        break;

                                    case Hard.Pink:
                                        brick.glin_animation = true;
                                        brick.ani_key = 2;
                                        brick.AnimationManager[2].Start();
                                        break;
                                }
                            }


                            if (brick.Hit <= 0)
                            {
                                brick.SetVisible(false);
                                brick.blas_animation = true;
                                brick.ani_key = 1;
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
                            brick.ani_key = 2;
                            brick.AnimationManager[2].Start();
                            brick.MetalBounce.Play();
                        }
                        
                    }
                }

            }
            return false;
        }
      
        public void AchievedLevel()
        {   
            Dispose();
            
            _levelNumber++;
            if (_levelNumber > 4)
                _levelNumber = 1;

            _brickList.Clear();
            _segments.Clear();
            NumberBricks = 0;
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
            _iniOn = true;
            Maintext = true;
            NextLevel = false;
            _ball.can_move = false;
            _paddle.can_move = false;
            Level_Time_lifeleft = 0;
            _levelNumber = 1;
            Points = 0;
            NumberBricks = 0;
            ExtraLifePoints = 3000;
        }


        public void Draw(GameTime gameTime)
        {

            SpriteBatch.Draw(_backGround, new Vector2(0, 0), Color.White);

            if(_segments[3].ActiveSegment == true)
                SpriteBatch.Draw(_paddle.myTexture, new Rectangle((int)_segments[3].end.X, (int)_segments[3].end.Y, (int)(_segments[3].ini.X - _segments[3].end.X), 10), Color.DarkOliveGreen);
            
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
                    brick.AnimationManager[brick.ani_key].Update(gameTime);
                    brick.AnimationManager[brick.ani_key].Draw(SpriteBatch, brick.R_blast);
                }
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet. We need call a Dictionary that containt all the animation created.
                if (brick.glin_animation)
                {
                    brick.AnimationManager[brick.ani_key].Update(gameTime);
                    brick.AnimationManager[brick.ani_key].Draw(SpriteBatch, brick.R_Collider);
                }
            }

            if (Level_Time_lifeleft > 2.6)
            {
                
                _paddle.AnimationManager[1].UpdateLoop(gameTime);
               // _paddle.AnimationManager[1].Draw(SpriteBatch, _paddle.Position);             
                _ball.Draw(gameTime);
                _ball.can_move = true;
                _paddle.can_move = true;
            }

        }
        public static void Mensage()

        {
            Console.WriteLine("Ouch!!!! ma dao, ma dao");
        }
        public void DestroyBrick(Brick brick)
        {

            //foreach (var segment in _segments)
            //    segment.ActiveSegment &= segment.owner != brick;
            //brick.visible = false;

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
                            brick.ani_key = 2;
                            brick.AnimationManager[2].Start();
                            break;

                        case Hard.Pink:
                            brick.glin_animation = true;
                            brick.ani_key = 2;
                            brick.AnimationManager[2].Start();
                            break;
                    }
                }


                if (brick.Hit <= 0)
                {
                    foreach (var segment in _segments)
                        segment.ActiveSegment &= segment.owner != brick;
                    brick.SetVisible(false);
                    brick.blas_animation = true;
                    brick.ani_key = 1;
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

        }

        public void PointAnimation(GameTime gameTime)
        {
            if (_ball.Play)
                _ball.Position += _ball.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds * _ball.Direction;
        }

        public void Bounces(float minDistance, Segment segment, GameTime gameTime)
        {
            if (minDistance < _ball.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds)
            { 

                _ball.Position += minDistance * _ball.Direction;
                _ball.Direction = Vector2.Reflect(_ball.Direction, segment.Normal);
                segment.owner.OnHit();
                //Bounces(mindistance, collider);

            }
            else PointAnimation(gameTime);
            //_ball.Position += minDistance * _ball.Direction;
            //_ball.Direction = Vector2.Reflect(_ball.Direction, segment.Normal);
        }

        public void HitBrick(Brick _brick)
        {
            if (_brick.visible)
            {
                
                _brick.Hit--;
                if (_brick.Hit >= 1)
                    _brick.BrickBounce.Play();

                if (_brick.Hit <= 0)
                {
                    _brick.SetVisible(false);
                    _brick.DestroyBounce.Play();
                    NumberBricks--;

                    switch (_brick.hardness)
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
            if (!_brick.destructible)
                _brick.MetalBounce.Play();
        }

        public void Dispose() => content.Unload();

     }
}       
        
