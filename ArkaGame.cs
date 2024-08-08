using Arkanoid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace Arkanoid_02
{
    enum GameState { MENU, PREPLAY, PLAY, GAMEOVER };
    public class ArkaGame : Game
    {
        private GameState gameState = GameState.MENU;
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private readonly Timer EnemyTimer     = new(5f);
        private readonly Timer mainText       = new (2.5f);
        private readonly Timer GameOverScreen = new (3f);
        private readonly Timer NextLevelText  = new (2.5f);
        private SpriteFont _numberPointFont;
        private SpriteFont _lifeLeft;
        private Texture2D _backGround;
        private Texture2D _scoreZone;
        private Texture2D _arkaLogo;

        // Vectors describing the boundaries of the screen.
        private Vector2 B_Limit, A_Limit, C_Limit, D_Limit;

        private Song NewLevelSound;

        // Game Objets
        private Level level;
        private Screen screen;
        private Shapes shapes;
        private Brick screeLimit;
        private Ball ball;
        private Paddle paddle;

        private readonly SoundEffect _ballWallBounce;
        private readonly Door[] _doors = new Door[2];
        public static readonly List<Segment> _segments = new();
        private static readonly List<Enemy> _enemies = new();
        private Random Random { get; set; }

        public static  int Points { get; set; }
        private int MaxPoints { get; set; }
        private int ExtraLifePoints { get; set; }

        public int EnemyNumber { get; set; }

        // This two variable manages time for speed increment of ball in "IncreaseBallSpeedOverTime" method.
        private float timeCount;
        private float ElapsedTime = 10f;

        public int ActualTime;

        public ArkaGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            graphics.PreferredBackBufferWidth = 843;
            graphics.PreferredBackBufferHeight = 900;

            _ballWallBounce = Content.Load<SoundEffect>("Sounds/WallBounce");

            A_Limit = new Vector2(25, 100);
            B_Limit = new Vector2(800, 100);
            C_Limit = new Vector2(800, 875);
            D_Limit = new Vector2(25, 875);

            Points          = 0;
            MaxPoints       = 50000;
            ExtraLifePoints = 6000;
            EnemyNumber     = 0;
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

            EnemyTimer.OnMatured += async () =>
            {
                var e = new Random().Next(0, 2);
                Door.DoorOpenCast(_doors[e]);
                await Task.Delay(800);
                OutEnemy(e);
                await Task.Delay(400);
                Door.DoorCloseCast(_doors[e]);
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            
            _numberPointFont    = Content.Load<SpriteFont>("Fonts/Points");
            _lifeLeft           = Content.Load<SpriteFont>("Fonts/Points");
            NewLevelSound       = Content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");
            _arkaLogo           = Content.Load<Texture2D>("Items/ArkaLogo");

            // GameObjets Content:            
            _segments.AddRange(paddle.GetSegments());
            paddle.OnHit = () => paddle.Bounce.Play();
            
            // Screen`s limits           
            _segments.AddRange(screeLimit.GetScreenSegment(A_Limit, B_Limit, C_Limit, D_Limit));
            screeLimit.OnHit = () => _ballWallBounce.Play();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch(gameState)
            {
                case GameState.MENU:
                    screen.WelcomeScreen(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.P))
                    {
                        GoToPreplay(gameTime);
                        paddle.Life = 2;
                        level.Iniciate();
                        gameState = GameState.PREPLAY;
                    }
                break;

                case GameState.PREPLAY:
                    mainText.CountDown(gameTime);
                    if (level.NextLevel) NextLevelText.CountDown(gameTime);
                    break;

                case GameState.PLAY:
                    //Manage paddle.
                    if (paddle.Active)
                        PaddleMovement(gameTime);

                    //Manage ball.
                    ball.StartUpAndReposition();
                    ReleaseBall();
                    ControlsLowerLimitBallPosition(gameTime);

                    //Manage Collision.
                    CollisionAndMotionController(gameTime);
                    Extralife();
                    AchievedLevel(gameTime);
                    GameOver();
                    GameOverScreen.Reset(gameTime);
                    break;

                case GameState.GAMEOVER:
                    screen.ScreenBlackGameOver(gameTime);
                    GameOverScreen.CountDown(gameTime);
                    GameOverScreen.OnMatured += () => gameState = GameState.MENU;
                break;
            }
             
         

            //    // Manage Enemy.
            //    if (ball.Play == true && level.EnemyNumber < 3)
            //        EnemyTimer.CountDown(gameTime);
            //    else
            //        EnemyTimer.Reset(gameTime);

            //    foreach (var enemies in level._enemies)
            //    {
            //        enemies.Animation();
            //        if (enemies.Position.Y >= D_Limit.Y + 10)
            //            level.FallEnemy(enemies);
            //    }

            
            //}

            //// Manage AchiveLvel.
            //if (_play && level.NumberBricks <= 0)
            //{
            //    Thread.Sleep(500);
            //    AchievedLevel();
            //    EnemyTimer.Reset(gameTime);

            //    level.Iniciate();
            //    mainText = new(2.5f);
            //    mainText.OnMatured += () => MainText();
            //}

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
                    MainText();


            }
            if (gameState == GameState.PLAY)
            {
                paddle.PlayerAnimation.UpdateLoop(gameTime);
                paddle.PlayerAnimation.Draw(spriteBatch, paddle.Position);
                ball.Draw(ball._circle.Center);
                ball.Active = true;
                paddle.Can_move = true;
                
            }

            spriteBatch.End();

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

        void GoToPreplay(GameTime gameTime)
        {
            mainText.Reset(gameTime);
            MediaPlayer.Play(NewLevelSound);
            mainText.OnMatured += () =>
            {
                paddle.Start();
                gameState = GameState.PLAY;
            };
            screen.playOn = true;
        }

        private void MainText()
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
                ball._circle.Center -= ball.Direction * collide.depth;// ((_ball.Speed * (float)gametime.ElapsedGameTime.TotalSeconds) - ) ;
                ball.Direction = Vector2.Reflect(ball.Direction, collide.Normal);
                collide.seg.Owner.OnHit();
            }
            else
                IncreaseBallSpeedOverTime(gameTime);
        }

        /// <summary>
        /// Animate the ball and the increasing his speed over time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void IncreaseBallSpeedOverTime(GameTime gameTime)
        {
            if (ball.Active)
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
            if (ball._circle.Center.Y >= D_Limit.Y + 150)
            {
                ball.Death();
                paddle.Death();
                ball.Active = false;
                paddle.Can_move = false;
                //level.Maintext = true;
                //level.NextLevel = false;
                mainText.Reset(gameTime);
                GoToPreplay(gameTime);
                gameState = GameState.PREPLAY;
            }
        }

        public void OutEnemy(int enemyPos)
        {
            var enemyT = Random.Next((int)EnemyType.LAST - 1);
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
                foreach (var segment in ArkaGame._segments)
                    segment.ActiveSegment &= segment.Owner != enemy;
                enemy.Blast.Start();
                enemy.Dead.Play();
                enemy.Active = false;
                EnemyNumber--;
            }
        }

        public void FallEnemy(Enemy enemy)
        {
            if (enemy.Active)
            {
                EnemyNumber--;
                enemy.Active = false;
                foreach (var segment in ArkaGame._segments)
                    segment.ActiveSegment &= segment.Owner != enemy;
            }
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

        public void AchievedLevel(GameTime gameTime)
        {
            if (level.NumberBricks <= 0)
            {
                level._levelNumber++;
                if (level._levelNumber > 4)
                    level._levelNumber = 1;

                level._brickList.Clear();
                _segments.Clear();
                _enemies.Clear();
                level.NumberBricks = 0;
                EnemyNumber = 0;
                //level.Maintext = false;
                //level.NextLevel = true;
                ball.Active = false;
                paddle.Can_move = false;
                NextLevelText.Reset(gameTime);
                EnemyTimer.Reset(gameTime);
                level.Iniciate();
                gameState = GameState.PREPLAY;
            }
        }
        public void GameOver()
        {
            if ( paddle.Life <= 0)
            {
                level._brickList.Clear();
                _segments.Clear();
                _enemies.Clear();
                //level.Maintext = true;
                //level.NextLevel = false;
                ball.Active = false;
                paddle.Can_move = false;
                level._levelNumber = 1;
                EnemyNumber = 0;
                Points = 0;
                level.NumberBricks = 0;
                ExtraLifePoints = 3000;
                gameState = GameState.GAMEOVER;
            }
        }
    }
}
