using Arkanoid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using System.Threading;
using System.Diagnostics;

namespace Arkanoid_02
{
    enum GameState { MENU, PREPLAY, PLAY, GAMEOVER };
    public class ArkaGame : Game
    {
        private GameState gameState = GameState.MENU;
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private readonly Timer Enemy_timer          = new(5f);
        private readonly Timer LevelText_timer      = new (2.2f);
        private readonly Timer GameOverScreen_timer = new (3f);
        private readonly Timer LifeText_timer       = new (2.8f);
        private SpriteFont _numberPointFont, _lifeLeft;        
        private Texture2D _arkaLogo;

        // Vectors describing the boundaries of the screen.
        private Vector2 B_Limit, A_Limit, C_Limit, D_Limit;

        // Game Objets
        private Level level;
        private Screen screen;
        private Shapes shapes;
        private Brick screeLimit;
        private Ball ball;
        private Paddle paddle;

        private Song NewLevelSound;
        private SoundEffect _ballWallBounce;
        private readonly Door[] _doors = new Door[2];
        private static readonly List<Enemy> _enemies = new();
        public static readonly List<Segment> _segments = new();        
        public static  int Points { get; set; }
        private int MaxPoints { get; set; }
        private int ExtraLifePoints { get; set; }
        private int EnemyNumber { get; set; }

        private bool ShowLife;
        private bool ShowLevel;

        // This two variable manages time for speed increment of ball in "IncreaseBallSpeedOverTime" method.
        private float timeCount;
        private float ElapsedTime = 10f;

        public ArkaGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            graphics.PreferredBackBufferWidth  = 843;
            graphics.PreferredBackBufferHeight = 900;
            A_Limit = new Vector2(25, 100);
            B_Limit = new Vector2(800, 100);
            C_Limit = new Vector2(800, 875);
            D_Limit = new Vector2(25, 875);

            Points          = 0;
            MaxPoints       = 50000;
            ExtraLifePoints = 12000;
            EnemyNumber     = 0;

            ShowLife = false;
            ShowLevel = true;
        }

        protected override void Initialize()
        {
            spriteBatch     = new SpriteBatch(GraphicsDevice);
            shapes          = new Shapes(this);
            level           = new Level(Services, spriteBatch,Content);
            screen          = new Screen(Services, spriteBatch);
            screeLimit      = new Brick(Hard.Blue, Content, spriteBatch, "Items/BlueBlock", Vector2.Zero);
            paddle          = new Paddle(Content, spriteBatch, "Items/Player", new Vector2(365, 810));
            ball            = new Ball(Content, spriteBatch, "Items/ball", new Vector2(380, 840));
            _doors[0]       = new Door(Content, spriteBatch, "Items/door", new Vector2(182, 75));
            _doors[1]       = new Door(Content, spriteBatch, "Items/door", new Vector2(573, 75));

            Enemy_timer.OnMatured += async () =>
            {
                var e = new Random().Next(0, 2);
                Door.DoorOpenCast(_doors[e]);
                await Task.Delay(800);
                OutEnemy(e);
                await Task.Delay(400);
                Door.DoorCloseCast(_doors[e]);
            };

            LevelText_timer.OnMatured += () =>
            {               
                paddle.Start();
                gameState = GameState.PLAY;
                _segments.AddRange(paddle.GetSegments());
                paddle.OnHit = () => paddle.Bounce.Play();
                _segments.AddRange(screeLimit.GetScreenSegment(A_Limit, B_Limit, C_Limit, D_Limit));
                screeLimit.OnHit = () => _ballWallBounce.Play();
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {            
            _arkaLogo           = Content.Load<Texture2D>("Items/ArkaLogo");
            _numberPointFont    = Content.Load<SpriteFont>("Fonts/Points");
            _lifeLeft           = Content.Load<SpriteFont>("Fonts/Points");
            NewLevelSound       = Content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");
            _ballWallBounce     = Content.Load<SoundEffect>("Sounds/WallBounce");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            switch(gameState)
            {
                case GameState.MENU:
                    screen.WelcomeScreen(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.P))
                    {
                        level._brickList.Clear();
                        _segments.Clear();
                        _enemies.Clear();
                        level._levelNumber = 1;
                        Points = 0;
                        GoToPreplay(gameTime);
                        paddle.Life = 3;
                        level.Iniciate();
                        gameState = GameState.PREPLAY;
                    }
                break;

                case GameState.PREPLAY:                    
                    if(ShowLevel)
                        LevelText_timer.CountDown(gameTime);
                    if (ShowLife)                    
                        LifeText_timer.CountDown(gameTime);
                    Enemy_timer.Reset(gameTime);
                LifeText_timer.OnMatured += () =>
                {
                    gameState = GameState.PLAY;
                    paddle.Active = true;
                };
                break;

                case GameState.PLAY:
                    //Manage paddle.
                    Debug.Assert(paddle.Active);
                    PaddleMovement(gameTime);

                    //Manage ball.
                    ball.StartUpAndReposition();
                    ReleaseBall();
                    ControlsLowerLimitBallPosition(gameTime);

                    //Manage Collision.
                    CollisionAndMotionController(gameTime);
                    Extralife();
                    if (level.NumberBricks == 0)
                    {
                        LevelCompleted(gameTime);
                        gameState = GameState.PREPLAY;
                    }
                    if (paddle.Life == 0)
                    {
                        GameOverScreen_timer.Reset(gameTime);
                        gameState = GameState.GAMEOVER;
                    }

                    //Manage Enemies.
                    Enemies(gameTime);
                    break;

                case GameState.GAMEOVER:
                    ShowLevel = true;
                    ShowLife = false;
                    ball.Active = false;
                    screen.ScreenBlackGameOver(gameTime);
                    GameOverScreen_timer.CountDown(gameTime);
                    GameOverScreen_timer.OnMatured += () => gameState = GameState.MENU;
                break;
            }           
            base.Update(gameTime);       
        }
        
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (gameState == GameState.PLAY || gameState == GameState.PREPLAY)
            {
                level.Draw(gameTime);
                // Score
                string points = Points.ToString();
                spriteBatch.DrawString(_numberPointFont, points, new Vector2(125, 20), Color.WhiteSmoke);

                // High Socre
                string maxpoints = MaxPoints.ToString();
                spriteBatch.DrawString(_numberPointFont, maxpoints, new Vector2(400, 20), Color.WhiteSmoke);

                // Round
                string l_Number = level._levelNumber.ToString();
                spriteBatch.DrawString(_numberPointFont, l_Number, new Vector2(635, 34), Color.WhiteSmoke);

                // Lives
                string l_life = paddle.Life.ToString();
                spriteBatch.DrawString(_numberPointFont, l_life, new Vector2(775, 10), Color.WhiteSmoke);

                // Doors
                foreach (var door in _doors)
                {
                    if (door.Active) door.Draw(gameTime);
                    if (door._openDoor.AnimaActive)
                    {
                        door._openDoor.Draw(spriteBatch, door.DoorPosition);
                        door._openDoor.Update(gameTime);
                    }

                    if (door._closeDoor.AnimaActive)
                    {
                        door._closeDoor.Draw(spriteBatch, door.DoorPosition);
                        door._closeDoor.Update(gameTime);
                    }
                }

                if (gameState == GameState.PREPLAY)
                {
                    if (ShowLevel)                    
                        LevelText();
                    if (ShowLife)
                        LifeLeftText();
                }
            }

            if (gameState == GameState.PLAY)
            {
                paddle.PlayerAnimation.UpdateLoop(gameTime);
                paddle.PlayerAnimation.Draw(spriteBatch, paddle.Position);
                ball.Draw(ball._circle.Center);

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

            spriteBatch.End();

            // ----------------------------- This code displays the segments on the screen. ----------------------------
            //shapes.Begin();
            //if (gameState == GameState.PLAY)
            //{
            //    foreach (var seg in _segments)
            //    {
            //        if (seg.ActiveSegment)
            //            shapes.Drawline(new(seg.Ini.X, 900 - seg.Ini.Y), new(seg.End.X, 900 - seg.End.Y), 1, Color.White);
            //    }
            //}
            //shapes.End();

            base.Draw(gameTime);
        }

        private void GoToPreplay(GameTime gameTime)
        {
            LevelText_timer.Reset(gameTime);
            MediaPlayer.Play(NewLevelSound);
            screen.playOn = true;
        }

        private void LifeLeftText()
        {
            spriteBatch.Draw(_arkaLogo, new Vector2(320, 620), Color.White);
            spriteBatch.DrawString(_lifeLeft, paddle.Life + "  LIVES  LEFT", new Vector2(290, 700), Color.White);
            spriteBatch.DrawString(_lifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        private void LevelText()
        {
            spriteBatch.Draw(_arkaLogo, new Vector2(320, 620), Color.White);
            spriteBatch.DrawString(_lifeLeft, level._levelNumber + "   ROUND", new Vector2(345, 700), Color.White);
            spriteBatch.DrawString(_lifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        /// <summary>
        /// Manages the paddle`s movement
        /// </summary>
        /// <param name="gameTime"></param>
        public void PaddleMovement(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();
            int movement = 0;
            if (paddle.Can_move && keyState.IsKeyDown(Keys.Left) && paddle.Position.X > 35f)
                movement = -1;
            if (paddle.Can_move && keyState.IsKeyDown(Keys.Right) && paddle.Position.X + paddle.Size.X < 810f)
                movement = 1;
            paddle.Position.X += movement * paddle._paddleDirection.X * paddle._paddleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// To beging the ball movement.
        /// </summary>
        public void ReleaseBall()
        {
            if (ball.Attach)
            {
                ball._circle.Center.Y = paddle.Position.Y;
                ball._circle.Center.X = paddle.Position.X + 70;
            }
            var pushkey = Keyboard.GetState();
            if (ball.Active && pushkey.IsKeyDown(Keys.Space)) ball.Attach = false;
        }

        // Code for control the segment collision with a Circle/Ball.
        private void CollisionAndMotionController(GameTime gameTime)
        {            
            ball._circle.Center += ball.Speed * ball.Direction * (float)gameTime.ElapsedGameTime.TotalSeconds;

            var collide = ArkaMath.CollideWithWorld(_segments, ball.Direction, ball._circle.Center, ball._circle.Radius);
            if (collide.depth > 0)
            {
                ball._circle.Center -= ball.Direction * collide.depth;
                ball.Direction = Vector2.Reflect(ball.Direction, collide.Normal);
                collide.seg.Owner.OnHit();
            }
            else
                IncreaseBallSpeedOverTime(gameTime);
        }
        //private void EnemyCollisionAndMotionController(GameTime gameTime)
        //{
        //    foreach (var enemy in _enemies)
        //    {
        //        enemy.SpinCenter += enemy.EnemyDirection * (float)gameTime.ElapsedGameTime.TotalSeconds;

        //        var collide = ArkaMath.CollideWithWorld(_segments, ball.Direction, ball._circle.Center, ball._circle.Radius);
        //        if (collide.depth > 0)
        //        {
        //            ball._circle.Center -= ball.Direction * collide.depth;
        //            ball.Direction = Vector2.Reflect(ball.Direction, collide.Normal);
        //            collide.seg.Owner.OnHit();
        //        }
        //        else
        //            enemy.EnemyCircleMovement(); 
                
        //    }
        //}
        /// <summary>
        /// Animate the ball and the increasing his speed over time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void IncreaseBallSpeedOverTime(GameTime gameTime)
        {
            if (ball.Active && !ball.Attach)
            {
                timeCount += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeCount > ElapsedTime && ball.Speed < ball.Maxspeed)
                {
                    ball.Speed += 30f;
                    ElapsedTime += 5f;
                }
            }
        }

        /// <summary>
        /// Controls if the ball drops, and kills it.
        /// </summary>
        private void ControlsLowerLimitBallPosition(GameTime gameTime)
        {
            if (ball._circle.Center.Y >= D_Limit.Y + 100)
            {
                ball.Death();
                paddle.Death();
                EnemyScreenErase();
                EnemyNumber = 0;
                _enemies.Clear();
                ball.Active = false;
                paddle.Start();
                ShowLife    = true;
                ShowLevel   = false;
                LifeText_timer.Reset(gameTime);
                GoToPreplay(gameTime);
                gameState = GameState.PREPLAY;
            }
        }
        
        // Manage Enemy.
        private void Enemies(GameTime gameTime)
        {           
            if (ball.Active == true && ball.Attach == false && EnemyNumber < 5)
                Enemy_timer.CountDown(gameTime);
            else
                Enemy_timer.Reset(gameTime);

            foreach (var enemies in _enemies)
            {
                enemies.Animation();
                if (enemies.Position.Y >= D_Limit.Y + 20)
                    FallEnemy(enemies);
            }
        }

        public void OutEnemy(int enemyPos)
        {
            var enemyT = new Random().Next((int)EnemyType.LAST - 1);
            Enemy _enemy = new((EnemyType)enemyT, Content, spriteBatch, enemyPos);
            _enemies.Add(_enemy);
            _enemy.OnHit = () => DestroyEnemy(_enemy);
            _enemy.Animation = _enemy.EnemyCircleMovement;            
            _segments.AddRange(_enemy.GetSegments());
            EnemyNumber++;
        }

        public void DestroyEnemy(Enemy enemy)
        {
            if (enemy.Active)
            {
                foreach (var segment in _segments)
                    segment.ActiveSegment &= segment.Owner != enemy;
                enemy.Blast.Start();
                enemy.Dead.Play();
                enemy.Active = false;
                EnemyNumber--;
            }
        }

        public void FallEnemy(Enemy enemy)
        {
            EnemyNumber--;
            foreach (var segment in _segments)
                    segment.ActiveSegment &= segment.Owner != enemy;
            enemy.Active = false;
        }

        private static void EnemyScreenErase()
        {
            foreach (var enemy in _enemies)
                _segments.RemoveAll(segment => segment.Owner == enemy); 
        }

        public void Extralife()
        {
            if (Points >= ExtraLifePoints)
            {
                ExtraLifePoints += Points;
                paddle.Life += 1;
                paddle.ExtraLife.Play();
            }
        }

        public void LevelCompleted(GameTime gameTime)
        {
            level._levelNumber++;
            if (level._levelNumber > 4)
                level._levelNumber = 1;
            level._brickList.Clear();
            _segments.Clear();
            _enemies.Clear();
            level.NumberBricks = 0;
            EnemyNumber = 0;
            ShowLife = false;
            ShowLevel = true;
            ball.Active = false;
            LevelText_timer.Reset(gameTime);
            Thread.Sleep(1000);
            GoToPreplay(gameTime);
            level.Iniciate();
        }
    }
}
