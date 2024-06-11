﻿using Arkanoid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Arkanoid_02
{
    public class ArkaMain : Game
    {
        public readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameTime _gametime;

        // Flag for selec WellcomeScreen or begin the game.
        private bool _play;
        // Flag for show Game Over Screem.
        private bool _gameOver_screen;
                
        public Level level;
        public Screen screen;
        public Shapes shapes;

        public int ActualTime;
        
        public ArkaMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _gametime = new GameTime();
            IsMouseVisible = false;

            _play = false;
            _gameOver_screen = false;

            _graphics.PreferredBackBufferWidth = 843;
            _graphics.PreferredBackBufferHeight = 900;
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            level = new Level(Services, _gametime, _spriteBatch);
            screen = new Screen(Services, _spriteBatch);
            shapes = new Shapes(this);
            base.Initialize();
        }

        protected override void LoadContent()
        {
           
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (!_play && !_gameOver_screen)
                screen.WellcomeScreen(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            { 
                _play = true;
                screen.playOn = true;
            }

            if (_play)
            {
                if (!level.Update(gameTime))
                {
                    level.GameOver();
                    _gameOver_screen = true;
                    _play = false;
                }
            }

            if (_gameOver_screen)
            {
                if (screen.ScreenBlackGameOver(gameTime))
                {_gameOver_screen = false; _play = false;}
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            if (_play)
                level.Draw(gameTime);
            _spriteBatch.End();

            //shapes.Begin();
            //foreach (var seg in Level._segments)
            //{
            //    if (seg.ActiveSegment)
            //        shapes.Drawline(new(seg.Ini.X, 900 - seg.Ini.Y), new(seg.End.X, 900 - seg.End.Y), 1, Color.White);
            //}
            //shapes.End();

            base.Draw(gameTime);
        }       
    }
}
