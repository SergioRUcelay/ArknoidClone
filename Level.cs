using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static Game_Arka.SpriteArk;

namespace Arkanoid_02
{
    public class Level : IDisposable
    {
        public ContentManager content;             

        private Ball _ball;
        private Player _player;
        private Brick _brick;
        public  Animations blast;
        public  Animations glint;
        public Screen screen;

        private readonly SpriteBatch SpriteBatch;
        private SpriteFont _numberPointFont;
        private SpriteFont _lifeLeft;
        private Texture2D _backGround;
        private Texture2D _arkaLogo;
      
        private Song _newlevel;
       
        private Vector2 _backGroundPosition;
        private Vector2 _playerPosition;
        private Vector2 _ballPosition;

        private readonly List<Brick> _brickList = new List<Brick>();
        private readonly string[][] _currentLevel = new string [30][];

        private int _levelNumber;
        public static double Time_lifeleft;
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
            _backGroundPosition = new Vector2(0, 0);
            _playerPosition = new Vector2(365, 810);
            _ballPosition = new Vector2(380, 840);
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

            _player = new Player(content, SpriteBatch,"Items/Player", _playerPosition);
            _player.AnimationAdd(1, _player.playerAnimation);
            _player.ani_manager[1].Start();
            _ball = new Ball(content, SpriteBatch, "Items/ball", _ballPosition);

            BrickLayout(content);
            MediaPlayer.Play(_newlevel);           

            Time_lifeleft = 0;
            Total_TimeLevel = 0;
            
            _iniOn = false;
           
        }

        public bool Update(GameTime gametime)
        {
            if (_iniOn)
                Iniciate();
                        
            Time_lifeleft += gametime.ElapsedGameTime.TotalSeconds;

            _player.Update(gametime);
            _ball.Update(gametime, ref _player.position);

            if (Points >= MaxPoints) MaxPoints = Points;

            if (!_ball.WallBounce()) { _ball.Death(); _player.Death(); _ball.can_move = false;
                _player.can_move = false;}

            if (PlayerCollision() && _ball.Play) { _player.Bounce.Play(); }
            
            BrickCollision(_brickList);
            Extralife();
                        
            if (NumberBricks <= 0)
            {
                Thread.Sleep(500);
                AchievedLevel();
            }
            return (_player.Life > 0);
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
            SpriteBatch.DrawString(_lifeLeft, _player.Life + "  LIVES  LEFT", new Vector2(290, 700), Color.White);
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
            Vector2 bricksizeX = new (61,0);
            Vector2 bricksizeY = new (0, 30);

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
                                _brick= new Brick(Hard.Blue, content, SpriteBatch, "Items/BlueBlock", position);
                                _brickList.Add(_brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                _brick.AnimationAdd(1, blast);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '2':
                                _brick = new Brick(Hard.Yellow, content, SpriteBatch, "Items/YellowBlock", position);
                                _brickList.Add(_brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                glint = new Animations(content, "Animation/Animation_YelowBlock_7", 7, 1, 0.05f);
                                _brick.AnimationAdd(1, blast);
                                _brick.AnimationAdd(2,glint);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '3':
                                _brick = new Brick(Hard.Green, content, SpriteBatch, "Items/GreenBlock", position);
                                _brickList.Add(_brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                _brick.AnimationAdd(1, blast);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '4':
                                _brick = new Brick(Hard.Pink, content, SpriteBatch, "Items/PinkBlock", position);
                                _brickList.Add(_brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.06f);
                                glint = new Animations(content, "Animation/Animation_PinkBlock_7", 7, 1, 0.05f);
                                _brick.AnimationAdd(1, blast);
                                _brick.AnimationAdd(2, glint);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '5':
                                _brick = new Brick(Hard.Metal, content, SpriteBatch, "Items/MetalBlock", position);
                                glint = new Animations(content, "Animation/Animation_MetalBlock_7", 7, 1, 0.03f);
                                _brick.AnimationAdd(2, glint);
                                _brickList.Add(_brick);
                                position += bricksizeX;
                                break;
                        }

                    }
                    position.X = iniposition.X;
                }
                position.Y += bricksizeY.Y;
            }

        }
              
        public bool PlayerCollision()
        {
            if (_player.visible == false) return false;

            if (_player.position.X + _player.Size.X > _ball.position.X && (_ball.position.X + _ball.Size.X) > _player.position.X &&
                    (_player.position.Y + _player.Size.Y) > _ball.position.Y && (_ball.position.Y + _ball.Size.Y) > _player.position.Y)
            {
                // Right
                if (_ball.position.X > _player.position.X && _ball.position.X + _ball.Size.X > _player.position.X + _player.Size.X && _ball.velocity.X < 0)
                {
                    _ball.Bounce(new Vector2(1, 0));
                }

                // Left
                if (_ball.position.X < _player.position.X && _ball.position.X + _ball.Size.X < _player.position.X + _player.Size.X && _ball.velocity.X > 0)
                {
                    _ball.Bounce(new Vector2(1, 0));
                }

                // Top
                if (_ball.position.Y < _player.position.Y && _ball.position.Y + _ball.Size.Y < _player.position.Y + _player.Size.Y && _ball.velocity.Y > 0)
                {
                    _ball.Bounce(new Vector2(0, 1));
                }

                // Botton
                if (_ball.position.Y > _player.position.Y && _ball.position.Y + _ball.Size.Y > _player.position.Y + _player.Size.Y && _ball.position.X < _player.position.X + _player.Size.X && _ball.velocity.Y < 0)
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
                        if (_ball.position.X > brick.position.X && _ball.position.X + _ball.Size.X > brick.position.X + brick.Size.X && _ball.velocity.X < 0 && on)
                        {
                            _ball.Bounce(new Vector2(1, 0));
                            on = false;
                        }

                        // Left
                        if (_ball.position.X < brick.position.X && _ball.position.X + _ball.Size.X < brick.position.X + brick.Size.X && _ball.velocity.X > 0 && on)
                        {
                            _ball.Bounce(new Vector2(1, 0));
                            on = false;
                        }

                        // Top
                        if (_ball.position.Y < brick.position.Y && _ball.position.Y + _ball.Size.Y < brick.position.Y + brick.Size.Y && _ball.velocity.Y > 0 && on)
                        {
                            _ball.Bounce(new Vector2(0, 1));
                            on = false;
                        }

                        // Botton
                        if (_ball.position.Y > brick.position.Y && _ball.position.Y + _ball.Size.Y > brick.position.Y + brick.Size.Y && _ball.position.X < brick.position.X + brick.Size.X && _ball.velocity.Y < 0 && on)
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
                                        brick.ani_manager[2].Start();
                                        break;

                                    case Hard.Pink:
                                        brick.glin_animation = true;
                                        brick.ani_key = 2;
                                        brick.ani_manager[2].Start();
                                        break;
                                }
                            }


                            if (brick.Hit <= 0)
                            {
                                brick.SetVisible(false);
                                brick.blas_animation = true;
                                brick.ani_key = 1;
                                brick.ani_manager[1].Start();
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
                            brick.ani_manager[2].Start();
                            brick.MetalBounce.Play();
                        }
                        
                    }
                }

            }
            return false;
        }
        //public bool BrickCollision(List<Brick> bricksOrder)
        //{
        //    foreach (Brick brick in bricksOrder)
        //    {
        //        if (brick.visible)
        //        {
        //            if (brick.position.X + brick.Size.X > ball.position.X && (ball.position.X + ball.Size.X) > brick.position.X &&
        //                (brick.position.Y + brick.Size.Y) > ball.position.Y && (ball.position.Y + ball.Size.Y) > brick.position.Y)
        //            {
        //                // Right
        //                if (ball.position.X > brick.position.X && ball.position.X + ball.Size.X > brick.position.X + brick.Size.X && ball.velocity.X < 0)
        //                {
        //                    ball.Bounce(new Vector2(1, 0));
        //                    continue;
        //                }

        //                // Left
        //                if (ball.position.X < brick.position.X && ball.position.X + ball.Size.X < brick.position.X + brick.Size.X && ball.velocity.X > 0)
        //                {
        //                    ball.Bounce(new Vector2(1, 0));
        //                    continue;
        //                }

        //                // Top
        //                if (ball.position.Y < brick.position.Y && ball.position.Y + ball.Size.Y < brick.position.Y + brick.Size.Y && ball.velocity.Y > 0)
        //                {
        //                    ball.Bounce(new Vector2(0, 1));
        //                    continue;
        //                }

        //                // Botton
        //                if (ball.position.Y > brick.position.Y && ball.position.Y + ball.Size.Y > brick.position.Y + brick.Size.Y && ball.position.X < brick.position.X + brick.Size.X && ball.velocity.Y < 0)
        //                {
        //                    ball.Bounce(new Vector2(0, -1));
        //                    continue;
        //                }

        //                if (brick.destructible)
        //                {
        //                    brick.hit--;
        //                    if (brick.hit >= 1)
        //                        brick.BrickBounce.Play();

        //                    if (brick.hit <= 0)
        //                    {
        //                        brick.SetVisible(false);
        //                        brick.DesdtroyBounce.Play();
        //                        NumberBricks--;

        //                        switch (brick.hardness)
        //                        {
        //                            case Hard.Blue:
        //                                Points += 50;
        //                                break;

        //                            case Hard.Green:
        //                                Points += 50;
        //                                break;

        //                            case Hard.Yellow:
        //                                Points += 100;
        //                                break;

        //                            case Hard.Pink:
        //                                Points += 300;
        //                                break;
        //                        }

        //                    }

        //                }
        //                if (!brick.destructible)
        //                    brick.MetalBounce.Play();
        //            }
        //        }

        //    }
        //    return false;
        //}

        public void AchievedLevel()
        {   
            Dispose();
            
            _levelNumber++;
            if (_levelNumber > 4)
                _levelNumber = 1;

            _brickList.Clear();
            NumberBricks = 0;
            Maintext = false;
            NextLevel = true;
            _ball.can_move = false;
            _player.can_move = false;

            Iniciate();
           
        }

        public void Extralife()
        {
            if (Points >= ExtraLifePoints)
            {
                ExtraLifePoints += Points;
                _player.Life += 1;
                _player.ExtraLife.Play();

            }
        }

        public void GameOver()
        {
            Dispose();
            _brickList.Clear();
            _iniOn = true;
            Maintext = true;
            NextLevel = false;
            _ball.can_move = false;
            _player.can_move = false;
            Time_lifeleft = 0;
            _levelNumber = 1;
            Points = 0;
            NumberBricks = 0;
            ExtraLifePoints = 3000;
        }


        public void Draw(GameTime gameTime)
        {

            SpriteBatch.Draw(_backGround, _backGroundPosition, Color.White);
            
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
            string l_life = _player.Life.ToString();
            SpriteBatch.DrawString(_numberPointFont, l_life, new Vector2(775, 10), Color.WhiteSmoke);

            if (Maintext)
            {
                if (Time_lifeleft < 2.5)
                    MainText();
            }

            if (NextLevel)
            {
                if (Time_lifeleft < 2.5)
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
                    brick.ani_manager[brick.ani_key].Update(gameTime);
                    brick.ani_manager[brick.ani_key].Draw(SpriteBatch, brick.R_blast);
                }
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet. We need call a Dictionary that containt all the animation created.
                if (brick.glin_animation)
                {
                    brick.ani_manager[brick.ani_key].Update(gameTime);
                    brick.ani_manager[brick.ani_key].Draw(SpriteBatch, brick.R_Collider);
                }
            }

            if (Time_lifeleft > 2.6)
            {
                
                _player.ani_manager[1].UpdateLoop(gameTime);
                _player.ani_manager[1].Draw(SpriteBatch, _player.position);             
                _ball.Draw(gameTime);
                _ball.can_move = true;
                _player.can_move = true;
            }

        }

        public void Dispose() => content.Unload();

     }
}       
        
