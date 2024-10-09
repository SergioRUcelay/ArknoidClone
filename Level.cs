using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Arkanoid_02
{
    public class Level
    {
        private readonly ContentManager content;

        private readonly SpriteBatch spriteBatch;
        private Texture2D backGround, scoreZone;

        public readonly List<Brick> brickList = new();
       
        public Level(IServiceProvider serviceProvider, SpriteBatch spriteBatch, ContentManager content)
        {
            this.content = content;
            this.spriteBatch = spriteBatch;
        }

        public void Iniciate(int _level)
        {
            LoadBackground(_level);
            BrickLayout(_level);
        }

        public void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(backGround, new Vector2(0, 0), Color.White);
            spriteBatch.Draw(scoreZone, new Vector2(0, 0), Color.White);

            foreach (var brick in brickList)
            {
                // Here call a Draw method of the objet.
                brick.Draw(gameTime);
            }

            foreach (var brick in brickList)
            {
                // Here call a Draw method of the animation objet.
                if (brick.Blast != null && brick.Blast.IsAnimaActive)
                {
                    brick.Blast.Draw(spriteBatch, brick.R_Blast);
                    brick.Blast.Update(gameTime);
                }
            }

            foreach (var brick in brickList)
            {
                // Here call a Draw method of the animation objet.
                if (brick.Glint != null && brick.Glint.IsAnimaActive)
                {
                    brick.Glint.Update(gameTime);
                    brick.Glint.Draw(spriteBatch, brick.R_Collider);
                }
            }
        }

        private void LoadBackground(int levelNumber)
        {
            scoreZone = content.Load<Texture2D>("Items/ScoresZone");
            string backGroundPath = string.Format("Levels/Level0{0}", levelNumber);
            backGround = content.Load<Texture2D>(backGroundPath);
        }

        private void BrickLayout(int levelNumber)
        {
            brickList.Clear();
            Vector2 bricksize = new(61, 30);
            Vector2 position  = new(0 , 0);

            //for all lines
            for (int i = 0; i < Levels.Level.GetLength(1); i++)
            {
                position.X = 0;
                //for each character
                for (int k = 0; k < Levels.Level[0,0].Length; k++)
                {
                    switch (Levels.Level[levelNumber, i][k])
                    {
                        case ',':
                        case '.':
                            position.X += bricksize.X;
                            break;

                        case '1':
                            var brick = new Brick(Hard.Blue, content, spriteBatch, "Items/BlueBlock", position);
                            brickList.Add(brick);
                            ArkaGame.SegmentsList.AddRange(brick.GetSegments());
                            brick.OnHit = () => DestroyBrick(brick);
                            position.X += bricksize.X;
                            break;

                        case '2':
                            brick = new Brick(Hard.Yellow, content, spriteBatch, "Items/YellowBlock", position);
                            brickList.Add(brick);
                            ArkaGame.SegmentsList.AddRange(brick.GetSegments());
                            brick.OnHit = () => DestroyBrick(brick);
                            position.X += bricksize.X;
                            break;

                        case '3':
                            brick = new Brick(Hard.Green, content, spriteBatch, "Items/GreenBlock", position);
                            brickList.Add(brick);
                            ArkaGame.SegmentsList.AddRange(brick.GetSegments());
                            brick.OnHit = () => DestroyBrick(brick);
                            position.X += bricksize.X;
                            break;

                        case '4':
                            brick = new Brick(Hard.Pink, content, spriteBatch, "Items/PinkBlock", position);
                            brickList.Add(brick);
                            ArkaGame.SegmentsList.AddRange(brick.GetSegments());
                            brick.OnHit = () => DestroyBrick(brick);
                            position.X += bricksize.X;
                            break;

                        case '5':
                            brick = new Brick(Hard.Metal, content, spriteBatch, "Items/MetalBlock", position);
                            brickList.Add(brick);
                            ArkaGame.SegmentsList.AddRange(brick.GetSegments());
                            brick.OnHit = () => DestroyBrick(brick);
                            position.X += bricksize.X;
                            break;
                    }
                }
                position.Y += bricksize.Y;
            }
        }

        public static void DestroyBrick(Brick brick)
        {
            if (brick.IsDestructible)
            {
                brick.Hit--;
                if (brick.Hit >= 1)
                {
                    brick.BrickBounce.Play();
                    switch (brick.Hardness)
                    {
                        case Hard.Yellow:
                        case Hard.Pink:
                            brick.Glint.Start();
                            break;
                    }
                }

                if (brick.Hit <= 0)
                {
                    foreach (var segment in ArkaGame.SegmentsList)
                        segment.IsActiveSegment &= segment.Owner != brick;

                    brick.IsActive = false;
                    brick.Blast.Start();
                    brick.DestroyBounce.Play();

                    switch (brick.Hardness)
                    {
                        case Hard.Blue:
                        case Hard.Green:
                            ArkaGame.Points += 50;
                            break;

                        case Hard.Yellow:
                            ArkaGame.Points += 100;
                            break;

                        case Hard.Pink:
                            ArkaGame.Points += 300;
                            break;
                    }
                }
            }
            if (!brick.IsDestructible)
            {
                brick.Glint.Start();
                brick.MetalBounce.Play();
            }
        }
    }
}