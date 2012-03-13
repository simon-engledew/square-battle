using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Collision.Shapes;

namespace Space.Extensions
{
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
}
