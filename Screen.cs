using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace Arkanoid_02
{
    public class Screen
    {
        private Vector2 welcomePosition, pressP_Position;
        private readonly Texture2D welcome, blackGameOver;
        private readonly ContentManager content;
        private readonly SpriteBatch spriteBatch;
        private readonly SpriteFont pressP;

        private double timerForDraw_P;
        private readonly string  welcome_text = "Press    P    to    play";

        public Screen(IServiceProvider serviceProvider, SpriteBatch spriteBatch)
        {
            content             = new ContentManager(serviceProvider, "Content");
            welcome             = content.Load<Texture2D>("Screens/MainScreen");
            pressP              = content.Load<SpriteFont>("Fonts/MainScreen");
            blackGameOver       = content.Load<Texture2D>("Screens/Black"); 
            this.spriteBatch         = spriteBatch;        
            welcomePosition     = Vector2.Zero;
            pressP_Position     = new Vector2(230,530);  // Tex "Press P for play".
        }

        public void WelcomeScreen(GameTime gameTime)
        {
            timerForDraw_P += gameTime.ElapsedGameTime.TotalSeconds;

            Draw(welcome, welcomePosition);

            if (timerForDraw_P < 1)
                DrawFont(pressP, welcome_text, pressP_Position);

            if (timerForDraw_P > 2)
                timerForDraw_P = 0;
        }

        public void ScreenBlackGameOver(GameTime gameTime)
        {   
            Draw(blackGameOver, welcomePosition);
        }

        private void Draw(Texture2D texture, Vector2 position)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, Color.White);
            spriteBatch.End();
        }

        private void DrawFont(SpriteFont texture, String TextToShow, Vector2 position)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(texture, TextToShow, position, Color.White);
            spriteBatch.End();
        }
    }
}
