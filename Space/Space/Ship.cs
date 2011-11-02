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

namespace Space
{
    class Ship
    {
        BasicEffect basicEffect;
        GraphicsDevice graphics;
        World world;

        Body blue;
        Body red;

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

            // RED, a rectangle with a heart of gold
            this.red = BodyFactory.CreateRectangle(world, 10.0f, 10.0f, 1.0f);
            // in the middle, a little bit of rotation
            this.red.SetTransform(new Vector2(center.X + 100.0f, center.Y), 0.5f);
            // turn on the simulation for this body. off by default to save CPU power
            this.red.BodyType = BodyType.Dynamic;
            // push red - he hates to be pushed!
            this.red.ApplyForce(new Vector2(-50.0f, 0.0f));
            // torque him
            this.red.ApplyTorque(100.0f);

            // blue, a down and out square, seen better days
            this.blue = BodyFactory.CreateRectangle(world, 10.0f, 10.0f, 1.0f);
            // set him up across from red
            this.blue.SetTransform(new Vector2(center.X - 100.0f, center.Y), 0.0f);
            this.blue.BodyType = BodyType.Dynamic;
            this.blue.ApplyForce(new Vector2(10.0f, 0.0f));
            this.blue.ApplyTorque(100.0f);
        }

        public void Update(GameTime gameTime)
        {
            float timedelta = gameTime.ElapsedGameTime.Milliseconds;

            // step the simulation forward
            this.world.Step(timedelta / 10.0f);
        }

        public void DrawBody(Body body, Color color)
        {
            // shift the current context into place to stamp down the little critter
            basicEffect.World = Matrix.Multiply(Matrix.CreateRotationZ(body.Rotation), Matrix.CreateTranslation(new Vector3(body.Position, 0.0f)));
            basicEffect.CurrentTechnique.Passes[0].Apply();

            // go through every fixture in this body (only one atm: the square!)
            foreach (Fixture fixture in body.FixtureList)
            {
                /*
                 * cast the shape into a special case of the Shape object -> PolygonShape. PolygonShape knows all about Vertices, which we need to draw. There
                 * are a whole host of other shape types that we could draw in different ways
                 */
                PolygonShape shape = (PolygonShape)fixture.Shape;

                // create a new vertex array to hold our drawing information (one bigger than the number of Vertices to loop back round and close the rectangle
                VertexPositionColor[] vertices = new VertexPositionColor[shape.Vertices.Count + 1];

                // loop through the vertices, saving each one into an array slot
                int i = 0;
                foreach (Vector2 vector in shape.Vertices)
                {
                    vertices[i].Position = new Vector3(vector, 0.0f);
                    vertices[i].Color = color;
                    ++i;
                }
                // put in the final vertex, closing off the rectangle
                vertices[i].Position = new Vector3(shape.Vertices[0], 0.0f);
                vertices[i].Color = color;

                // draw the rectangle to the screen
                graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertices, 0, 4);
            }
        }

        public void Draw()
        {
            // draw RED
            DrawBody(red, Color.Red);
            // draw BLUE
            DrawBody(blue, Color.Blue);
            // fight!
        }
    }
}
