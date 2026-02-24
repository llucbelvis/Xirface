using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;



namespace Xirface
{
    class MathF{

        public static float Sign(float value)
        {
            return value == 0 ? 1 : Math.Sign(value);
        }

        public static Vector2 LookAt(Vector2 p1, Vector2 p2, float width)
        {
            Vector2 p1p2 = p1 - p2;

            if (float.IsNaN(p1p2.X))
                return Vector2.Zero;

            float p1p2x = Math.Sign(p1p2.X + 0.01f);

            return new Vector2(p1p2.Y * p1p2x, -p1p2.X * p1p2x) / Vector2.Distance(p1, p2) * width * p1p2x;
        }

        public static Vector2 ToOrigin(Vector2 position, Vector2 origin)
        {
            return Vector2.Transform(position, Matrix4x4.CreateTranslation(new Vector3(origin,0)));

        }
        public static Vector2 ToGrid(Vector2 position)
        {
            return position;
        }
    }
} 