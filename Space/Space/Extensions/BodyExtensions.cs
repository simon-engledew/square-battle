using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Space.Extensions
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
}
