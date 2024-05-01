using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using Microsoft.Xna.Framework.Media;

namespace Arkanoid_02
{
    public class Screen
    {
        private Song WellcomeSong;
        private Song GameOverSong;

        private Texture2D Wellcome;
        private Texture2D BlackGameOver;

        private Vector2 WellcomePosition;
        private Vector2 BlackGameOver_Position;
        private Vector2 PressP_Position;

        private readonly ContentManager content;
        private readonly SpriteBatch SpriteBatch;
        private SpriteFont PressP;

        private double time_draw;

        public bool playOn; // Flag for play de Melody of The Game.
        public bool PlayOnBlack; // Flag for play de Melody of The Game Over Screen.
        public bool GO_On; // Flag to know when show Game Over Screen.

        public Screen(IServiceProvider serviceProvider, SpriteBatch spriteBatch)
        {
            content = new ContentManager(serviceProvider, "Content");
            SpriteBatch = spriteBatch;
            BlackGameOver_Position = new Vector2(221,450);  // Tex "Game Over".
            WellcomePosition = Vector2.Zero;
            PressP_Position = new Vector2(230,530);  // Tex "Press P for play".
            
            playOn = true;
        }

        public void WellcomeScreen(GameTime gameTime)
        {
            time_draw += gameTime.ElapsedGameTime.TotalSeconds;

            WellcomeSong = content.Load<Song>("Sounds/Start_Demo");
            Wellcome = content.Load<Texture2D>("Screens/MainScreen");
            PressP = content.Load<SpriteFont>("Fonts/MainScreen");
            string Wellcome_text = "Press    P    to    play";
            Draw(Wellcome, WellcomePosition);

            if (time_draw < 1)
                DrawFont(PressP, Wellcome_text, PressP_Position);

            if (time_draw > 2)
                time_draw = 0;

            if (playOn)
            {
                MediaPlayer.Play(WellcomeSong);
                playOn = false;
            }

            PlayOnBlack = true;
        }
        public bool ScreenBlackGameOver(GameTime gameTime)
        {
            
            time_draw += gameTime.ElapsedGameTime.TotalSeconds;

            GameOverSong = content.Load<Song>("Sounds/05_-_Arkanoid_-_NES_-_Game_Over");

            if (time_draw <= 4)
            {
                BlackGameOver = content.Load<Texture2D>("Screens/Black");
                Draw(BlackGameOver, WellcomePosition);
                //PressP = content.Load<SpriteFont>("Fonts/MainScreen");
                //string Game_Over = "G A M E    O V E R";
                //DrawFont(PressP, Game_Over, BlackGameOver_Position);
            }

            if (PlayOnBlack)
            {
                MediaPlayer.Play(GameOverSong);
                PlayOnBlack = false;
            }

            if (time_draw > 4)
            {
                time_draw = 0;  
                return true;

            }
            return false;
            
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
