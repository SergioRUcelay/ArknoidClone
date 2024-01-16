using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Arkanoid_02
{
    public class HandlerCollisions
    {
        private readonly Ball ball;
        private readonly Brick brick;
        private readonly Player player;
        private readonly BackFrameBounces mainScreen;

        private readonly List<Brick> brickLayout = new List<Brick>();

        public HandlerCollisions(ref Ball ball, Player player, Brick brick, List<Brick> brickslist)
        {
            this.ball = ball;
            this.brick = brick;
            this.player = player;
            brickLayout = brickslist;

            mainScreen = new BackFrameBounces();
        }

        public void Update()
        {
           
            WallBounce();
            //BrickCollision(brickLayout);
            Collision(brick);
            PlayerCollision();
        }

        public bool BrickCollision(List<Brick> bricksOrder)
        {
            for (int i = 0;i<bricksOrder.Count;i++)
            {

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

        public void WallBounce()
        {

            // TopWall
            if (ball.position.Y <= mainScreen.A_point.Y)
                ball.FrameBounce(mainScreen.CD_point);

            // RightWall
            if (ball.position.X >= mainScreen.D_point.X)
                ball.FrameBounce(mainScreen.AC_point);

            // LeftWall
            if (ball.position.X <= mainScreen.C_point.X)
                ball.FrameBounce(mainScreen.AC_point);

            // BottomWall
            if (ball.position.Y >= mainScreen.D_point.Y + 150)
            {
                ball.Death(); //Ball.FrameBounce(mainScreen.CD_point);
                player.Death(); //Ball.FrameBounce(mainScreen.CD_point);
            }

        }

    }

}
