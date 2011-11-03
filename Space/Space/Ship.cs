using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common.PolygonManipulation;

namespace Space
{
    public static class BodyExtensions
    {
        public static void Draw(this Body body, GraphicsDevice graphics, Color color)
        {
            foreach (Fixture fixture in body.FixtureList)
            {
                fixture.Draw(graphics, color, Matrix.Multiply(Matrix.CreateRotationZ(body.Rotation), Matrix.CreateTranslation(new Vector3(body.Position, 0.0f))));
            }
        }
    }

    public static class FixtureExtensions
    {
        public static void Draw(this Fixture fixture, GraphicsDevice graphics, Color color, Matrix? matrix = null)
        {
            VertexPositionColor[] vertices = ((PolygonShape)fixture.Shape).ToVertices(color, matrix);
            
            graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertices, 0, vertices.Length - 1);
        }
    }

    public static class PolygonShapeExtensions
    {
        public static VertexPositionColor[] ToVertices(this PolygonShape shape, Color color, Matrix? matrix = null)
        {
            // create a new vertex array to hold our drawing information (one bigger than the number of Vertices to loop back round and close the rectangle
            VertexPositionColor[] vertices = new VertexPositionColor[shape.Vertices.Count + 1];

            // loop through the vertices, saving each one into an array slot
            int i = 0;
            foreach (Vector2 vector in shape.Vertices)
            {
                vertices[i].Position = new Vector3(Vector2.Transform(vector, matrix ?? Matrix.Identity), 0.0f);
                vertices[i].Color = color;
                ++i;
            }
            // put in the final vertex, closing off the polygon
            vertices[i].Position = vertices[0].Position;
            vertices[i].Color = vertices[0].Color;

            return vertices;
        }
    }

    class Ship
    {
        BasicEffect basicEffect;
        GraphicsDevice graphics;
        World world;
        Body ship;
        Random random = new Random(0);

        public Ship(GraphicsDevice graphicsDevice)
        {
            this.graphics = graphicsDevice;

            /*
             * Basic viewport that looks into the scene
             */
            basicEffect = new BasicEffect(graphics);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, graphics.Viewport.Width,
                graphics.Viewport.Height, 0,
                0, 1
            );

            
            // physics world where the simulation happens
            this.world = new World(Vector2.Zero);

            // center of the board
            Point center = graphics.Viewport.TitleSafeArea.Center;

            for (int i = 0; i < 100; i++)
            {
                int mass = random.Next(1, 5);
                Body body = BodyFactory.CreateRectangle(world, 10.0f, 10.0f, mass);
                body.SetTransform(new Vector2(center.X + random.Next(-100, 100), center.Y + random.Next(-100, 100)), 0.5f);
                body.BodyType = BodyType.Dynamic;
                body.ApplyForce(new Vector2(random.Next(-50, 50), 0.0f));
                body.ApplyTorque(random.Next(1, 100));
                int grey = ((5 - mass) * 10) + 50;
                body.UserData = new Color(grey, grey, grey);
            }

            
            this.ship = BodyFactory.CreatePolygon(world, new Vertices(new Vector2[] { new Vector2(0, -10), new Vector2(10, 10), new Vector2(-10, 10) }), 1.0f);
            this.ship.SetTransform(new Vector2(center.X, center.Y), 0.0f);
            this.ship.BodyType = BodyType.Dynamic;
            this.ship.UserData = Color.White;
            //ship.ApplyForce(new Vector2(random.Next(-50, 50), 0.0f));
            //ship.ApplyTorque(1.0f);
        }

        public void Update(GameTime gameTime)
        {
            float timedelta = gameTime.ElapsedGameTime.Milliseconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                this.ship.ApplyAngularImpulse(-10.0f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                this.ship.ApplyAngularImpulse(10.0f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                Vector2 velocityDelta = new Vector2(
                    (float)Math.Cos(this.ship.Rotation - MathHelper.PiOver2) * 10,
                    (float)Math.Sin(this.ship.Rotation - MathHelper.PiOver2) * 10
                );

                this.ship.ApplyLinearImpulse(velocityDelta);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                CuttingTools.Cut(world, Vector2.Zero, new Vector2(graphics.Viewport.Width, graphics.Viewport.Height), 0.001f);
            }

            // step the simulation forward
            this.world.Step(timedelta / 10.0f);

            foreach (Body body in this.world.BodyList)
            {
                body.SetTransform(VectorExtensions.Modulo(body.Position, new Vector2(graphics.Viewport.Width, graphics.Viewport.Height)), body.Rotation);
            }
        }
        
        public void Draw()
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            foreach (Body body in this.world.BodyList)
            {
                body.Draw(graphics, (Color)(body.UserData ?? Color.Red));
            }
        }
    }
}
