using Game_Arka;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Arkanoid_02
{
    public class ArkaMain : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private bool Play;
        private bool GameOver_screen;
                
        public Level level;
        public Screen screen;
        
        public ArkaMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            // Flag for selec WellcomeScreen or begin the game.
            Play = false;

            // Flag for show Game Over Screem.
            GameOver_screen = false;

            graphics.PreferredBackBufferWidth = 843;
            graphics.PreferredBackBufferHeight = 900;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            spriteBatch = new SpriteBatch(GraphicsDevice);
            level = new Level(Services, spriteBatch);
            screen = new Screen(Services, spriteBatch);
                       
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.P))
               { Play = true; screen.playOn = true; }

            if (GameOver_screen)
            {
                if (screen.ScreenBlackGameOver(gameTime))
                {
                    GameOver_screen = false;
                    Play = false;
                }
            }

            if (!Play && !GameOver_screen)
                screen.WellcomeScreen(gameTime);

            if (Play)
            {
                if (!level.Update(gameTime))
                {
                    level.GameOver();
                    GameOver_screen = true;
                    Play = false;
                }
            }

            // TODO: Add your update logic here
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            if (Play)
                level.Draw(gameTime); //, spriteBatch);
                       
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

    }
}
