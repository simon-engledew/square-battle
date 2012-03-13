using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;

namespace Space.Extensions
{
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
}
