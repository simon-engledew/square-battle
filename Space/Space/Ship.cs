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
        List<Body> bodies = new List<Body>();
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
                Body body = BodyFactory.CreateRectangle(world, 10.0f, 10.0f, random.Next(1, 5));
                body.SetTransform(new Vector2(center.X + random.Next(-100, 100), center.Y + random.Next(-100, 100)), 0.5f);
                body.BodyType = BodyType.Dynamic;
                body.ApplyForce(new Vector2(random.Next(-50, 50), 0.0f));
                body.ApplyTorque(random.Next(1, 100));
                this.bodies.Add(body);
            }

            
            Body ship = BodyFactory.CreatePolygon(world, new Vertices(new Vector2[] { new Vector2(0, -10), new Vector2(10, 10), new Vector2(-10, 10) }), 1.0f);
            ship.SetTransform(new Vector2(center.X, center.Y), 0.0f);
            ship.BodyType = BodyType.Dynamic;

            this.bodies.Add(ship);
            //ship.ApplyForce(new Vector2(random.Next(-50, 50), 0.0f));
            //ship.ApplyTorque(1.0f);
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
                graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertices, 0, shape.Vertices.Count);
            }
        }

        public void Draw()
        {
            for (int i = 0; i < this.bodies.Count; i++)
            {
                DrawBody(this.bodies[i], i % 2 == 0 ? Color.Red : Color.Green);
            }
        }
    }
}
