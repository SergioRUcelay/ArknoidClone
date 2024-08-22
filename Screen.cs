using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace Arkanoid_02
{
    public class Screen
    {
        private Texture2D Welcome;
        private Texture2D BlackGameOver;

        private Vector2 WelcomePosition;
        private Vector2 BlackGameOver_Position;
        private Vector2 PressP_Position;

        private readonly ContentManager content;
        private readonly SpriteBatch SpriteBatch;
        private SpriteFont PressP;

        private double timerForDraw_P;
        string Welcome_text = "Press    P    to    play";
        public bool GO_On; // Flag to know when show Game Over Screen.

        public Screen(IServiceProvider serviceProvider, SpriteBatch spriteBatch)
        {
            content                = new ContentManager(serviceProvider, "Content");
            Welcome                = content.Load<Texture2D>("Screens/MainScreen");
            PressP                 = content.Load<SpriteFont>("Fonts/MainScreen");
            BlackGameOver          = content.Load<Texture2D>("Screens/Black"); 
            SpriteBatch            = spriteBatch;
            BlackGameOver_Position = new Vector2(221,450);  // Tex "Game Over".
            WelcomePosition        = Vector2.Zero;
            PressP_Position        = new Vector2(230,530);  // Tex "Press P for play".
        }

        public void WelcomeScreen(GameTime gameTime)
        {
            timerForDraw_P += gameTime.ElapsedGameTime.TotalSeconds;

            Draw(Welcome, WelcomePosition);

            if (timerForDraw_P < 1)
                DrawFont(PressP, Welcome_text, PressP_Position);

            if (timerForDraw_P > 2)
                timerForDraw_P = 0;
        }

        public void ScreenBlackGameOver(GameTime gameTime)
        {   
            Draw(BlackGameOver, WelcomePosition);
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
