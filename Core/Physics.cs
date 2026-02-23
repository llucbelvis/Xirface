using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;


namespace Xirface
{
    public class Physics
    {
        public enum BodyType
        {
            None,
            Sphere,
            Square,
            Complex
        }

        public static Root Check(HashSet<Root> roots, Vector2 point)
        {

            foreach (Root root in roots)
            {
                Root collision = Hierarchy(root, point);
                if (collision != null)
                    return collision;
            }

            return null;
        }

        public static Root Hierarchy(Root root, Vector2 point)
        {
            Vector2 local = point - root.Absolute();

            if (root.Mesh?.BodyType == BodyType.Complex) {
                if (Polygon(local, root.Mesh.Body, 1))
                    return Children(root, point);
            }
            else
            {
                if (Square(local, root.Size))
                    return Children(root, point);
            }

            return null;
        }

        private static Root Children(Root root, Vector2 point)
        {
            foreach (Root child in root.Children)
            {
                if (Hierarchy(child, point) is Root collision)
                    return collision;
            }   

            return root;
        }

		public static bool Sphere(Vector3 p1, Vector3 p2, float rad) {

            return (rad > Vector3.Distance(p1, p2));
        }



        public static bool Square(Vector2 pos, Vector2 size)
        {
            return pos.X >= 0 && pos.X <= size.X && pos.Y >= 0 && pos.Y <= size.Y;
        }

        public static bool Polygon(Vector2 point, Vector2[] polygon, float zoom)
        {
            bool inside = false;

            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 curr = polygon[i] * zoom;
                Vector2 next = polygon[(i + 1) % polygon.Length] * zoom;

                bool hasCrossed = (curr.Y >= point.Y) != (next.Y >= point.Y);
                bool isRight = point.X < (next.X - curr.X) * (point.Y - curr.Y) / (next.Y - curr.Y) + curr.X;

                if (hasCrossed && isRight)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}