using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using Microsoft.Xna.Framework.Media;

namespace Arkanoid_02
{
    public class Screen
    {
        private Song WelcomeSong;
        private Song GameOverSong;

        private Texture2D Welcome;
        private Texture2D BlackGameOver;

        private Vector2 WelcomePosition;
        private Vector2 BlackGameOver_Position;
        private Vector2 PressP_Position;

        private readonly ContentManager content;
        private readonly SpriteBatch SpriteBatch;
        private SpriteFont PressP;

        private double timerForDraw_P;

        public bool playOn; // Flag for play de Melody of The Game.
        public bool PlayOnBlack; // Flag for play de Melody of The Game Over Screen.
        public bool GO_On; // Flag to know when show Game Over Screen.

        public Screen(IServiceProvider serviceProvider, SpriteBatch spriteBatch)
        {
            content                = new ContentManager(serviceProvider, "Content");
            SpriteBatch            = spriteBatch;
            BlackGameOver_Position = new Vector2(221,450);  // Tex "Game Over".
            WelcomePosition        = Vector2.Zero;
            PressP_Position        = new Vector2(230,530);  // Tex "Press P for play".
            
            playOn = true;
        }

        public void WelcomeScreen(GameTime gameTime)
        {
            timerForDraw_P += gameTime.ElapsedGameTime.TotalSeconds;

            WelcomeSong = content.Load<Song>("Sounds/Start_Demo");
            Welcome     = content.Load<Texture2D>("Screens/MainScreen");
            PressP      = content.Load<SpriteFont>("Fonts/MainScreen");
            string Welcome_text = "Press    P    to    play";
            Draw(Welcome, WelcomePosition);

            if (timerForDraw_P < 1)
                DrawFont(PressP, Welcome_text, PressP_Position);

            if (timerForDraw_P > 2)
                timerForDraw_P = 0;

            if (playOn)
            {
                MediaPlayer.Play(WelcomeSong);
                playOn = false;
            }

            PlayOnBlack = true;
        }
        public void ScreenBlackGameOver(GameTime gameTime)
        {
            timerForDraw_P += gameTime.ElapsedGameTime.TotalSeconds;

            GameOverSong    = content.Load<Song>("Sounds/05_-_Arkanoid_-_NES_-_Game_Over");
            BlackGameOver   = content.Load<Texture2D>("Screens/Black");
            Draw(BlackGameOver, WelcomePosition);

            if (PlayOnBlack)
            {
                MediaPlayer.Play(GameOverSong);
                PlayOnBlack = false;
            }

        }

        private void Draw(Texture2D texture, Vector2 position)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(texture, position, Color.White);
            SpriteBatch.End();
        }

        private void DrawFont(SpriteFont texture, String TextToShow, Vector2 position)
        {
            SpriteBatch.Begin();
            SpriteBatch.DrawString(texture, TextToShow, position, Color.White);
            SpriteBatch.End();
        }
    }
}
