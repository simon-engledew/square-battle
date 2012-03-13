using System;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space.Extensions;

namespace Space
{
    public class SpaceGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        World world;
        Body ship;
        Random random = new Random(0);
        Vector2 bounds;

        public SpaceGame()
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
                body.CollisionCategories = Category.Cat1;
            }

            for (int i = 0; i < 1000; i++)
            {
                Body body = BodyFactory.CreateCircle(this.world, 1.0f, 0.25f);
                body.BodyType = BodyType.Dynamic;
                body.SetTransform(new Vector2(this.random.Next(0, GraphicsDevice.Viewport.TitleSafeArea.Right), this.random.Next(0, GraphicsDevice.Viewport.TitleSafeArea.Bottom)), 0.0f);
                body.ApplyTorque(1.0f);
                body.UserData = Color.CornflowerBlue * Math.Min((float)this.random.NextDouble() + 0.2f, 1.0f);
                body.CollidesWith = Category.Cat1;
                body.CollisionCategories = Category.Cat2;
            }

            this.ship = BodyFactory.CreatePolygon(this.world, new Vertices(new Vector2[] { new Vector2(0, -10), new Vector2(10, 10), new Vector2(-10, 10) }), 1.0f);
            this.ship.SetTransform(new Vector2(center.X, center.Y), 0.0f);
            this.ship.BodyType = BodyType.Dynamic;
            this.ship.UserData = Color.White;
            this.ship.CollisionCategories = Category.Cat1;
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
