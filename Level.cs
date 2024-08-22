using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Arkanoid_02
{
    public class Level
    {
        private readonly ContentManager _content;

        private readonly SpriteBatch _spriteBatch;
        private Texture2D _backGround;
        private Texture2D _scoreZone;

        public readonly List<Brick> _brickList = new();
        private readonly string[][] _currentLevel = new string[30][];
       
        public Level(IServiceProvider serviceProvider, SpriteBatch spriteBatch, ContentManager content)
        {
            _content = content;
            _spriteBatch = spriteBatch;
        }

        public void Iniciate(int _level)
        {
            LoadBackground(_level);
            BrickLayout(_level);
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_backGround, new Vector2(0, 0), Color.White);
            _spriteBatch.Draw(_scoreZone, new Vector2(0, 0), Color.White);

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the objet.
                brick.Draw(gameTime);
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet.
                if (brick.Blast != null && brick.Blast.AnimaActive)
                {
                    brick.Blast.Draw(_spriteBatch, brick.R_blast);
                    brick.Blast.Update(gameTime);
                }
            }

            foreach (var brick in _brickList)
            {
                // Here call a Draw method of the animation objet.
                if (brick.Glint != null && brick.Glint.AnimaActive)
                {
                    brick.Glint.Update(gameTime);
                    brick.Glint.Draw(_spriteBatch, brick.R_Collider);
                }
            }
        }

        private void LoadBackground(int _levelNumber)
        {
            _scoreZone = _content.Load<Texture2D>("Items/ScoresZone");
             string backGroundPath = string.Format("Levels/Level0{0}", _levelNumber);
             _backGround = _content.Load<Texture2D>(backGroundPath);            
        }

        private void BrickLayout(int _levelNumber)
        {
            _brickList.Clear();

            Vector2 iniposition = new(0, 0);
            Vector2 position = new(0, 0);
            Vector2 bricksizeX = new(61, 0); // new (_brick.Size.X,0);
            Vector2 bricksizeY = new(0, 30); // new (0, _brick.Size.Y);

            string brickLayoutPath = string.Format(@"D:\cODEX\LaysCarpGameStudios\Arkanoid\Content\Levels\BlockLevel0" + _levelNumber + ".txt");//, _levelNumber);
            using StreamReader blockLine = new (brickLayoutPath);
            int e = 0;
            while (blockLine.Peek() > -1)
            {
                _currentLevel[e] = new string[] { blockLine.ReadLine() };
                e++;
            }
            
            // Line ( Y )
            for (int i = 0; i < _currentLevel.Length; i++)
            {
                // Colums ( X )
                for (int j = 0; j < _currentLevel[i].Length; j++)
                {
                    // Char of the  X
                    for (int k = 0; k < _currentLevel[i][j].Length; k++)
                    {
                        switch (_currentLevel[i][j][k])
                        {
                            case ',':
                            case '.':
                                position += bricksizeX;
                                break;

                            //all the following cases using just one
                            ///hits:
                            ///use         case default
                            ///use a map (string, tuple) (C# Dictionary is equivalent to C++ unordered_map. Furthermore, C# SortedDictionary is equivalent to C++ map.)
                            ///map(string, (string, int))
                            ///use tuples
                            ///profit!
                            //                              break;
                            //
                            case '1':
                                var _brick = new Brick(Hard.Blue, _content, _spriteBatch, "Items/BlueBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                break;

                            case '2':
                                _brick = new Brick(Hard.Yellow, _content, _spriteBatch, "Items/YellowBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                break;

                            case '3':
                                _brick = new Brick(Hard.Green, _content, _spriteBatch, "Items/GreenBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                break;

                            case '4':
                                _brick = new Brick(Hard.Pink, _content, _spriteBatch, "Items/PinkBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                break;

                            case '5':
                                _brick = new Brick(Hard.Metal, _content, _spriteBatch, "Items/MetalBlock", position);
                                _brickList.Add(_brick);
                                ArkaGame._segments.AddRange(_brick.GetSegments());
                                _brick.OnHit = () => DestroyBrick(_brick);
                                position += bricksizeX;
                                break;
                        }
                    }
                    position.X = iniposition.X;
                }
                position.Y += bricksizeY.Y;
            }
        }

        public void DestroyBrick(Brick brick)
        {
            if (brick.destructible)
            {
                brick.Hit--;
                if (brick.Hit >= 1)
                {
                    brick.BrickBounce.Play();
                    switch (brick.hardness)
                    {
                        case Hard.Yellow:
                        case Hard.Pink:
                            brick.Glint.Start();
                            break;
                    }
                }

                if (brick.Hit <= 0)
                {
                    foreach (var segment in ArkaGame._segments)
                        segment.ActiveSegment &= segment.Owner != brick;

                    brick.Active = false;
                    brick.Blast.Start();
                    brick.DestroyBounce.Play();

                    switch (brick.hardness)
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
            if (!brick.destructible)
            {
                brick.Glint.Start();
                brick.MetalBounce.Play();
            }
        }
    }
}       
        
