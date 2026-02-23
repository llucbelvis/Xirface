using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Numerics;
using System.Runtime.CompilerServices;


namespace Xirface
{

    public partial class Mesh<TVertex> where TVertex : unmanaged, IVertex
    {
        public static Mesh<VertexPositionColorTexture> Square(Vector2 size, Color color, Texture2D texture)
        {
            Mesh<VertexPositionColorTexture> Mesh = new();

            Mesh.BodyType = Physics.BodyType.Square;
            Mesh.Indices = [0, 1, 2, 1, 3, 2];

            Mesh.Vertices = [
                new(new Vector3(0,0,0), color , new Vector2(0, 1)),
                new(new Vector3(0,texture.Height * size.Y,0), color ,new Vector2(0,0)),
                new(new Vector3(texture.Width * size.X, 0,0), color  ,new Vector2(1, 1)),
                new(new Vector3(texture.Width * size.X, texture.Height * size.Y,0), color,new Vector2(1, 0)),
            ];

            Mesh.Body = [
                new Vector2(0, 0),
                new Vector2(0, size.Y),
                new Vector2(size.X, size.Y),
                new Vector2(size.X, 0)
            ];

            Mesh.Dirty();

            return Mesh;
        }

    }


    public partial class Mesh<TVertex> where TVertex : unmanaged, IVertex
    {

        struct CornerPoints
        {
            public Vector2 Outer, Inner, Mid, OuterA, OuterB, InnerA, InnerB;
            public Vector2 OuterMidA, OuterMidB, InnerMidA, InnerMidB;

            public Vector2[] OuterCurve;
            public Vector2[] InnerCurve;
        }
        
        public static Mesh<VertexPositionColor> Square(Vector2 size,  Color fillColor)
        {
            Mesh<VertexPositionColor> Mesh = new();

            Mesh.BodyType = Physics.BodyType.Square;
            Mesh.Indices = [0,1,2,0,2,3];

            Mesh.Vertices = [
                new VertexPositionColor(new Vector3(0,0,0), fillColor),
                new VertexPositionColor(new Vector3(0,size.Y,0), fillColor),
                new VertexPositionColor(new Vector3(size.X,size.Y,0), fillColor),
                new VertexPositionColor(new Vector3(size.X,0,0), fillColor)
            ];

            Mesh.Body = [
                new Vector2(0, 0),
                new Vector2(0, size.Y),
                new Vector2(size.X, size.Y),
                new Vector2(size.X, 0)
            ];

            Mesh.Dirty();

            return Mesh;
        }

        public static Mesh<VertexPositionColor> Square(Vector2 size, Color fillColor, float curve, float stroke, Color strokeColor, int iterations)
        {
            Mesh<VertexPositionColor> Mesh = new();

            Mesh.Indices = new uint[(iterations + 1) * 24 + 30 + 900];
            Mesh.Vertices = new VertexPositionColor[(iterations + 1) * 16 + 20 + 900];

            Mesh.BodyType = Physics.BodyType.Complex;

            CornerPoints bottomLeft = new();
            CornerPoints topLeft = new();
            CornerPoints topRight = new();
            CornerPoints bottomRight = new();

            size.X = Math.Abs(size.X) * MathF.Sign(size.X);
            size.Y = Math.Abs(size.Y) * MathF.Sign(size.Y);

            bottomLeft.Outer = new Vector2(-curve * MathF.Sign(size.X), -curve * MathF.Sign(size.Y));
            bottomLeft.Inner = bottomLeft.Outer + new Vector2(curve * MathF.Sign(size.X), curve * MathF.Sign(size.Y));

            bottomLeft.OuterA = new Vector2(bottomLeft.Inner.X, bottomLeft.Outer.Y);
            bottomLeft.OuterB = new Vector2(bottomLeft.Outer.X, bottomLeft.Inner.Y);

            bottomLeft.InnerA = Vector2.Lerp(bottomLeft.OuterA, bottomLeft.Inner, stroke);
            bottomLeft.InnerB = Vector2.Lerp(bottomLeft.OuterB, bottomLeft.Inner, stroke);

            bottomLeft.Mid = Vector2.Lerp(bottomLeft.Outer, bottomLeft.Inner, stroke);

            bottomLeft.OuterMidA = Vector2.Lerp(bottomLeft.OuterA, bottomLeft.Outer, 0.5522847498f);
            bottomLeft.OuterMidB = Vector2.Lerp(bottomLeft.OuterB, bottomLeft.Outer, 0.5522847498f);

            bottomLeft.InnerMidA = Vector2.Lerp(bottomLeft.InnerA, bottomLeft.Mid, 0.5522847498f);
            bottomLeft.InnerMidB = Vector2.Lerp(bottomLeft.InnerB, bottomLeft.Mid, 0.5522847498f);

            topLeft.Outer = new Vector2(bottomLeft.Outer.X, size.Y + curve * MathF.Sign(size.Y));
            topLeft.Inner = topLeft.Outer + new Vector2(curve * MathF.Sign(size.X), -curve * MathF.Sign(size.Y));

            topLeft.OuterA = new Vector2(topLeft.Inner.X, topLeft.Outer.Y);
            topLeft.OuterB = new Vector2(topLeft.Outer.X, topLeft.Inner.Y);

            topLeft.InnerA = Vector2.Lerp(topLeft.OuterA, topLeft.Inner, stroke);
            topLeft.InnerB = Vector2.Lerp(topLeft.OuterB, topLeft.Inner, stroke);

            topLeft.Mid = Vector2.Lerp(topLeft.Outer, topLeft.Inner, stroke);

            topLeft.OuterMidA = Vector2.Lerp(topLeft.OuterA, topLeft.Outer, 0.5522847498f);
            topLeft.OuterMidB = Vector2.Lerp(topLeft.OuterB, topLeft.Outer, 0.5522847498f);

            topLeft.InnerMidA = Vector2.Lerp(topLeft.InnerA, topLeft.Mid, 0.5522847498f);
            topLeft.InnerMidB = Vector2.Lerp(topLeft.InnerB, topLeft.Mid, 0.5522847498f);

            topRight.Outer = new Vector2(size.X + curve * MathF.Sign(size.X), size.Y + curve * MathF.Sign(size.Y));
            topRight.Inner = topRight.Outer + new Vector2(-curve * MathF.Sign(size.X), -curve * MathF.Sign(size.Y));

            topRight.OuterA = new Vector2(topRight.Inner.X, topRight.Outer.Y);
            topRight.OuterB = new Vector2(topRight.Outer.X, topRight.Inner.Y);

            topRight.InnerA = Vector2.Lerp(topRight.OuterA, topRight.Inner, stroke);
            topRight.InnerB = Vector2.Lerp(topRight.OuterB, topRight.Inner, stroke);

            topRight.Mid = Vector2.Lerp(topRight.Outer, topRight.Inner, stroke);

            topRight.OuterMidA = Vector2.Lerp(topRight.OuterA, topRight.Outer, 0.5522847498f);
            topRight.OuterMidB = Vector2.Lerp(topRight.OuterB, topRight.Outer, 0.5522847498f);

            topRight.InnerMidA = Vector2.Lerp(topRight.InnerA, topRight.Mid, 0.5522847498f);
            topRight.InnerMidB = Vector2.Lerp(topRight.InnerB, topRight.Mid, 0.5522847498f);

            bottomRight.Outer = new Vector2(size.X + curve * MathF.Sign(size.X), bottomLeft.Outer.Y);
            bottomRight.Inner = bottomRight.Outer + new Vector2(-curve * MathF.Sign(size.X), curve * MathF.Sign(size.Y));

            bottomRight.OuterA = new Vector2(bottomRight.Inner.X, bottomRight.Outer.Y);
            bottomRight.OuterB = new Vector2(bottomRight.Outer.X, bottomRight.Inner.Y);

            bottomRight.InnerA = Vector2.Lerp(bottomRight.OuterA, bottomRight.Inner, stroke);
            bottomRight.InnerB = Vector2.Lerp(bottomRight.OuterB, bottomRight.Inner, stroke);

            bottomRight.Mid = Vector2.Lerp(bottomRight.Outer, bottomRight.Inner, stroke);

            bottomRight.OuterMidA = Vector2.Lerp(bottomRight.OuterA, bottomRight.Outer, 0.5522847498f);
            bottomRight.OuterMidB = Vector2.Lerp(bottomRight.OuterB, bottomRight.Outer, 0.5522847498f);

            bottomRight.InnerMidA = Vector2.Lerp(bottomRight.InnerA, bottomRight.Mid, 0.5522847498f);
            bottomRight.InnerMidB = Vector2.Lerp(bottomRight.InnerB, bottomRight.Mid, 0.5522847498f);

            bottomLeft.OuterCurve = new Vector2[iterations + 1];
            bottomLeft.InnerCurve = new Vector2[iterations + 1];

            topLeft.OuterCurve = new Vector2[iterations + 1];
            topLeft.InnerCurve = new Vector2[iterations + 1];

            topRight.OuterCurve = new Vector2[iterations + 1];
            topRight.InnerCurve = new Vector2[iterations + 1];

            bottomRight.OuterCurve = new Vector2[iterations + 1];
            bottomRight.InnerCurve = new Vector2[iterations + 1];

            for (float i = 0f; i < iterations + 1; i++)
            {
                var t = i / iterations;

                bottomLeft.OuterCurve[(int)i] = (float)Math.Pow((1 - t), 3) * bottomLeft.OuterA + 3 * (float)Math.Pow((1 - t), 2) * t * bottomLeft.OuterMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * bottomLeft.OuterMidB + (float)Math.Pow(t, 3f) * bottomLeft.OuterB;
                bottomLeft.InnerCurve[(int)i] = (float)Math.Pow((1 - t), 3) * bottomLeft.InnerA + 3 * (float)Math.Pow((1 - t), 2) * t * bottomLeft.InnerMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * bottomLeft.InnerMidB + (float)Math.Pow(t, 3f) * bottomLeft.InnerB;

                topLeft.OuterCurve[(int)i] = (float)Math.Pow((1 - t), 3) * topLeft.OuterA + 3 * (float)Math.Pow((1 - t), 2) * t * topLeft.OuterMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * topLeft.OuterMidB + (float)Math.Pow(t, 3f) * topLeft.OuterB;
                topLeft.InnerCurve[(int)i] = (float)Math.Pow((1 - t), 3) * topLeft.InnerA + 3 * (float)Math.Pow((1 - t), 2) * t * topLeft.InnerMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * topLeft.InnerMidB + (float)Math.Pow(t, 3f) * topLeft.InnerB;

                topRight.OuterCurve[(int)i] = (float)Math.Pow((1 - t), 3) * topRight.OuterA + 3 * (float)Math.Pow((1 - t), 2) * t * topRight.OuterMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * topRight.OuterMidB + (float)Math.Pow(t, 3f) * topRight.OuterB;
                topRight.InnerCurve[(int)i] = (float)Math.Pow((1 - t), 3) * topRight.InnerA + 3 * (float)Math.Pow((1 - t), 2) * t * topRight.InnerMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * topRight.InnerMidB + (float)Math.Pow(t, 3f) * topRight.InnerB;

                bottomRight.OuterCurve[(int)i] = (float)Math.Pow((1 - t), 3) * bottomRight.OuterA + 3 * (float)Math.Pow((1 - t), 2) * t * bottomRight.OuterMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * bottomRight.OuterMidB + (float)Math.Pow(t, 3f) * bottomRight.OuterB;
                bottomRight.InnerCurve[(int)i] = (float)Math.Pow((1 - t), 3) * bottomRight.InnerA + 3 * (float)Math.Pow((1 - t), 2) * t * bottomRight.InnerMidA + 3 * (1 - t) * (float)Math.Pow(t, 2) * bottomRight.InnerMidB + (float)Math.Pow(t, 3f) * bottomRight.InnerB;
            }

            for (int i = 0; i < iterations; i++)
            {
                Mesh.Vertices[i * 16 + 0] = new(new Vector3(bottomLeft.OuterCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 1] = new(new Vector3(bottomLeft.OuterCurve[i + 1], 0), fillColor);
                Mesh.Vertices[i * 16 + 2] = new(new Vector3(bottomLeft.InnerCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 3] = new(new Vector3(bottomLeft.InnerCurve[i + 1], 0), fillColor);

                Mesh.Vertices[i * 16 + 4] = new(new Vector3(topLeft.OuterCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 5] = new(new Vector3(topLeft.OuterCurve[i + 1], 0), fillColor);
                Mesh.Vertices[i * 16 + 6] = new(new Vector3(topLeft.InnerCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 7] = new(new Vector3(topLeft.InnerCurve[i + 1], 0), fillColor);

                Mesh.Vertices[i * 16 + 8] = new(new Vector3(topRight.OuterCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 9] = new(new Vector3(topRight.OuterCurve[i + 1], 0), fillColor);
                Mesh.Vertices[i * 16 + 10] = new(new Vector3(topRight.InnerCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 11] = new(new Vector3(topRight.InnerCurve[i + 1], 0), fillColor);

                Mesh.Vertices[i * 16 + 12] = new(new Vector3(bottomRight.OuterCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 13] = new(new Vector3(bottomRight.OuterCurve[i + 1], 0), fillColor);
                Mesh.Vertices[i * 16 + 14] = new(new Vector3(bottomRight.InnerCurve[i], 0), fillColor);
                Mesh.Vertices[i * 16 + 15] = new(new Vector3(bottomRight.InnerCurve[i + 1], 0), fillColor);


                Mesh.Indices[i * 24 + 0] = (uint)(i * 16 + 0);
                Mesh.Indices[i * 24 + 1] = (uint)(i * 16 + 1);
                Mesh.Indices[i * 24 + 2] = (uint)(i * 16 + 2);
                Mesh.Indices[i * 24 + 3] = (uint)(i * 16 + 1);
                Mesh.Indices[i * 24 + 4] = (uint)(i * 16 + 3);
                Mesh.Indices[i * 24 + 5] = (uint)(i * 16 + 2);

                Mesh.Indices[i * 24 + 6] = (uint)(i * 16 + 4);
                Mesh.Indices[i * 24 + 7] = (uint)(i * 16 + 6);
                Mesh.Indices[i * 24 + 8] = (uint)(i * 16 + 5);
                Mesh.Indices[i * 24 + 9] = (uint)(i * 16 + 5);
                Mesh.Indices[i * 24 + 10] = (uint)(i * 16 + 6);
                Mesh.Indices[i * 24 + 11] = (uint)(i * 16 + 7);

                Mesh.Indices[i * 24 + 12] = (uint)(i * 16 + 8);
                Mesh.Indices[i * 24 + 13] = (uint)(i * 16 + 9);
                Mesh.Indices[i * 24 + 14] = (uint)(i * 16 + 10);
                Mesh.Indices[i * 24 + 15] = (uint)(i * 16 + 9);
                Mesh.Indices[i * 24 + 16] = (uint)(i * 16 + 11);
                Mesh.Indices[i * 24 + 17] = (uint)(i * 16 + 10);

                Mesh.Indices[i * 24 + 18] = (uint)(i * 16 + 12);
                Mesh.Indices[i * 24 + 19] = (uint)(i * 16 + 14);
                Mesh.Indices[i * 24 + 20] = (uint)(i * 16 + 13);
                Mesh.Indices[i * 24 + 21] = (uint)(i * 16 + 13);
                Mesh.Indices[i * 24 + 22] = (uint)(i * 16 + 14);
                Mesh.Indices[i * 24 + 23] = (uint)(i * 16 + 15);
            }

            Mesh.Vertices[(iterations + 1) * 16 + 0] = new VertexPositionColor(new Vector3(bottomLeft.OuterB, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 1] = new VertexPositionColor(new Vector3(topLeft.OuterB, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 2] = new VertexPositionColor(new Vector3(topLeft.InnerB, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 3] = new VertexPositionColor(new Vector3(bottomLeft.InnerB, 0), fillColor);

            Mesh.Vertices[(iterations + 1) * 16 + 4] = new VertexPositionColor(new Vector3(topLeft.InnerA, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 5] = new VertexPositionColor(new Vector3(topLeft.OuterA, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 6] = new VertexPositionColor(new Vector3(topRight.OuterA, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 7] = new VertexPositionColor(new Vector3(topRight.InnerA, 0), fillColor);

            Mesh.Vertices[(iterations + 1) * 16 + 8] = new VertexPositionColor(new Vector3(bottomRight.InnerB, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 9] = new VertexPositionColor(new Vector3(topRight.InnerB, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 10] = new VertexPositionColor(new Vector3(topRight.OuterB, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 11] = new VertexPositionColor(new Vector3(bottomRight.OuterB, 0), fillColor);

            Mesh.Vertices[(iterations + 1) * 16 + 12] = new VertexPositionColor(new Vector3(bottomLeft.OuterA, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 13] = new VertexPositionColor(new Vector3(bottomLeft.InnerA, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 14] = new VertexPositionColor(new Vector3(bottomRight.InnerA, 0), fillColor);
            Mesh.Vertices[(iterations + 1) * 16 + 15] = new VertexPositionColor(new Vector3(bottomRight.OuterA, 0), fillColor);

            Mesh.Indices[(iterations + 1) * 24 + 0] = (uint)((iterations + 1) * 16 + 0);
            Mesh.Indices[(iterations + 1) * 24 + 1] = (uint)((iterations + 1) * 16 + 1);
            Mesh.Indices[(iterations + 1) * 24 + 2] = (uint)((iterations + 1) * 16 + 2);
            Mesh.Indices[(iterations + 1) * 24 + 3] = (uint)((iterations + 1) * 16 + 2);
            Mesh.Indices[(iterations + 1) * 24 + 4] = (uint)((iterations + 1) * 16 + 3);
            Mesh.Indices[(iterations + 1) * 24 + 5] = (uint)((iterations + 1) * 16 + 0);

            Mesh.Indices[(iterations + 1) * 24 + 6] = (uint)((iterations + 1) * 16 + 4);
            Mesh.Indices[(iterations + 1) * 24 + 7] = (uint)((iterations + 1) * 16 + 5);
            Mesh.Indices[(iterations + 1) * 24 + 8] = (uint)((iterations + 1) * 16 + 6);
            Mesh.Indices[(iterations + 1) * 24 + 9] = (uint)((iterations + 1) * 16 + 6);
            Mesh.Indices[(iterations + 1) * 24 + 10] = (uint)((iterations + 1) * 16 + 7);
            Mesh.Indices[(iterations + 1) * 24 + 11] = (uint)((iterations + 1) * 16 + 4);

            Mesh.Indices[(iterations + 1) * 24 + 12] = (uint)((iterations + 1) * 16 + 8);
            Mesh.Indices[(iterations + 1) * 24 + 13] = (uint)((iterations + 1) * 16 + 9);
            Mesh.Indices[(iterations + 1) * 24 + 14] = (uint)((iterations + 1) * 16 + 10);
            Mesh.Indices[(iterations + 1) * 24 + 15] = (uint)((iterations + 1) * 16 + 10);
            Mesh.Indices[(iterations + 1) * 24 + 16] = (uint)((iterations + 1) * 16 + 11);
            Mesh.Indices[(iterations + 1) * 24 + 17] = (uint)((iterations + 1) * 16 + 8);

            Mesh.Indices[(iterations + 1) * 24 + 18] = (uint)((iterations + 1) * 16 + 12);
            Mesh.Indices[(iterations + 1) * 24 + 19] = (uint)((iterations + 1) * 16 + 13);
            Mesh.Indices[(iterations + 1) * 24 + 20] = (uint)((iterations + 1) * 16 + 14);
            Mesh.Indices[(iterations + 1) * 24 + 21] = (uint)((iterations + 1) * 16 + 14);
            Mesh.Indices[(iterations + 1) * 24 + 22] = (uint)((iterations + 1) * 16 + 15);
            Mesh.Indices[(iterations + 1) * 24 + 23] = (uint)((iterations + 1) * 16 + 12);

            int cIndex = (iterations + 1) * 24 + 23 + 1;
            int cVertex = (iterations + 1) * 16 + 15 + 1;

            for (int i = 0; i < iterations; i++)
            {
                Mesh.Vertices[i * 12 + cVertex + 0] = new VertexPositionColor(new Vector3(bottomLeft.InnerCurve[i], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 1] = new VertexPositionColor(new Vector3(bottomLeft.InnerCurve[i + 1], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 2] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

                Mesh.Indices[i * 12 + cIndex + 0] = (uint)(i * 12 + cVertex + 0);
                Mesh.Indices[i * 12 + cIndex + 1] = (uint)(i * 12 + cVertex + 1);
                Mesh.Indices[i * 12 + cIndex + 2] = (uint)(i * 12 + cVertex + 2);

                Mesh.Vertices[i * 12  + cVertex + 3] = new VertexPositionColor(new Vector3(topLeft.InnerCurve[i], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 4] = new VertexPositionColor(new Vector3(topLeft.InnerCurve[i + 1], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 5] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

                Mesh.Indices[i * 12 + cIndex + 3] = (uint)(i * 12 + cVertex + 3);
                Mesh.Indices[i * 12 + cIndex + 4] = (uint)(i * 12 + cVertex + 5);
                Mesh.Indices[i * 12 + cIndex + 5] = (uint)(i * 12 + cVertex + 4);

                Mesh.Vertices[i * 12 + cVertex + 6] = new VertexPositionColor(new Vector3(topRight.InnerCurve[i], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 7] = new VertexPositionColor(new Vector3(topRight.InnerCurve[i + 1], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 8] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

                Mesh.Indices[i * 12 + cIndex + 6] = (uint)(i * 12 + cVertex + 6);
                Mesh.Indices[i * 12 + cIndex + 7] = (uint)(i * 12 + cVertex + 7);
                Mesh.Indices[i * 12 + cIndex + 8] = (uint)(i * 12 + cVertex + 8);

                Mesh.Vertices[i * 12 + cVertex + 9] = new VertexPositionColor(new Vector3(bottomRight.InnerCurve[i], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 10] = new VertexPositionColor(new Vector3(bottomRight.InnerCurve[i + 1], 0), strokeColor);
                Mesh.Vertices[i * 12 + cVertex + 11] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

                Mesh.Indices[i * 12 + cIndex + 9] = (uint)(i * 12 + cVertex + 9);
                Mesh.Indices[i * 12 + cIndex + 10] = (uint)(i * 12 + cVertex + 11);
                Mesh.Indices[i * 12 + cIndex + 11] = (uint)(i * 12 + cVertex + 10);
            }

            cIndex = (iterations + 1) * 12 + cIndex + 11 + 1;
            cVertex = (iterations + 1) * 12 + cVertex + 11 + 1;

            Mesh.Vertices[cVertex + 0] = new VertexPositionColor(new Vector3(bottomLeft.InnerA, 0), strokeColor);
            Mesh.Vertices[cVertex + 1] = new VertexPositionColor(new Vector3(bottomRight.InnerA, 0), strokeColor);
            Mesh.Vertices[cVertex + 2] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

            Mesh.Indices[cIndex + 0] = (uint)(cVertex + 0);
            Mesh.Indices[cIndex + 1] = (uint)(cVertex + 2);
            Mesh.Indices[cIndex + 2] = (uint)(cVertex + 1);

            Mesh.Vertices[cVertex + 3] = new VertexPositionColor(new Vector3(bottomLeft.InnerB, 0), strokeColor);
            Mesh.Vertices[cVertex + 4] = new VertexPositionColor(new Vector3(topLeft.InnerB, 0), strokeColor);
            Mesh.Vertices[cVertex + 5] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

            Mesh.Indices[cIndex + 3] = (uint)(cVertex + 3);
            Mesh.Indices[cIndex + 4] = (uint)(cVertex + 4);
            Mesh.Indices[cIndex + 5] = (uint)(cVertex + 5);

            Mesh.Vertices[cVertex + 6] = new VertexPositionColor(new Vector3(topLeft.InnerA, 0), strokeColor);
            Mesh.Vertices[cVertex + 7] = new VertexPositionColor(new Vector3(topRight.InnerA, 0), strokeColor);
            Mesh.Vertices[cVertex + 8] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

            Mesh.Indices[cIndex + 6] = (uint)(cVertex + 6);
            Mesh.Indices[cIndex + 7] = (uint)(cVertex + 7);
            Mesh.Indices[cIndex + 8] = (uint)(cVertex + 8);

            Mesh.Vertices[cVertex + 9] = new VertexPositionColor(new Vector3(topRight.InnerB, 0), strokeColor);
            Mesh.Vertices[cVertex + 10] = new VertexPositionColor(new Vector3(bottomRight.InnerB, 0), strokeColor);
            Mesh.Vertices[cVertex + 11] = new VertexPositionColor(new Vector3(bottomLeft.Inner, 0), strokeColor);

            Mesh.Indices[cIndex + 9] = (uint)(cVertex + 9);
            Mesh.Indices[cIndex + 10] = (uint)(cVertex + 10);
            Mesh.Indices[cIndex + 11] = (uint)(cVertex + 11);


            Mesh.Body = bottomLeft.OuterCurve.Concat(topLeft.OuterCurve.Reverse()).Concat(topRight.OuterCurve).Concat(bottomRight.OuterCurve.Reverse()).ToArray();

            Mesh.Dirty();

            return Mesh;
        }
    }
}