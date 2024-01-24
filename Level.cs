using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using static Game_Arka.SpriteArk;

namespace Arkanoid_02
{
    public class Level : IDisposable
    {
        public ContentManager content;             

        private Ball ball;
        private Player player;
        private Brick brick;
        public  Animations blast;
        public  Animations glow;
        public Screen screen;

        private readonly SpriteBatch SpriteBatch;
        private SpriteFont NumberPointFont;
        private SpriteFont LifeLeft;
        private Texture2D backGround;
        private Texture2D ArkaLogo;
      
        private Song Newlevel;
       
        private Vector2 backGroundPosition;
        private Vector2 playerPosition;
        private Vector2 ballPosition;
        private Vector2 ani_pos;

        private readonly List<Brick> brickList = new List<Brick>();
        private readonly string[][] currentLevel = new string [30][];

        private int levelNumber;
        private int glowKey;
        public static double Time_lifeleft;
        public double Total_TimeLevel;
        private const int maxlevelNumber = 4;
        public int NumberBricks { set; get; }
        private int Points { get; set; }
        private int ExtraLifePoints { get; set; }
        private int MaxPoints { get; set; }

        // Flag for load Iniciate method only the first time call the Update method
        private bool iniOn;
        // Flag for print Maintext first load level, or wen died. Print the live less on screen.
        public static bool Maintext;
        // Flag for print NextLevel every load new level.
        public static bool NextLevel;
        // Flag to indicate that there was a collision, and call the glow animation.
        public bool glowOnn;

        public Level(IServiceProvider serviceProvider, SpriteBatch spriteBatch)
        {
            content = new ContentManager(serviceProvider, "Content");
            SpriteBatch = spriteBatch;
            backGroundPosition = new Vector2(0, 0);
            playerPosition = new Vector2(365, 810);
            ballPosition = new Vector2(380, 840);
            levelNumber = 1;            
            NumberBricks = 0;
            ExtraLifePoints = 6000;
            Points = 0;
            MaxPoints = 50000;            
            iniOn = true;
            Maintext = true;
            NextLevel = false;
            glowOnn = false;
        }

        private void Iniciate()
        {
            LoadBackgraund();

            // Sounds:
            Newlevel = content.Load<Song>("Sounds/02_-_Arkanoid_-_NES_-_Game_Start");

            //Textures:
            ArkaLogo = content.Load<Texture2D>("Items/ArkaLogo");

            // Fonts:
            NumberPointFont = content.Load<SpriteFont>("Fonts/Points");
            LifeLeft = content.Load<SpriteFont>("Fonts/Points");

            player = new Player(content, SpriteBatch,"Items/Player", playerPosition);
            player.AnimationAdd(1, player.playerAnimation);
            ball = new Ball(content, SpriteBatch, "Items/ball", ballPosition);

            BrickLayout(content);
            MediaPlayer.Play(Newlevel);           

            Time_lifeleft = 0;
            Total_TimeLevel = 0;
            
            iniOn = false;
           
        }

        public bool Update(GameTime gametime)
        {
            if (iniOn)
                Iniciate();
                        
            Time_lifeleft += gametime.ElapsedGameTime.TotalSeconds;

            player.Update(gametime);
            ball.Update(gametime, ref player.position);

            if (Points >= MaxPoints) MaxPoints = Points;

            if (!ball.WallBounce()) { ball.Death(); player.Death(); ball.can_move = false;
                player.can_move = false;}

            if (PlayerCollision() && ball.Play) { player.Bounce.Play(); }
            
            BrickCollision(brickList);
            Extralife();
                        
            if (NumberBricks <= 0)
            {
                Thread.Sleep(500);
                AchievedLevel();
            }
            return (player.Life > 0);
        }

        private void LoadBackgraund()
        { 
            if (levelNumber <= maxlevelNumber)
            {
                 string backGroundPath = string.Format("Levels/Level0{0}", levelNumber);
                 backGround = content.Load<Texture2D>(backGroundPath);
            }
            else { throw new NotSupportedException("Level number exceeds content."); }

        }

        private void MainText()
        {
            SpriteBatch.Draw(ArkaLogo, new Vector2(320, 620), Color.White);
            SpriteBatch.DrawString(LifeLeft, player.Life + "  LIVES  LEFT", new Vector2(290, 700), Color.White);
            SpriteBatch.DrawString(LifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        private void LevelText()
        {
            SpriteBatch.Draw(ArkaLogo, new Vector2(320, 620), Color.White);
            SpriteBatch.DrawString(LifeLeft, levelNumber + "   ROUND", new Vector2(345, 700), Color.White);
            SpriteBatch.DrawString(LifeLeft, "READY!!", new Vector2(345, 750), Color.White);
        }

        private void BrickLayout(ContentManager content)
        {
            Vector2 iniposition = new (0,0);
            Vector2 position = new (0,0);
            Vector2 bricksizeX = new (61,0);
            Vector2 bricksizeY = new (0, 30);

            if (levelNumber <= maxlevelNumber)
            {
                string brickLayoutPath = string.Format(@"D:\cODEX\LaysCarpGameStudios\Arkanoid\Content\Levels\BlockLevel0{0}.txt", levelNumber);
                using StreamReader blockLine = new StreamReader(brickLayoutPath);
                int i = 0;
                while (blockLine.Peek() > -1)
                {
                    currentLevel[i] = new string[] { blockLine.ReadLine() };
                    i++;
                }

            }
            else { throw new NotSupportedException("Level number exceeds content."); }
            
            // Line ( Y )
            for (int i = 0; i < currentLevel.Length; i++)
            {
                // Colums ( X )
                for (int j = 0; j < currentLevel[i].Length; j++)
                {
                    // Char of the  X
                    for(int k = 0;k < currentLevel[i][j].Length; k++)
                    {
                        switch (currentLevel[i][j][k])
                        {
                            case ',':
                                position += bricksizeX;
                                break;

                            case '.':
                                position += bricksizeX;
                                break;

//all the following cases using just one
///hits:
///use         case default
///use a map (string, tuple)
///map(string, (string, int))
///use tuples
///profit!
  //                              break;
  //
                            case '1':
                                brick= new Brick(Hard.Blue, content, SpriteBatch, "Items/BlueBlock", position);
                                brickList.Add(brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.05f);
                                brick.AnimationAdd(1, blast);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '2':
                                brick = new Brick(Hard.Yellow, content, SpriteBatch, "Items/YellowBlock", position);
                                brickList.Add(brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.05f);
                                glow = new Animations(content, "Animation/Animation_YelowBlock_7", 7, 1, 0.05f);
                                brick.AnimationAdd(1, blast);
                                brick.AnimationAdd(2,glow);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '3':
                                brick = new Brick(Hard.Green, content, SpriteBatch, "Items/GreenBlock", position);
                                brickList.Add(brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.05f);
                                brick.AnimationAdd(1, blast);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '4':
                                brick = new Brick(Hard.Pink, content, SpriteBatch, "Items/PinkBlock", position);
                                brickList.Add(brick);
                                blast = new Animations(content, "Animation/Blast_animation", 7, 1, 0.05f);
                                glow = new Animations(content, "Animation/Animation_PinkBlock_7", 7, 1, 0.05f);
                                brick.AnimationAdd(1, blast);
                                brick.AnimationAdd(2, glow);
                                position += bricksizeX;
                                NumberBricks++;
                                break;

                            case '5':
                                brick = new Brick(Hard.Metal, content, SpriteBatch, "Items/MetalBlock", position);
                                brickList.Add(brick);
                                position += bricksizeX;
                                break;
                        }

                    }
                    position.X = iniposition.X;
                }
                position.Y += bricksizeY.Y;
            }

        }
              
        public bool PlayerCollision()
        {
            if (player.visible == false) return false;

            if (player.position.X + player.Size.X > ball.position.X && (ball.position.X + ball.Size.X) > player.position.X &&
                    (player.position.Y + player.Size.Y) > ball.position.Y && (ball.position.Y + ball.Size.Y) > player.position.Y)
            {
                // Right
                if (ball.position.X > player.position.X && ball.position.X + ball.Size.X > player.position.X + player.Size.X && ball.velocity.X < 0)
                {
                    ball.Bounce(new Vector2(1, 0));
                }

                // Left
                if (ball.position.X < player.position.X && ball.position.X + ball.Size.X < player.position.X + player.Size.X && ball.velocity.X > 0)
                {
                    ball.Bounce(new Vector2(1, 0));
                }

                // Top
                if (ball.position.Y < player.position.Y && ball.position.Y + ball.Size.Y < player.position.Y + player.Size.Y && ball.velocity.Y > 0)
                {
                    ball.Bounce(new Vector2(0, 1));
                }

                // Botton
                if (ball.position.Y > player.position.Y && ball.position.Y + ball.Size.Y > player.position.Y + player.Size.Y && ball.position.X < player.position.X + player.Size.X && ball.velocity.Y < 0)
                {
                    ball.Bounce(new Vector2(0, 1));
                }
                return true;
            }
            return false;
        }
        public bool BrickCollision(List<Brick> bricksOrder)
        {
            foreach (Brick brick in bricksOrder)
            {
                bool on = true;
                if (brick.visible)
                {
                    if (ball.R_Collider.Intersects(brick.R_Collider))
                    {
                     
                        // Right
                        if (ball.position.X > brick.position.X && ball.position.X + ball.Size.X > brick.position.X + brick.Size.X && ball.velocity.X < 0 && on)
                        {
                            ball.Bounce(new Vector2(1, 0));
                            on = false;
                        }

                        // Left
                        if (ball.position.X < brick.position.X && ball.position.X + ball.Size.X < brick.position.X + brick.Size.X && ball.velocity.X > 0 && on)
                        {
                            ball.Bounce(new Vector2(1, 0));
                            on = false;
                        }

                        // Top
                        if (ball.position.Y < brick.position.Y && ball.position.Y + ball.Size.Y < brick.position.Y + brick.Size.Y && ball.velocity.Y > 0 && on)
                        {
                            ball.Bounce(new Vector2(0, 1));
                            on = false;
                        }

                        // Botton
                        if (ball.position.Y > brick.position.Y && ball.position.Y + ball.Size.Y > brick.position.Y + brick.Size.Y && ball.position.X < brick.position.X + brick.Size.X && ball.velocity.Y < 0 && on)
                        {
                            ball.Bounce(new Vector2(0, -1));
                           
                        }
                        
                        if (brick.destructible)
                        {
                            brick.Hit--;
                            if (brick.Hit >= 1)
                            {
                                brick.BrickBounce.Play();
                                switch (brick.hardness)
                                {
                                    case Hard.Yellow:
                                        glowOnn = true;
                                        glowKey = 2;
                                        ani_pos = brick.position;

                                        break;

                                    case Hard.Pink:
                                        glowOnn = true;
                                        glowKey = 3;
                                        ani_pos = brick.position;
                                        break;
                                }
                            }


                            if (brick.Hit <= 0)
                            {
                                brick._animation = true;
                                brick.SetVisible(false);
                                brick.DesdtroyBounce.Play();
                                NumberBricks--;

                                switch (brick.hardness)
                                {
                                    case Hard.Blue:
                                        Points += 50;
                                        break;

                                    case Hard.Green:
                                        Points += 50;
                                        break;

                                    case Hard.Yellow:
                                        Points += 100;
                                        break;

                                    case Hard.Pink:
                                        Points += 300;
                                        break;
                                }

                            }

                        }
                        if (!brick.destructible)
                            brick.MetalBounce.Play();
                        
                    }
                }

            }
            return false;
        }
        //public bool BrickCollision(List<Brick> bricksOrder)
        //{
        //    foreach (Brick brick in bricksOrder)
        //    {
        //        if (brick.visible)
        //        {
        //            if (brick.position.X + brick.Size.X > ball.position.X && (ball.position.X + ball.Size.X) > brick.position.X &&
        //                (brick.position.Y + brick.Size.Y) > ball.position.Y && (ball.position.Y + ball.Size.Y) > brick.position.Y)
        //            {
        //                // Right
        //                if (ball.position.X > brick.position.X && ball.position.X + ball.Size.X > brick.position.X + brick.Size.X && ball.velocity.X < 0)
        //                {
        //                    ball.Bounce(new Vector2(1, 0));
        //                    continue;
        //                }

        //                // Left
        //                if (ball.position.X < brick.position.X && ball.position.X + ball.Size.X < brick.position.X + brick.Size.X && ball.velocity.X > 0)
        //                {
        //                    ball.Bounce(new Vector2(1, 0));
        //                    continue;
        //                }

        //                // Top
        //                if (ball.position.Y < brick.position.Y && ball.position.Y + ball.Size.Y < brick.position.Y + brick.Size.Y && ball.velocity.Y > 0)
        //                {
        //                    ball.Bounce(new Vector2(0, 1));
        //                    continue;
        //                }

        //                // Botton
        //                if (ball.position.Y > brick.position.Y && ball.position.Y + ball.Size.Y > brick.position.Y + brick.Size.Y && ball.position.X < brick.position.X + brick.Size.X && ball.velocity.Y < 0)
        //                {
        //                    ball.Bounce(new Vector2(0, -1));
        //                    continue;
        //                }

        //                if (brick.destructible)
        //                {
        //                    brick.hit--;
        //                    if (brick.hit >= 1)
        //                        brick.BrickBounce.Play();

        //                    if (brick.hit <= 0)
        //                    {
        //                        brick.SetVisible(false);
        //                        brick.DesdtroyBounce.Play();
        //                        NumberBricks--;

        //                        switch (brick.hardness)
        //                        {
        //                            case Hard.Blue:
        //                                Points += 50;
        //                                break;

        //                            case Hard.Green:
        //                                Points += 50;
        //                                break;

        //                            case Hard.Yellow:
        //                                Points += 100;
        //                                break;

        //                            case Hard.Pink:
        //                                Points += 300;
        //                                break;
        //                        }

        //                    }

        //                }
        //                if (!brick.destructible)
        //                    brick.MetalBounce.Play();
        //            }
        //        }

        //    }
        //    return false;
        //}

        private void CallAnimation(int i, Vector2 position)
        {
            //Animations.Draw()
        }

        public void AchievedLevel()
        {   
            Dispose();
            
            levelNumber++;
            if (levelNumber > 4)
                levelNumber = 1;

            brickList.Clear();
            NumberBricks = 0;
            Maintext = false;
            NextLevel = true;
            ball.can_move = false;
            player.can_move = false;

            Iniciate();
           
        }

        public void Extralife()
        {
            if (Points >= ExtraLifePoints)
            {
                ExtraLifePoints += Points;
                player.Life += 1;
                player.ExtraLife.Play();

            }
        }

        public void GameOver()
        {
            Dispose();
            brickList.Clear();
            iniOn = true;
            Maintext = true;
            NextLevel = false;
            ball.can_move = false;
            player.can_move = false;
            Time_lifeleft = 0;
            levelNumber = 1;
            Points = 0;
            NumberBricks = 0;
            ExtraLifePoints = 3000;
        }


        public void Draw(GameTime gameTime)
        {

            SpriteBatch.Draw(backGround, backGroundPosition, Color.White);
            
            // Score
            string points = Points.ToString();
            SpriteBatch.DrawString(NumberPointFont, points, new Vector2(125,20), Color.WhiteSmoke);

            // High Socre
            string maxpoints = MaxPoints.ToString();
            SpriteBatch.DrawString(NumberPointFont, maxpoints, new Vector2(400,20), Color.WhiteSmoke);
            
            // Round
            string l_Number = levelNumber.ToString();
            SpriteBatch.DrawString(NumberPointFont, l_Number, new Vector2(635, 34), Color.WhiteSmoke);

            // Lives
            string l_life = player.Life.ToString();
            SpriteBatch.DrawString(NumberPointFont, l_life, new Vector2(775, 10), Color.WhiteSmoke);

            if (Maintext)
            {
                if (Time_lifeleft < 2.5)
                    MainText();
            }

            if (NextLevel)
            {
                if (Time_lifeleft < 2.5)
                    LevelText();
            }

            //// Here call a Draw method of the animation objet. We need call a Dictionary that containt all the animation created.
            //if (brick._animation)
            //{
            //    foreach (var brick in brickList)
            //    {
            //        brick.ani_manager[1].Update(gameTime);
            //        brick.ani_manager[1].Draw(SpriteBatch, brick.position);
            //    }
            //}

            // // Here call a Draw method of the objet.
            //if (!brick._animation)
            //{
            //    foreach (var brick in brickList)
            //    {
            //        brick.Draw(gameTime);
            //    }
            //}
            
            foreach (var brick in brickList)
            {
                // Here call a Draw method of the objet.
                if (!brick._animation)
                    brick.Draw(gameTime);

                // Here call a Draw method of the animation objet. We need call a Dictionary that containt all the animation created.
                if (brick._animation)
                {
                    brick.ani_manager[1].Update(gameTime);
                    brick.ani_manager[1].Draw(SpriteBatch, brick.position);
                }
                    
                

            }

            if (Time_lifeleft > 2.6)
            {
                player.ani_manager[1].Update(gameTime);             
                player.ani_manager[1].Draw(SpriteBatch, player.position);             
                ball.Draw(gameTime);
                ball.can_move = true;
                player.can_move = true;
            }

        }

        public void Dispose() => content.Unload();

     }
}       
        
