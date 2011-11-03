using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
                Matrix matrix = Matrix.Multiply(
                    Matrix.CreateRotationZ(body.Rotation),
                    Matrix.CreateTranslation(new Vector3(body.Position, 0.0f))
                );

                fixture.Draw(graphics, color, matrix);
            }
        }

        public static Vector2 GetVelocity(this Body body, float force)
        {
            return new Vector2(
                (float)Math.Cos(body.Rotation - MathHelper.PiOver2) * force,
                (float)Math.Sin(body.Rotation - MathHelper.PiOver2) * force
            );
        }
    }

    public static class FixtureExtensions
    {
        public static void Draw(this Fixture fixture, GraphicsDevice graphics, Color color, Matrix? matrix = null)
        {
            VertexPositionColor[] vertices;

            switch (fixture.ShapeType)
            {
                case ShapeType.Polygon:
                {
                    vertices = ((PolygonShape)fixture.Shape).ToVertices(color, matrix);
                }
                break;
                case ShapeType.Circle:
                {
                    CircleShape circle = ((CircleShape)fixture.Shape);

                    vertices = new VertexPositionColor[]
                    {
                        new VertexPositionColor(new Vector3(Vector2.Transform(Vector2.Zero, matrix ?? Matrix.Identity), 0.0f), color),
                        new VertexPositionColor(new Vector3(Vector2.Transform(new Vector2(circle.Radius), matrix ?? Matrix.Identity), 0.0f), color)
                    };
                }
                break;
                default: throw new InvalidOperationException(String.Format("Unable to render ShapeType {0}", fixture.ShapeType));
            }

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

    public static class VectorExtensions
    {
        public static Vector2 Modulo(Vector2 a, Vector2 b)
        {
            return new Vector2(
                 (float)(a.X - b.X * Math.Floor(a.X / b.X)),
                 (float)(a.Y - b.Y * Math.Floor(a.Y / b.Y))
            );
        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        World world;
        Body ship;
        Random random = new Random(0);
        Vector2 bounds;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.bounds = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            this.basicEffect = new BasicEffect(GraphicsDevice);
            this.basicEffect.VertexColorEnabled = true;
            this.basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, this.basicEffect.GraphicsDevice.Viewport.Width,
                this.basicEffect.GraphicsDevice.Viewport.Height, 0,
                0, 1
            );


            this.world = new World(Vector2.Zero);

            Point center = GraphicsDevice.Viewport.TitleSafeArea.Center;

            for (int i = 0; i < 100; i++)
            {
                int mass = random.Next(1, 5);
                Body body = BodyFactory.CreateRectangle(this.world, 10.0f, 10.0f, mass);
                body.SetTransform(new Vector2(center.X + random.Next(-100, 100), center.Y + random.Next(-100, 100)), 0.5f);
                body.BodyType = BodyType.Dynamic;
                body.ApplyForce(new Vector2(this.random.Next(-50, 50), 0.0f));
                body.ApplyTorque(this.random.Next(1, 100));
                int grey = ((5 - mass) * 10) + 50;
                body.UserData = new Color(grey, grey, grey);
            }

            for (int i = 0; i < 1000; i++)
            {
                Body body = BodyFactory.CreateCircle(this.world, 1.0f, 0.25f);
                body.BodyType = BodyType.Dynamic;
                body.SetTransform(new Vector2(this.random.Next(0, GraphicsDevice.Viewport.TitleSafeArea.Right), this.random.Next(0, GraphicsDevice.Viewport.TitleSafeArea.Bottom)), 0.0f);
                body.ApplyTorque(1.0f);
                body.UserData = Color.CornflowerBlue * Math.Min((float)this.random.NextDouble() + 0.2f, 1.0f);
            }

            this.ship = BodyFactory.CreatePolygon(this.world, new Vertices(new Vector2[] { new Vector2(0, -10), new Vector2(10, 10), new Vector2(-10, 10) }), 1.0f);
            this.ship.SetTransform(new Vector2(center.X, center.Y), 0.0f);
            this.ship.BodyType = BodyType.Dynamic;
            this.ship.UserData = Color.White;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            int timedelta = gameTime.ElapsedGameTime.Milliseconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                this.ship.ApplyAngularImpulse(-10.0f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                this.ship.ApplyAngularImpulse(10.0f);
            }

            this.ship.ApplyAngularImpulse(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X * 10.0f);

            float trigger = GamePad.GetState(PlayerIndex.One).Triggers.Right;

            if (trigger > 0)
            {
                this.ship.ApplyLinearImpulse(this.ship.GetVelocity(trigger * 10.0f));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                this.ship.ApplyLinearImpulse(this.ship.GetVelocity(10.0f));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                CuttingTools.Cut(world, Vector2.Zero, bounds, 0.001f);
            }

            this.world.Step(timedelta / 10.0f);

            foreach (Body body in this.world.BodyList)
            {
                Vector2 wrappedPosition = VectorExtensions.Modulo(body.Position, bounds);

                body.SetTransformIgnoreContacts(ref wrappedPosition, body.Rotation);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            foreach (Body body in this.world.BodyList)
            {
                body.Draw(GraphicsDevice, (Color)(body.UserData ?? Color.Red));
            }
        }
    }
}
