using Game_Arka;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Arkanoid_02
{
    public class ArkaMain : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private bool _play;
        private bool _gameOver_screen;
                
        public Level level;
        public Screen screen;
        
        public ArkaMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            // Flag for selec WellcomeScreen or begin the game.
            _play = false;

            // Flag for show Game Over Screem.
            _gameOver_screen = false;

            _graphics.PreferredBackBufferWidth = 843;
            _graphics.PreferredBackBufferHeight = 900;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            level = new Level(Services, _spriteBatch);
            screen = new Screen(Services, _spriteBatch);
                       
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
               { _play = true; screen.playOn = true; }

            if (_gameOver_screen)
            {
                if (screen.ScreenBlackGameOver(gameTime))
                {_gameOver_screen = false; _play = false;}
            }

            if (!_play && !_gameOver_screen)
                screen.WellcomeScreen(gameTime);

            if (_play)
            {
                if (!level.Update(gameTime))
                {
                    level.GameOver();
                    _gameOver_screen = true;
                    _play = false;
                }
            }

            // TODO: Add your update logic here
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (_play)
                level.Draw(gameTime); //, spriteBatch);
                       
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
