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
using Stateless;

namespace Arkanoid_02
{
    enum GameState { START, MENU, LEVELSTART, PREPLAY, PLAY, PLAYERDIES, LEVELEND, GAMEOVER };
    enum Trigger { ToMenu, ToLevelStart, ToPreplay, ToPlay, ToPlayerDies, ToLevelEnd, ToGameOver };

    public class ArkaGame : Game
    {
        private readonly StateMachine<GameState, Trigger> Fsm = new (GameState.START);
        private readonly GraphicsDeviceManager graphics;
        private readonly Timer Enemy_timer = new(5f);
        private readonly Timer LevelText_timer = new(2.2f);
        private readonly Timer GameOverScreen_timer = new(3f);
        private readonly Timer LifeText_timer = new(2.8f);
        private GameTime gameTime;
        private SpriteBatch spriteBatch;
        private SpriteFont _numberPointFont, _lifeLeft;
        private Texture2D _arkaLogo;

        // Describes the boundaries of the game board.
        private Vector2 B_Limit, A_Limit, C_Limit, D_Limit;

        // Game Objets
        private Level level;
        private Screen screen;
        private Shapes shapes;
        private Brick screeLimit;
        private Ball ball;
        private Paddle paddle;
        private Song WelcomeSong;
        private Song GameOverSong;

        private Song NewLevelSound;
        private SoundEffect _ballWallBounce;
        private readonly Door[] _doors = new Door[2];
        private static readonly List<Enemy> _enemies = new();
        public static readonly List<Segment> _segments = new();
        public static int Points { get; set; }
        private int MaxPoints { get; set; }    
        private int ExtraLifePoints { get; set; }

        private bool ShowLife;
        private bool ShowLevel;

        // This two variable manages time for speed increment of ball in "IncreaseBallSpeedOverTime" method.
        private float timeCount;
        private float ElapsedTime = 10f;
        private int currentLevel = 0;

        public ArkaGame()
        {
            Fsm.Configure(GameState.START)
                .Permit(Trigger.ToMenu, GameState.MENU);

            Fsm.Configure(GameState.MENU)
                .Permit(Trigger.ToLevelStart, GameState.LEVELSTART)
                .OnEntry(() => MediaPlayer.Play(WelcomeSong))
                .OnExit(() =>
                {
                    Points = 0;
                    paddle.Life = 3;
                    currentLevel = 0;
                });

            Fsm.Configure(GameState.LEVELSTART)
                .Permit(Trigger.ToPreplay, GameState.PREPLAY)
                .OnEntry(() => LevelStart(currentLevel));

            Fsm.Configure(GameState.PREPLAY)
                .Permit(Trigger.ToPlay, GameState.PLAY)
                .OnEntry(() =>
                {
                    LevelText_timer.Reset(gameTime);
                    MediaPlayer.Play(NewLevelSound);
                })
                .OnExit(() => ShowLife = false);

            Fsm.Configure(GameState.PLAY)
                .Permit(Trigger.ToLevelEnd, GameState.LEVELEND)
                .Permit(Trigger.ToPlayerDies, GameState.PLAYERDIES)

                .OnEntry(() =>
                {
                    ball.StartUpAndReposition();
                    paddle.Start();
                    _enemies.Clear();
                })
                .OnExit(() =>
                {
                    paddle.Active = false;
                    ball.Active = false;
                });

            Fsm.Configure(GameState.PLAYERDIES)
                .Permit(Trigger.ToPreplay, GameState.PREPLAY)
                .Permit(Trigger.ToGameOver, GameState.GAMEOVER)
                .OnEntry(()=>
                {                  
                    paddle.Death();
                    EnemyScreenErase();
                    LifeText_timer.Reset(gameTime);
                });

            Fsm.Configure(GameState.LEVELEND)
                .Permit(Trigger.ToLevelStart, GameState.LEVELSTART);

            Fsm.Configure(GameState.GAMEOVER)
                .Permit(Trigger.ToMenu, GameState.MENU)
                .OnEntry(() =>
                {
                    GameOverScreen_timer.Reset(gameTime);
                    MediaPlayer.Play(GameOverSong);
                });

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            graphics.PreferredBackBufferWidth = 843;
            graphics.PreferredBackBufferHeight = 900;
            A_Limit = new Vector2(25, 100);
            B_Limit = new Vector2(800, 100);
            C_Limit = new Vector2(800, 875);
            D_Limit = new Vector2(25, 875);

            MaxPoints = 50000;
            ExtraLifePoints = 12000;
        }

        protected override void Initialize()
        {
            spriteBatch     = new SpriteBatch   (GraphicsDevice);
            shapes          = new Shapes        (this);
            level           = new Level         (Services, spriteBatch,Content);
            screen          = new Screen        (Services, spriteBatch);
            screeLimit      = new Brick         (Hard.Blue, Content, spriteBatch, "Items/BlueBlock", Vector2.Zero);
            paddle          = new Paddle        (Content, spriteBatch, "Items/Player", new Vector2(365, 810));
            ball            = new Ball          (Content, spriteBatch, "Items/ball", new Vector2(380, 840));
            _doors[0]       = new Door          (Content, spriteBatch, "Items/door", new Vector2(182, 75));
            _doors[1]       = new Door          (Content, spriteBatch, "Items/door", new Vector2(573, 75));

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
                ShowLevel = false;                
                _segments.AddRange(paddle.GetSegments());
                paddle.OnHit = () => paddle.Bounce.Play();
                _segments.AddRange(screeLimit.GetScreenSegment(A_Limit, B_Limit, C_Limit, D_Limit));
                screeLimit.OnHit = () => _ballWallBounce.Play();
                Fsm.Fire(Trigger.ToPlay);
            };

            LifeText_timer.OnMatured += () =>
            {
                Fsm.Fire(Trigger.ToPlay);
                ShowLife = false;
            };

            GameOverScreen_timer.OnMatured += () => Fsm.Fire(Trigger.ToMenu);
            base.Initialize();
        }

        protected override void LoadContent()
        {            
            _arkaLogo           = Content.Load<Texture2D>("Items/ArkaLogo");
            _numberPointFont    = Content.Load<SpriteFont>("Fonts/Points");
            _lifeLeft           = Content.Load<SpriteFont>("Fonts/Points");
            NewLevelSound       = Content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");
            _ballWallBounce     = Content.Load<SoundEffect>("Sounds/WallBounce");
            WelcomeSong         = Content.Load<Song>("Sounds/Start_Demo");
            GameOverSong        = Content.Load<Song>("Sounds/05_-_Arkanoid_-_NES_-_Game_Over");
            Fsm.Fire(Trigger.ToMenu);
        }

        protected override void Update(GameTime gameTime)
        {
            //We store a copy for the internal use of the FSM as the timers do require it
            this.gameTime = gameTime;
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (Fsm.State)
            {
                case GameState.MENU:
                    screen.WelcomeScreen(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.P))
                        Fsm.Fire(Trigger.ToLevelStart);
                break;

                case GameState.PREPLAY:
                    if (ShowLevel)
                        LevelText_timer.CountDown(gameTime);
                    if (ShowLife)
                        LifeText_timer.CountDown(gameTime);
                    Enemy_timer.Reset(gameTime);
                break;

                case GameState.PLAY:
                    WeArePlaying();
                break;

                case GameState.PLAYERDIES:
                    if (paddle.Life > 0)
                    {
                        Fsm.Fire(Trigger.ToPreplay);
                        ShowLife = true;
                    }
                    else
                        Fsm.Fire(Trigger.ToGameOver);           
                break;

                case GameState.LEVELEND:
                    currentLevel = ++currentLevel % 4;
                    Fsm.Fire(Trigger.ToLevelStart);
                break;

                case GameState.GAMEOVER:
                    screen.ScreenBlackGameOver(gameTime);
                    GameOverScreen_timer.CountDown(gameTime);
                break;
            }

            base.Update(gameTime);       
        }
        
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            
            if (Fsm.State == GameState.PLAY || Fsm.State == GameState.PREPLAY)
            {
                level.Draw(gameTime);
                // Score
                string points = Points.ToString();
                spriteBatch.DrawString(_numberPointFont, points, new Vector2(125, 20), Color.WhiteSmoke);

                // High Score
                string maxpoints = MaxPoints.ToString();
                spriteBatch.DrawString(_numberPointFont, maxpoints, new Vector2(400, 20), Color.WhiteSmoke);

                // Round
                string l_Number = (currentLevel+1).ToString();
                spriteBatch.DrawString(_numberPointFont, l_Number, new Vector2(635, 34), Color.WhiteSmoke);

                // Lives
                string l_life = paddle.Life.ToString();
                spriteBatch.DrawString(_numberPointFont, l_life, new Vector2(775, 10), Color.WhiteSmoke);

                // Doors
                foreach (var door in _doors)
                {
                    if (door.Active)
                        door.Draw(gameTime);

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
                if (ShowLevel)
                    DrawLevelNumberText();
                if (ShowLife)
                    DrawLifeLeftText();
            }

            if (Fsm.State == GameState.PLAY)
            {
                paddle.PlayerAnimation.UpdateLoop(gameTime);
                paddle.PlayerAnimation.Draw(spriteBatch, paddle.Position);
                ball.Draw(ball._circle.Center);

                // Enemies
                foreach (var enemy in _enemies)
                {
                    Debug.Assert(enemy != null);
                    if (enemy.Active)
                    {
                        enemy.EnemyAnimation.UpdateLoop(gameTime);
                        enemy.EnemyAnimation.Draw(spriteBatch, enemy.Position);
                    }
                    else if (enemy.Blast.AnimaActive)
                    {
                        enemy.Blast.Update(gameTime);
                        enemy.Blast.Draw(spriteBatch, enemy.Position);
                    }
                    //else the enemy was deactivated as it exited through the bottom
                }
            }

            spriteBatch.End();

            //----------------------------------------- This code displays the segments on the screen. ----------------------------
            //shapes.Begin();
            //if (Fsm.State == GameState.PLAY)
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
  
        /// <summary>
        /// Calls to the diferents methodes that makes we can play.
        /// </summary>
        private void WeArePlaying()
        {
            //Manage paddle.
            Debug.Assert(paddle.Active);
            PaddleMovement(gameTime);

            //Manage ball.
            ReleaseBall();
            ControlsLowerLimitBallPosition(gameTime);

            //Manage Collision.
            CollisionAndMotionController(gameTime);
            Extralife();
            if (!level._brickList.Exists((brick) => brick.Active))
            {
                Thread.Sleep(1000);
                Fsm.Fire(Trigger.ToLevelEnd);
            }
            
            //Manage Enemies.
            UpdateEnemies(gameTime);
        }

        /// <summary>
        /// Draw the number of lives left at the begenning of the level.
        /// </summary>
        private void DrawLifeLeftText()
        {
            spriteBatch.Draw(_arkaLogo, new Vector2(320, 620), Color.White);
            spriteBatch.DrawString(_lifeLeft, paddle.Life + "  LIVES  LEFT", new Vector2(290, 700), Color.White);
            spriteBatch.DrawString(_lifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        /// <summary>
        /// Draw the number of level that are we playing.
        /// </summary>
        private void DrawLevelNumberText()
        {
            spriteBatch.Draw(_arkaLogo, new Vector2(320, 620), Color.White);
            spriteBatch.DrawString(_lifeLeft, "ROUND   " + (currentLevel + 1), new Vector2(345, 700), Color.White);
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
            if (ball.Active && pushkey.IsKeyDown(Keys.Space))
                ball.Attach = false;
        }

        /// <summary>
        /// Code for control the segment collision with a Circle/Ball.
        /// </summary>
        /// <param name="gameTime"></param>
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
                Fsm.Fire(Trigger.ToPlayerDies);            
        }

        /// <summary>
        ///  Manage Enemy.
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateEnemies(GameTime gameTime)
        {   
            if (/*ball.Active == true && ball.Attach == false && */ _enemies.Count < 5)
                Enemy_timer.CountDown(gameTime);
            else
                Enemy_timer.Reset(gameTime);

            _enemies.RemoveAll(e => !e.Active && !e.Blast.AnimaActive);

            foreach (var enemy in _enemies)
            {
                Debug.Assert(enemy.Active || enemy.Blast.AnimaActive);
                enemy.Animation();
                if (enemy.Position.Y >= D_Limit.Y + 20)
                    DeactivateEnemy(enemy);
            }
        }

        /// <summary>
        /// Create new enemies.
        /// </summary>
        /// <param name="enemyPos"></param>
        public void OutEnemy(int enemyPos)
        {
            var enemyT = new Random().Next((int)EnemyType.LAST - 1);
            Enemy _enemy = new((EnemyType)enemyT, Content, spriteBatch, enemyPos);
            _enemies.Add(_enemy);
            _enemy.OnHit = () => ExplodeEnemy(_enemy);
            _enemy.Animation = _enemy.EnemyCircleMovement;            
            _segments.AddRange(_enemy.GetSegments());
        }

        /// <summary>
        /// When have been hit it. Clear and disable the enemies and their segments from a list. And subtracts them from the EnemyNumber value.
        /// </summary>
        /// <param name="enemy"></param>
        public void ExplodeEnemy(Enemy enemy)
        {
            Debug.Assert(enemy.Active);
            DeactivateEnemy(enemy);
            enemy.Blast.Start();
            enemy.Dead.Play();
        }

        /// <summary>
        /// When the enemy crosses the bottom of the screen. Clear and disable the enemies and their segments from a list. And subtracts them from the EnemyNumber value.
        /// </summary>
        /// <param name="enemy"></param>
        public void DeactivateEnemy(Enemy enemy)
        {
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
        private void LevelStart(int _level)
        {
            ShowLevel = true;
            _segments.Clear();
            level.Iniciate(_level);
            Fsm.Fire(Trigger.ToPreplay);
        }
    }
}
