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
        
        public static void PointOnLine(Track Track, Vector2 point,Vector2[] line, Vector2 origin, HashSet<(Vector2 mousePosition, Track Track)> snaps)
        {

            Vector2 p1p2 = (origin + line[0]) - (origin + line[line.Length - 1]), p1, p2;

            for (int i = 0; i < (line.Length - 1); i++)
            {
                if (System.Math.Abs(p1p2.X) == System.Math.Abs(p1p2.Y))
                {
                    p1 = origin + new Vector2(line[i].X, line[i].Y);
                    p2 = origin + new Vector2(line[i + 1].X, line[i + 1].Y);
                }
                else if (Math.Abs(p1p2.X) < Math.Abs(p1p2.Y))
                {

                    p1 = origin + new Vector2(0, line[i].Y);
                    p2 = origin + new Vector2(0, line[i + 1].Y);
                }
                else
                {

                    p1 = origin + new Vector2(line[i].X, 0);
                    p2 = origin + new Vector2(line[i + 1].X, 0);
                }

                Vector2 p2p1 = p2 - p1;
                Vector2 mp1 = point - p1;

                double proj = (mp1.X) * (p2p1.X) + ((mp1.Y) * (p2p1.Y));
                double p2p1sq = Math.Pow((p2 - p1).X, 2) + Math.Pow((p2 - p1).Y, 2);

                double d = (proj / p2p1sq);

                if (d > 0 && d < 1)
                {
                    Vector2 final = origin + (line[i] + (line[i + 1] - line[i]) * (float)d);

                    snaps.Add((final, Track));
                }
            }

            return;
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