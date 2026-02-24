using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Xirface
{
    public partial class Mesh<TVertex> where TVertex : unmanaged, IVertex
    {
        public static Mesh<VertexPositionColor> Sphere(float rad, int iterations, Color color)
        {
            Mesh<VertexPositionColor> Mesh = new();
;
            Mesh.Indices = new uint[iterations * 6];
            Mesh.Vertices = new VertexPositionColor[iterations * 6];

            for (float t = 0; t < iterations; t += 1f)
            {

                Mesh.Vertices[(int)t * 6] = new((new Vector3((float)Math.Cos((t / iterations) * Math.PI) * rad, (float)Math.Sin((t / iterations) * Math.PI) * rad, 0)), color);
                Mesh.Vertices[(int)t * 6 + 1] = new((new Vector3((float)Math.Cos(((t + 1f) / iterations) * Math.PI) * rad, (float)Math.Sin(((t + 1f) / iterations) * Math.PI) * rad, 0)), color);
                Mesh.Vertices[(int)t * 6 + 2] = new(Vector3.Zero, color);

                Mesh.Vertices[(int)t * 6 + 3] = new((-new Vector3((float)Math.Cos((t / iterations) * Math.PI) * rad, (float)Math.Sin((t / iterations) * Math.PI) * rad, 0)), color);
                Mesh.Vertices[(int)t * 6 + 4] = new((-new Vector3((float)Math.Cos(((t + 1f) / iterations) * Math.PI) * rad, (float)Math.Sin(((t + 1f) / iterations) * Math.PI) * rad, 0)), color);
                Mesh.Vertices[(int)t * 6 + 5] = new(Vector3.Zero, color);

                Mesh.Indices[(int)t * 6] = (uint)(t * 6);
                Mesh.Indices[(int)t * 6 + 1] = (uint)(t * 6 + 1);
                Mesh.Indices[(int)t * 6 + 2] = (uint)(t * 6 + 2);

                Mesh.Indices[(int)t * 6 + 3] = (uint)(t * 6 + 3);
                Mesh.Indices[(int)t * 6 + 4] = (uint)(t * 6 + 4);
                Mesh.Indices[(int)t * 6 + 5] = (uint)(t * 6 + 5);
            }

            Mesh.Dirty();

            return Mesh;
        }
    }
}