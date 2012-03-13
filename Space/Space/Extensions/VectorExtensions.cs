using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Space.Extensions
{
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
}
