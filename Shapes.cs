using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Arkanoid
{
    public sealed class Shapes : IDisposable
    {
        public static readonly float MinLineThickness = 1.0f;
        public static readonly float MaxLineThickness = 10f;

        private bool isDispose;
        private Game game;
        private BasicEffect effect;

        private readonly VertexPositionColor[] vertices;
        private readonly int[] indices;

        private int shapeCount;
        private int vertexCount;
        private int indexCount;

        private bool isStarted;

        public Shapes(Game game)
        {
            isDispose = false;
            this.game = game ?? throw new ArgumentNullException(nameof(game));

            effect = new(game.GraphicsDevice)
            {
                TextureEnabled = false,
                FogEnabled = false,
                LightingEnabled = false,
                VertexColorEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity,
                Projection = Matrix.Identity
            };

            const int MaxVertexCount = 1024;
            const int MaxIndexCount = MaxVertexCount * 3;

            vertices = new VertexPositionColor[MaxVertexCount];
            indices = new int[MaxIndexCount];

            shapeCount = 0;
            vertexCount = 0;
            indexCount = 0;

            isStarted = false;
        }

        public void Dispose()
        {
            if (isDispose) return;

            effect?.Dispose();
            isDispose = true;
        }

        public void Begin()
        {
            if (isStarted)
            {
                throw new Exception("batching is already started.");
            }

            //Viewport vp = game.GraphicsDevice.Viewport;
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, game.GraphicsDevice.Viewport.Width,0, game.GraphicsDevice.Viewport.Height, 0f, 1f);
           // effect.Projection = Matrix.CreateOrthographicOffCenter(0, 10, 0, 10, 0f, 1f);

            isStarted = true;
        }

        public void End()
        {
            Flush();
            isStarted = false;
        }

        private void Flush()
        {
            if (shapeCount == 0)
                return;

            EnsureStarted();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList,
                    vertices, 0, vertexCount, indices, 0, indexCount / 3);
            }
            shapeCount = 0;
            vertexCount = 0;
            indexCount = 0;
        }

        private void EnsureStarted()
        {
            if (!isStarted)
            {
                throw new Exception("batching was never started.");
            }
        }

        public void EnsureSpace(int shapeVertexCount, int shapeIndexCount)
        {
            if (shapeVertexCount > vertices.Length) { throw new Exception("Maximun shape vertex count is: " + vertices.Length); }

            if (shapeIndexCount > indices.Length) { throw new Exception("Maximun shape index count is: " + indices.Length); }

            if (vertexCount + shapeVertexCount > vertices.Length ||
                indexCount + shapeIndexCount > indices.Length)
            {
                Flush();
            }
        }

        public void DrawRectangleFill(float x, float y, float width, float height, Color color)
        {
            EnsureStarted();

            const int shapeVertexCount = 4;
            const int shapeIndexCount = 6;

            EnsureSpace(shapeVertexCount, shapeIndexCount);

            float left = x;
            float right = x + width;
            float botton = y;
            float top = y + height;

            Vector2 a = new (left, top);
            Vector2 b = new (right, top);
            Vector2 c = new (right, botton);
            Vector2 d = new (left, botton);

            indices[indexCount++] = 0 + vertexCount;
            indices[indexCount++] = 1 + vertexCount;
            indices[indexCount++] = 2 + vertexCount;
            indices[indexCount++] = 0 + vertexCount;
            indices[indexCount++] = 2 + vertexCount;
            indices[indexCount++] = 3 + vertexCount;

            vertices[vertexCount++] = new(new Vector3(a, 0f), color);
            vertices[vertexCount++] = new(new Vector3(b, 0f), color);
            vertices[vertexCount++] = new(new Vector3(c, 0f), color);
            vertices[vertexCount++] = new(new Vector3(d, 0f), color);

            shapeCount++;
        }

        public void Drawline(Vector2 a, Vector2 b, float thickness, Color color)
        {
            EnsureStarted();

            const int shapeVertexCount = 4;
            const int shapeIndexCount = 6;

            EnsureSpace(shapeVertexCount, shapeIndexCount);

            //thickness = Util.Clamp(thickness, MinLineThickness, MaxLineThickness);
            //thickness++;

            float halfthickness = thickness / 2f;

            Vector2 e1 = b - a;
            e1.Normalize();
            e1 *= halfthickness;

            Vector2 e2 = -e1;
            Vector2 n1 = new(-e1.Y, e1.X);
            Vector2 n2 = -n1;

            Vector2 q1 = a + n1 + e2;
            Vector2 q2 = b + n1 + e1;
            Vector2 q3 = b + n2 + e1;
            Vector2 q4 = a + n2 + e2;

            indices[indexCount++] = 0 + vertexCount;
            indices[indexCount++] = 1 + vertexCount;
            indices[indexCount++] = 2 + vertexCount;
            indices[indexCount++] = 0 + vertexCount;
            indices[indexCount++] = 2 + vertexCount;
            indices[indexCount++] = 3 + vertexCount;

            vertices[vertexCount++] = new(new Vector3(q1, 0f), color);
            vertices[vertexCount++] = new(new Vector3(q2, 0f), color);
            vertices[vertexCount++] = new(new Vector3(q3, 0f), color);
            vertices[vertexCount++] = new(new Vector3(q4, 0f), color);
            shapeCount++;

        }
    }
}
