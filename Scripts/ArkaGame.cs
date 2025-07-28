using Arkanoid;
using Command_Interpreter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.X3DAudio;
using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Arkanoid_02
{
    enum GameState { START, MENU, LEVELSTART, PREPLAY, PLAY, PLAYERDIES, LEVELEND, GAMEOVER };
    enum Trigger { ToMenu, ToLevelStart, ToPreplay, ToPlay, ToPlayerDies, ToLevelEnd, ToGameOver };

    /// <summary>
    /// Represents the main game class for the Arkanoid-style game.
    /// </summary>
    /// <remarks>This class manages the overall game flow, including state transitions, game object
    /// initialization, rendering, and updates. It uses a finite state machine (FSM) to control the game's states, such
    /// as menu, gameplay, level transitions, and game over. The game logic includes paddle movement, ball physics,
    /// enemy management, and scoring.</remarks>
    public class ArkaGame : Game
    {
        private readonly StateMachine<GameState, Trigger> fsm = new(GameState.START);
        private readonly GraphicsDeviceManager graphics;
        private readonly Timer enemyTimer = new(5f);
        private readonly Timer levelTextTimer = new(2.2f);
        private readonly Timer gameOverScreenTimer = new(3f);
        private readonly Timer lifeTextTimer = new(2.8f);
        private GameTime gameTime;
        private SpriteBatch spriteBatch;
        private SpriteFont numberPointFont, lifeLeft;
        private Texture2D arkaLogo;

        // Instanciate Shapes Class.
        private Shapes shapes;

		// Describes the boundaries of the game board.
		private Vector2 A_Limit, B_Limit, C_Limit, D_Limit;

        // Game Objects
        private Level level;
        private Screen screen;
        private Brick screeLimit;
        private Ball ball;
        private Paddle paddle;
        private Song welcomeSong;
        private Song gameOverSong;

        private Song newLevelSound;
        private SoundEffect ballWallBounce;
        private readonly Door[] doorArray = new Door[2];
        private static readonly List<Enemy> enemyList = new();
        public static readonly List<Segment> SegmentsList = new();
        public static int Points { get; set; }
        private int MaxPoints { get; set; }
        private int ExtraLifePoints { get; set; }

        private bool showLife;
        private bool showLevel;
        private bool infiniteLife;

        public static bool drawshapes;
        public static bool drawbricks;

        // These two variable manage time for speed increment of the ball in "IncreaseBallSpeedOverTime" method.
        private float timeCount;
        private float elapsedTime = 10f;
        private int currentLevel;

        // Comand Interpreter
        private string textconsole;
        private readonly Commands com = new Commands();

		HttpListener listener = new HttpListener();

		public ArkaGame()
        {
            fsm.Configure(GameState.START)
                .Permit(Trigger.ToMenu, GameState.MENU);

            fsm.Configure(GameState.MENU)
                .Permit(Trigger.ToLevelStart, GameState.LEVELSTART)
                .OnEntry(() => MediaPlayer.Play(welcomeSong))
                .OnExit(() =>
                {
                    Points = 0;
                    paddle.Life = 3;
                    currentLevel = 0;
                });

            fsm.Configure(GameState.LEVELSTART)
                .Permit(Trigger.ToPreplay, GameState.PREPLAY)
                .OnEntry(() => LevelStart(currentLevel));

            fsm.Configure(GameState.PREPLAY)
                .Permit(Trigger.ToPlay, GameState.PLAY)
                .OnEntry(() =>
                {
                    levelTextTimer.Reset(gameTime);
                    MediaPlayer.Play(newLevelSound);
                })
                .OnExit(() => showLife = false);

            fsm.Configure(GameState.PLAY)
                .Permit(Trigger.ToLevelEnd, GameState.LEVELEND)
                .Permit(Trigger.ToPlayerDies, GameState.PLAYERDIES)
                // Implementation for Command_interpreter
                .Permit(Trigger.ToLevelStart, GameState.LEVELSTART)

                .OnEntry(() =>
                {
                    ball.StartUpAndReposition();
                    paddle.Start();
                    enemyList.Clear();
                })
                .OnExit(() =>
                {
                    paddle.IsActive = false;
                    ball.IsActive = false;
                });

            fsm.Configure(GameState.PLAYERDIES)
                .Permit(Trigger.ToPreplay, GameState.PREPLAY)
                .Permit(Trigger.ToGameOver, GameState.GAMEOVER)
                .OnEntry(() =>
                {
                    if (!infiniteLife)
                        paddle.Death();
                    else paddle.InfiniteLife();
                    EnemyScreenErase();
                    lifeTextTimer.Reset(gameTime);
                });

            fsm.Configure(GameState.LEVELEND)
                .Permit(Trigger.ToLevelStart, GameState.LEVELSTART);

            fsm.Configure(GameState.GAMEOVER)
                .Permit(Trigger.ToMenu, GameState.MENU)
                .OnEntry(() =>
                {
                    gameOverScreenTimer.Reset(gameTime);
                    MediaPlayer.Play(gameOverSong);
                });

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            infiniteLife = false;


            graphics.PreferredBackBufferWidth = 843;
            graphics.PreferredBackBufferHeight = 900;
            A_Limit = new Vector2(25, 100);
            B_Limit = new Vector2(800, 100);
            C_Limit = new Vector2(800, 875);
            D_Limit = new Vector2(25, 875);

            MaxPoints = 5000;
            ExtraLifePoints = 12000;

            // Code that provides the string to represent a draw that describes the FiniteStateMachine interdependencies.
            //string graph = UmlDotGraph.Format(fsm.GetInfo());
            //Console.WriteLine(graph);


            // Adding functions to the CommandInterpreter.
            try
            {
                com.AddFunc("il", InfiniteLives, "Activate infinite lives");
                com.AddFunc("lv", SwichLevel, "Swich the current level");
				com.AddFunc("lv", GetLevel, "Swich the current level");
                com.AddFunc("Seg", DrawShape, "Swich on-off the visivility of the segments");
                com.AddFunc("Brick", DrawBrick, "Swich on-off the visivility of the bricks");
                com.AddFunc("Cap", CaptureWindow, "Capture a screen of the game");

			}
			catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

        }

        /// <summary>
        /// Initializes the game components and sets up the initial game state.
        /// </summary>
        /// <remarks>This method is responsible for creating and configuring the primary game objects, 
        /// including the level, screen, paddle, ball, and doors. It also sets up event handlers  for timers and game
        /// state transitions. This method is called once during the game's  initialization phase.</remarks>
        protected override void Initialize()
        {

 //--------- Configuration the console. ------------------

			// Configure the port to be used.
			listener.Prefixes.Add("http://localhost:7000/");
            // Initialize the port.
		    listener.Start();// I initialize the listening
			Console.WriteLine("Websocket server started at ws://localhost:7000/");

//--------- End the configuration. ------------------

			spriteBatch     = new SpriteBatch(GraphicsDevice);
            level           = new Level(Services, spriteBatch, Content);
            screen          = new Screen(Services, spriteBatch);
            screeLimit      = new Brick(Hard.Blue, Content, spriteBatch, "Items/BlueBlock", Vector2.Zero);
            paddle          = new Paddle(Content, spriteBatch, "Items/Player", new Vector2(365, 810));
            ball            = new Ball(Content, spriteBatch, "Items/ball", new Vector2(380, 840));
            doorArray[0]    = new Door(Content, spriteBatch, "Items/door", new Vector2(182, 75));
            doorArray[1]    = new Door(Content, spriteBatch, "Items/door", new Vector2(573, 75));

			// Initialize the shape Class, for render the segments
			shapes = new Shapes(this);
            drawshapes = false;
            drawbricks = true;

			enemyTimer.OnMatured += async () =>
            {
                var e = new Random().Next(0, 2);
                Door.DoorOpenCast(doorArray[e]);
                await Task.Delay(800);
                OutEnemy(e);
                await Task.Delay(400);
                Door.DoorCloseCast(doorArray[e]);
            };

            levelTextTimer.OnMatured += () =>
            {
                showLevel = false;
                SegmentsList.AddRange(paddle.GetSegments());
                paddle.OnHit = () => paddle.Bounce.Play();
                SegmentsList.AddRange(screeLimit.GetScreenSegment(A_Limit, B_Limit, C_Limit, D_Limit));
                screeLimit.OnHit = () => ballWallBounce.Play();
                fsm.Fire(Trigger.ToPlay);
            };

            lifeTextTimer.OnMatured += () =>
            {
                fsm.Fire(Trigger.ToPlay);
                showLife = false;
            };

            gameOverScreenTimer.OnMatured += () => fsm.Fire(Trigger.ToMenu);
            base.Initialize();
        }

        /// <summary>
        /// Loads the content required for the game, including textures, fonts, and audio assets.
        /// </summary>
        /// <remarks>This method is called during the initialization phase to load all necessary game
        /// resources. It prepares textures, fonts, sound effects, and songs that are used throughout the game.
        /// Additionally, it transitions the game state to the main menu.</remarks>
        protected override void LoadContent()
        {
            arkaLogo        = Content.Load<Texture2D>("Items/ArkaLogo");
            numberPointFont = Content.Load<SpriteFont>("Fonts/Points");
            lifeLeft        = Content.Load<SpriteFont>("Fonts/Points");
            ballWallBounce  = Content.Load<SoundEffect>("Sounds/WallBounce");
            newLevelSound   = Content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");
            welcomeSong     = Content.Load<Song>("Sounds/Start_Demo");
            gameOverSong    = Content.Load<Song>("Sounds/05_-_Arkanoid_-_NES_-_Game_Over");

            fsm.Fire(Trigger.ToMenu);
        }

        /// <summary>
        /// Updates the game state and processes game logic based on the current state of the finite state machine (FSM).
        /// </summary>
        /// <remarks>This method is called once per frame and handles input, state transitions, and game
        /// logic for the active game state. It processes keyboard input (e.g., exiting the game or transitioning between
        /// states) and delegates specific behavior  to the appropriate state logic, such as displaying screens, managing
        /// timers, or updating gameplay elements.</remarks>
        /// <param name="gameTime">The current game time, which provides timing values for the update cycle.</param>
        protected override void Update(GameTime gameTime)
        {
            // Comunication with the console.
			Listener(listener);

            //We store a copy for the internal use of the FSM as the timers do require it
            this.gameTime = gameTime;
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (fsm.State)
            {
                case GameState.MENU:
                    screen.WelcomeScreen(gameTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.P))
                        fsm.Fire(Trigger.ToLevelStart);
                    break;

                case GameState.PREPLAY:
                    if (showLevel)
                        levelTextTimer.CountDown(gameTime);
                    if (showLife)
                        lifeTextTimer.CountDown(gameTime);
                    enemyTimer.Reset(gameTime);
                    break;

                case GameState.PLAY:
                    WeArePlaying();
                    break;

                case GameState.PLAYERDIES:
                    if (paddle.Life > 0)
                    {
                        fsm.Fire(Trigger.ToPreplay);
                        showLife = true;
                    }
                    else
                        fsm.Fire(Trigger.ToGameOver);
                    break;

                case GameState.LEVELEND:
                    currentLevel = ++currentLevel % 4;
                    fsm.Fire(Trigger.ToLevelStart);
                    break;

                case GameState.GAMEOVER:
                    screen.ScreenBlackGameOver(gameTime);
                    gameOverScreenTimer.CountDown(gameTime);
                    break;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Renders the game elements to the screen based on the current game state.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values for the current frame.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            if (fsm.State == GameState.PLAY || fsm.State == GameState.PREPLAY)
            {
                level.Draw(gameTime);
                // Score
                string points = Points.ToString();
                spriteBatch.DrawString(numberPointFont, points, new Vector2(125, 20), Color.WhiteSmoke);

                // High Score
                string maxpoints = MaxPoints.ToString();
                spriteBatch.DrawString(numberPointFont, maxpoints, new Vector2(400, 20), Color.WhiteSmoke);

                // Round
                string l_Number = (currentLevel + 1).ToString();
                spriteBatch.DrawString(numberPointFont, l_Number, new Vector2(635, 34), Color.WhiteSmoke);

                // Lives
                string l_life = paddle.Life.ToString();
                spriteBatch.DrawString(numberPointFont, l_life, new Vector2(775, 10), Color.WhiteSmoke);

                // Doors
                foreach (var door in doorArray)
                {
                    if (door.IsActive)
                        door.Draw(gameTime);

                    if (door.OpenDoor.IsAnimaActive)
                    {
                        door.OpenDoor.Draw(spriteBatch, door.DoorPosition);
                        door.OpenDoor.Update(gameTime);
                    }

                    if (door.CloseDoor.IsAnimaActive)
                    {
                        door.CloseDoor.Draw(spriteBatch, door.DoorPosition);
                        door.CloseDoor.Update(gameTime);
                    }
                }
                if (showLevel)
                    DrawLevelNumberText();
                if (showLife)
                    DrawLifeLeftText();
            }

            if (fsm.State == GameState.PLAY)
            {
                paddle.PlayerAnimation.UpdateLoop(gameTime);
                paddle.PlayerAnimation.Draw(spriteBatch, paddle.Position);
                ball.Draw(ball.Circle.Center);

                // Enemies
                foreach (var enemy in enemyList)
                {
                    Debug.Assert(enemy != null);
                    if (enemy.IsActive)
                    {
                        enemy.EnemyAnimation.UpdateLoop(gameTime);
                        enemy.EnemyAnimation.Draw(spriteBatch, enemy.Position);
                    }
                    else if (enemy.Blast.IsAnimaActive)
                    {
                        enemy.Blast.Update(gameTime);
                        enemy.Blast.Draw(spriteBatch, enemy.Position);
                    }
                }
            }

			spriteBatch.End();

            // This code displays the segments on the screen.
            if (drawshapes)
            {
                shapes.Begin();
                if (fsm.State == GameState.PLAY)
                {
                    foreach (var seg in SegmentsList)
                    {
                        if (seg.IsActiveSegment)
                            shapes.Drawline(new(seg.Ini.X, 900 - seg.Ini.Y), new(seg.End.X, 900 - seg.End.Y), 1, Color.White);
                    }
                }
                shapes.End();

                base.Draw(gameTime);
            }
        }

		/// <summary>
		/// Calls to the different methods that makes we can play.
		/// </summary>
		private void WeArePlaying()
        {
            //Manage paddle.
            Debug.Assert(paddle.IsActive);
            PaddleMovement(gameTime);

            //Manage ball.
            ReleaseBall();
            ControlsLowerLimitBallPosition(gameTime);

            //Manage Collision.
            CollisionAndMotionController(gameTime);
            Extralife();
            if (!level.brickList.Exists((brick) => brick.IsActive))
            {
                Thread.Sleep(1000);
                fsm.Fire(Trigger.ToLevelEnd);
            }

            //Manage Enemies.
            UpdateEnemies(gameTime);
        }

        /// <summary>
        /// Draw the number of lives left at the beginning of the level.
        /// </summary>
        private void DrawLifeLeftText()
        {
            spriteBatch.Draw(arkaLogo, new Vector2(320, 620), Color.White);
            spriteBatch.DrawString(lifeLeft, paddle.Life + "  LIVES  LEFT", new Vector2(290, 700), Color.White);
            spriteBatch.DrawString(lifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        /// <summary>
        /// Draw the number of the level that we are playing.
        /// </summary>
        private void DrawLevelNumberText()
        {
            spriteBatch.Draw(arkaLogo, new Vector2(320, 620), Color.White);
            spriteBatch.DrawString(lifeLeft, "ROUND   " + (currentLevel + 1), new Vector2(345, 700), Color.White);
            spriteBatch.DrawString(lifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        /// <summary>
        /// This method manages the paddle's movement.
        /// </summary>
        /// <param name="gameTime"> Provides a snapshot of timing values for the current frame. </param>
        public void PaddleMovement(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();
            int movement = 0;
            if (paddle.canMove && keyState.IsKeyDown(Keys.Left) && paddle.Position.X > 35f)
                movement = -1;
            if (paddle.canMove && keyState.IsKeyDown(Keys.Right) && paddle.Position.X + paddle.Size.X < 810f)
                movement = 1;
            paddle.Position.X += movement * paddle.PaddleDirection.X * paddle.PaddleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// To beging the ball movement.
        /// </summary>
        public void ReleaseBall()
        {
            if (ball.Attach)
            {
                ball.Circle.Center.Y = paddle.Position.Y;
                ball.Circle.Center.X = paddle.Position.X + 70;
            }
            var pushkey = Keyboard.GetState();
            if (ball.IsActive && pushkey.IsKeyDown(Keys.Space))
                ball.Attach = false;
        }

        /// <summary>
        /// Code for control the segment collision with a Circle/Ball.
        /// </summary>
        /// <param name="gameTime"> Provides a snapshot of timing values for the current frame. </param>
        private void CollisionAndMotionController(GameTime gameTime)
        {
            ball.Circle.Center += ball.Speed * ball.Direction * (float)gameTime.ElapsedGameTime.TotalSeconds;

            var collide = ArkaMath.CollideWithWorld(SegmentsList, ball.Direction, ball.Circle.Center, ball.Circle.Radius);
            if (collide.Depth > 0)
            {
                ball.Circle.Center -= ball.Direction * collide.Depth;
                ball.Direction = Vector2.Reflect(ball.Direction, collide.Normal);
                collide.Seg.Owner.OnHit();
            }
            else
                IncreaseBallSpeedOverTime(gameTime);
        }

        /// <summary>
        /// Animate the ball and the increasing his speed over time.
        /// </summary>
        /// <param name="gameTime"> Provides a snapshot of timing values for the current frame. </param>
        public void IncreaseBallSpeedOverTime(GameTime gameTime)
        {
            if (ball.IsActive && !ball.Attach)
            {
                timeCount += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeCount > elapsedTime && ball.Speed < ball.Maxspeed)
                {
                    ball.Speed += 30f;
                    elapsedTime += 5f;
                }
            }
        }

        /// <summary>
        /// Controls if the ball drops, and kills it.
        /// </summary>
        private void ControlsLowerLimitBallPosition(GameTime gameTime)
        {
            if (ball.Circle.Center.Y >= D_Limit.Y + 100)
                fsm.Fire(Trigger.ToPlayerDies);
        }

        /// <summary>
        ///  Manage Enemy.
        /// </summary>
        /// <param name="gameTime"> Provides a snapshot of timing values for the current frame. </param>
        private void UpdateEnemies(GameTime gameTime)
        {
            if (ball.IsActive == true && ball.Attach == false && enemyList.Count < 5)
                enemyTimer.CountDown(gameTime);
            else
                enemyTimer.Reset(gameTime);  

            enemyList.RemoveAll(e => !e.IsActive && !e.Blast.IsAnimaActive);

            foreach (var enemy in enemyList)
            {
                Debug.Assert(enemy.IsActive || enemy.Blast.IsAnimaActive);
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
            enemyList.Add(_enemy);
            _enemy.OnHit = () => ExplodeEnemy(_enemy);
            _enemy.Animation = _enemy.EnemyCircleMovement;
            SegmentsList.AddRange(_enemy.GetSegments());
        }

        /// <summary>
        /// When have been hit it. Clear and disable the enemies and their segments from a list. And subtracts them from the EnemyNumber value.
        /// </summary>
        /// <param name="enemy"></param>
        public static void ExplodeEnemy(Enemy enemy)
        {
            Debug.Assert(enemy.IsActive);
            DeactivateEnemy(enemy);
            enemy.Blast.Start();
            enemy.Dead.Play();
        }

        /// <summary>
        /// Deactivates the specified enemy and updates the active state of related segments.
        /// </summary>
        /// <remarks>This method sets the <see cref="Enemy.IsActive"/> property of the specified enemy to <see
        /// langword="false"/>. Additionally, it updates the active state of segments in the <c>SegmentsList</c> to ensure no
        /// segment remains associated with the deactivated enemy.</remarks>
        /// <param name="enemy">The enemy to deactivate. Cannot be null.</param>
        public static void DeactivateEnemy(Enemy enemy)
        {
            foreach (var segment in SegmentsList)
                segment.IsActiveSegment &= segment.Owner != enemy;
            enemy.IsActive = false;
        }

        /// <summary>
        /// Removes all screen segments associated with enemies from the segment list.
        /// </summary>
        private static void EnemyScreenErase()
        {
            foreach (var enemy in enemyList)
                SegmentsList.RemoveAll(segment => segment.Owner == enemy);
        }

        /// <summary>
        /// Create a new life wen you reach the points.
        /// </summary>
        public void Extralife()
        {
            if (Points >= ExtraLifePoints)
            {
                ExtraLifePoints += Points;
                paddle.Life += 1;
                paddle.ExtraLife.Play();
            }
        }

        /// <summary>
        /// Initializes the specified game level and transitions the game state to the preplay phase.
        /// </summary>
        /// <param name="_level">The level number to initialize. Must be a valid level identifier.</param>
        private void LevelStart(int _level)
        {
            showLevel = true;
            SegmentsList.Clear();
            level.Iniciate(_level);
            fsm.Fire(Trigger.ToPreplay);
        }

// ----------- Methods for use with the Command Interpreter Dll. ---------------------
        private string GetLevel()
        {
            return currentLevel.ToString();
		}
		private void SwichLevel(int _level)
		{
			currentLevel = _level;
			fsm.Fire(Trigger.ToLevelStart);
		}

		public string InfiniteLives(bool value)
        {
            infiniteLife = value;
            return $"player {(value? "has":"doesn't have")} infinite lives";
        }

        public void DrawShape(string value)
        {
            if (value == "on")
                drawshapes = true;
            if (value == "off")
                drawshapes = false;
        }

		public void DrawBrick(string value)
		{
			if (value == "on")
				drawbricks = true;
			if (value == "off")
				drawbricks = false;
		}

        public Texture2D CaptureWindow()
        {
            int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int h = GraphicsDevice.PresentationParameters.BackBufferHeight;
		    Texture2D deltaCapture = new (GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color);
			Color[] pixelData = new Color[h * w];

            GraphicsDevice.GetBackBufferData(pixelData);
            deltaCapture.SetData(pixelData);
            return deltaCapture;
        }


// --------------- Methods to prepare the communication with the Web-Console. --------------------
		public async void Listener(HttpListener listener)
        {
			HttpListenerContext context = await listener.GetContextAsync(); // I store in "context" what arrives through the listener.
			if (context.Request.IsWebSocketRequest)// If there is a request.
			{
				HttpListenerWebSocketContext wsConstext = await context.AcceptWebSocketAsync(null); // We access the Listener information
																									// We put it in a separate Task. This way, we don't lose responses (if we put an await, we might lose it).
				Task.Run(() => ReadWebSocket(wsConstext.WebSocket, com));
			}
			else // We only expect websocket requests. We return an error for non-websocket requests.
			{
				context.Response.StatusCode = 400;
				context.Response.Close();
			}
		}
		static async Task ReadWebSocket(WebSocket socket, Commands com)
		{
			byte[] buffer = new byte[10234]; // Creating the byte array where we will save the request.
			while (socket.State == WebSocketState.Open) // As long as the websocket is open.
			{
				// We save in 'result' what we receive from the WebSocket, as an Array segment of what is in the 'buffer'
				WebSocketReceiveResult webCommand = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				string command = Encoding.UTF8.GetString(buffer, 0, webCommand.Count);// We encode a String in UTF8

				CommandReply result = com.Command(command);
                if (result.Return is Texture2D buffa)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        buffa.SaveAsPng(ms, buffa.Width, buffa.Height);
                        result.Return = Convert.ToBase64String(ms.ToArray());
                    }
                }
				Console.WriteLine($"Message received: {command}");

				string xmlOutput = WriterOfNewXmlString(result);
				string response = XmlToText(xmlOutput);// Response string
				byte[] responseBytes = Encoding.UTF8.GetBytes(response); // Conversion of String to Array of Bytes
				await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None); // Enviamos la respuesta.
			}
		}
		static string XmlToText(string xml)
		{
			XslCompiledTransform xslTranslater = new();
            try
            {
                var xslFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "XSL_HTMLTFile.xslt");
                xslTranslater.Load(xslFile);
                using (StringWriter texOutput = new())
			    {
				    using XmlReader xmlMemory = XmlReader.Create(new StringReader(xml));
				    xslTranslater.Transform(xmlMemory, null, texOutput);
				    xml = texOutput.ToString();
			    }
			    xml = xml.Replace("\\x1b", "\x1b"); // Replace the \x1b with the escape character for color as .NET can not generate escape characters from Xslt.
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"The { ex.FileName} not found at said location");
            }
			return xml;
		}

		static string WriterOfNewXmlString<T>(T newxmlmessage)
		{
			// Declare the needed variables
			string consoleOutput;

			StringWriter logEntryWriter = new();
			XmlSerializer _serializerFor_LogEntry = new(typeof(T));

			_serializerFor_LogEntry.Serialize(logEntryWriter, newxmlmessage);

			consoleOutput = logEntryWriter.ToString();
			return consoleOutput;
		}
	}

    
}
