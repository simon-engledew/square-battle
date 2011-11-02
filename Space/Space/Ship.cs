using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Space
{
    class Ship
    {
        BasicEffect basicEffect;
        VertexPositionColor[] vertices;
        float rotation = 0;
        Vector2 velocity = Vector2.Zero;
        Vector2 position = Vector2.Zero;
        GraphicsDevice graphics;

        public Ship(GraphicsDevice graphicsDevice)
        {
            this.graphics = graphicsDevice;

            basicEffect = new BasicEffect(graphics);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, graphics.Viewport.Width,
                graphics.Viewport.Height, 0,
                0, 1
            );

            vertices = new VertexPositionColor[4];

            vertices[0].Position = new Vector3(0, -10, 0);
            vertices[0].Color = Color.Green;

            vertices[1].Position = new Vector3(-10, 10, 0);
            vertices[1].Color = Color.Green;

            vertices[2].Position = new Vector3(10, 10, 0);
            vertices[2].Color = Color.Green;

            vertices[3].Position = new Vector3(0, -10, 0);
            vertices[3].Color = Color.Green;

            Point center = graphics.Viewport.TitleSafeArea.Center;

            this.position = new Vector2(center.X, center.Y);
        }

        public void Update(GameTime gameTime)
        {
            float timedelta = gameTime.ElapsedGameTime.Milliseconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) rotation -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) rotation += 0.1f;

            Vector2 velocityDelta = new Vector2(
                (float)Math.Cos(rotation - MathHelper.PiOver2),
                (float)Math.Sin(rotation - MathHelper.PiOver2)
            );

            if (Keyboard.GetState().IsKeyDown(Keys.Up)) velocityDelta *= 0.4f;
            else if (Keyboard.GetState().IsKeyDown(Keys.Down)) velocityDelta *= -0.4f;
            else velocityDelta *= 0.0f;

            velocity += velocityDelta;

            Vector2.Add(ref position, ref velocity, out position);

            position = VectorExtensions.Modulo(position, new Vector2(graphics.Viewport.Width, graphics.Viewport.Height));

            basicEffect.World = Matrix.Multiply(Matrix.CreateRotationZ(rotation), Matrix.CreateTranslation(position.X, position.Y, 0));
        }

        public void Draw()
        {
            basicEffect.CurrentTechnique.Passes[0].Apply();

            graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertices, 0, 3);

        }
    }
}
