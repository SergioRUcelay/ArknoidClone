using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Arkanoid_02
{
    public class HandlerCollisions
    {
        private readonly Ball ball;
        //private readonly Brick brick;
        private readonly Player player;
        private readonly Level level;
        
        private readonly List<Brick> brickLayout = new List<Brick>();
        
        public HandlerCollisions(ref Ball ball, Player player, List<Brick> brickslist, Level level)
        {
            this.ball = ball;
            //this.brick = brick;
            this.player = player;
            this.level = level;
            brickLayout = brickslist;
        }

        public void Update()
        {
            if (BrickCollision(brickLayout))
            {
                


            }
            //Collision(brick);
            //PlayerCollision();
        }

        public bool BrickCollision(List<Brick> bricksOrder)
        {
            foreach (Brick brick in bricksOrder)
            {
                if (brick.visible == true)
                {
                    if (brick.position.X + brick.size.X > ball.position.X && (ball.position.X + ball.size.X) > brick.position.X &&
                        (brick.position.Y + brick.size.Y) > ball.position.Y && (ball.position.Y + ball.size.Y) > brick.position.Y)
                    {
                        // Right
                        if (ball.position.X > brick.position.X && ball.position.X + ball.size.X > brick.position.X + brick.size.X && ball.speed.X < 0)
                        {
                            ball.Bounce(new Vector2(1, 0));
                        }

                        // Left
                        if (ball.position.X < brick.position.X && ball.position.X + ball.size.X < brick.position.X + brick.size.X && ball.speed.X > 0)
                        {
                            ball.Bounce(new Vector2(-1, 0));
                        }

                        //// Top
                        if (ball.position.Y < brick.position.Y && ball.position.Y + ball.size.Y < brick.position.Y + brick.size.Y && ball.speed.Y > 0)
                        {
                            ball.Bounce(new Vector2(0, 1));
                        }

                        // Botton
                        if (ball.position.Y > brick.position.Y && ball.position.Y + ball.size.Y > brick.position.Y + brick.size.Y && ball.position.X < brick.position.X + brick.size.X && ball.speed.Y < 0)
                        {
                            ball.Bounce(new Vector2(0, 1));
                        }

                        if (brick.destructible == true)
                        {
                            brick.hit--;

                            if (brick.hit <= 0)
                            {
                                brick.SetVisible(false);
                                level.numberBricks--;                             
                            }

                        }
                    }
                }

            }
            return false;
        }

        public bool Collision(Brick brick)
        {
            if (brick.visible == true)
            {
                if (brick.position.X + brick.size.X > ball.position.X && (ball.position.X + ball.size.X) > brick.position.X &&
                    (brick.position.Y + brick.size.Y) > ball.position.Y && (ball.position.Y + ball.size.Y) > brick.position.Y)
                {
                    // Right
                    if (ball.position.X > brick.position.X && ball.position.X + ball.size.X > brick.position.X + brick.size.X && ball.speed.X < 0)
                    {
                        ball.Bounce(new Vector2(1, 0));
                    }

                    // Left
                    if (ball.position.X < brick.position.X && ball.position.X + ball.size.X < brick.position.X + brick.size.X && ball.speed.X > 0)
                    {
                        ball.Bounce(new Vector2(1, 0));
                    }

                    //// Top
                    if (ball.position.Y < brick.position.Y && ball.position.Y + ball.size.Y < brick.position.Y + brick.size.Y && ball.speed.Y > 0)
                    {
                        ball.Bounce(new Vector2(0, 1));
                    }

                    // Botton
                    if (ball.position.Y > brick.position.Y && ball.position.Y + ball.size.Y > brick.position.Y + brick.size.Y && ball.position.X < brick.position.X + brick.size.X && ball.speed.Y < 0)
                    {
                        ball.Bounce(new Vector2(0, 1));
                    }

                    brick.SetVisible(false);
                }
            }
            return false;
        }

        public bool PlayerCollision()
        {
            if(player.visible == false) return false;

            if (player.position.X + player.size.X > ball.position.X && (ball.position.X + ball.size.X) > player.position.X &&
                    (player.position.Y + player.size.Y) > ball.position.Y && (ball.position.Y + ball.size.Y) > player.position.Y)
            {
                // Right
                if (ball.position.X > player.position.X && ball.position.X + ball.size.X > player.position.X + player.size.X && ball.speed.X < 0)
                {   
                    ball.Bounce(new Vector2(1, 0));
                }

                // Left
                if (ball.position.X < player.position.X && ball.position.X + ball.size.X < player.position.X + player.size.X && ball.speed.X > 0)
                {
                    ball.Bounce(new Vector2(1, 0));
                }

                // Top
                if (ball.position.Y < player.position.Y && ball.position.Y + ball.size.Y < player.position.Y + player.size.Y && ball.speed.Y > 0)
                {
                        ball.Bounce(new Vector2(0, 1));
                }

                // Botton
                if (ball.position.Y > player.position.Y && ball.position.Y + ball.size.Y > player.position.Y + player.size.Y && ball.position.X < player.position.X + player.size.X && ball.speed.Y < 0)
                {
                    ball.Bounce(new Vector2(0, 1));
                }
            }
            return false;
        }

       /* public bool PlayerCollision()
        {
            if (player.visible == false) return false;

            if (player.position.X + player.size.X > ball.position.X && (ball.position.X + ball.size.X) > player.position.X &&
                    (player.position.Y + player.size.Y) > ball.position.Y && (ball.position.Y + ball.size.Y) > player.position.Y)
            {
                // Right
                if (ball.position.X > player.position.X && ball.position.X + ball.size.X > player.position.X + player.size.X && ball.speed.X < 0)
                {
                    ball.Bounce(new Vector2(1, 0));
                }

                // Left
                if (ball.position.X < player.position.X && ball.position.X + ball.size.X < player.position.X + player.size.X && ball.speed.X > 0)
                {
                    ball.Bounce(new Vector2(1, 0));
                }

                // Top
                if (ball.position.Y < player.position.Y && ball.position.Y + ball.size.Y < player.position.Y + player.size.Y && ball.speed.Y > 0)
                {
                    ball.Bounce(new Vector2(0, 1));
                }

                // Botton
                if (ball.position.Y > player.position.Y && ball.position.Y + ball.size.Y > player.position.Y + player.size.Y && ball.position.X < player.position.X + player.size.X && ball.speed.Y < 0)
                {
                    ball.Bounce(new Vector2(0, 1));
                }
            }
            return false;
        }*/

       

    }

}
