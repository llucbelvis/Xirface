using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;


namespace Xirface
{
    public class Bezier
    {
        public Vector2 drawc0;
        public Vector2[] drawA;
        public Vector2[] drawB;

        public Vector2 pathc0;
        public Vector2[] path;

        public Vector2 snapc0;
        public Vector2[] snapA;
        public Vector2[] snapB;

        public int index;
        public static Vector2 Quadratic(Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            return (float)Math.Pow((1f - t), 2f) * p1 + ((2f * (1f - t)) * t * p2) + ((float)Math.Pow(t, 2f) * p3);
        }

        public static Vector2 Cubic(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t)
        {
            return (float)Math.Pow((1 - t), 3) * p1 + 3 * (float)Math.Pow((1 - t), 2) * t * p2 + 3 * (1 - t) * (float)Math.Pow(t, 2) * p3 + (float)Math.Pow(t, 3f) * p4;
        }
        private void Set(Vector2 drawc0, Vector2[] drawA, Vector2[] drawB, Vector2 pathc0, Vector2[] pathA, Vector2[] pathB, Vector2 snapc0, Vector2[] snapA, Vector2[] snapB)
        {
            this.drawc0 = drawc0;
            this.drawA = drawA;
            this.drawB = drawB;
            this.pathc0 = pathc0;
            this.path = pathA;
            this.snapc0 = snapc0;
            this.snapA = snapA;
            this.snapB = snapB;
        }

        public static Bezier[] Generate(Vector2 wp1, Vector2 wp2, float curve, float width, int iterations)
        {
            Vector2 wp1p2 = wp1 - wp2;
            float wp1p2x = Math.Sign(wp1p2.X);
            float wp1p2y = Math.Sign(wp1p2.Y);

            Vector2 lp2 = wp2 - wp1;

            Vector2 lp1 = Vector2.Zero;

            (Vector2 c0, Vector2[] A, Vector2[] B) draw;
            (Vector2 c0, Vector2[] A, Vector2[] B) path;
            (Vector2 c0, Vector2[] A, Vector2[] B) snap;

            Bezier[] Beziers;

            Bezier A = new(); Bezier B = new();
            Bezier C = new(); Bezier D = new();
            Bezier E = new(); Bezier F = new();
            Bezier G = new(); Bezier H = new();
            Bezier I = new(); Bezier J = new();
            Bezier K = new(); Bezier M = new();
            Bezier L = new(); Bezier N = new();
            Bezier O = new();


            if (Math.Round(wp1p2.X * wp1p2x * 1000) == Math.Round(wp1p2.Y * wp1p2y * 1000))
            {

                draw = GenerateCD(lp1, lp2, curve, width, iterations);
                path = GenerateCD(lp1, lp2, curve, width / 2, iterations);
                snap = GenerateCD(lp1, lp2, curve, width * 2, iterations);

                C.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateCD(lp2, lp1, curve, width, iterations);
                path = GenerateCD(lp2, lp1, curve, width / 2, iterations);
                snap = GenerateCD(lp2, lp1, curve, width * 2, iterations);

                D.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateG(lp1, lp2, curve, width, iterations);
                path = GenerateG(lp1, lp2, curve, width / 2, iterations);
                snap = GenerateG(lp1, lp2, curve, width * 2, iterations);

                G.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateHI(wp1p2, lp1, lp2, curve, width, iterations);
                path = GenerateHI(wp1p2, lp1, lp2, curve, 0, iterations);
                snap = GenerateHI(wp1p2, lp1, lp2, curve, width * 2, iterations);

                H.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateHI(wp1p2, lp2, lp1, curve, width, iterations);
                path = GenerateHI(wp1p2, lp2, lp1, curve, 0, iterations);
                snap = GenerateHI(wp1p2, lp2, lp1, curve, width * 2, iterations);

                I.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateJK(wp1p2, lp1, lp2, curve, width, iterations);
                path = GenerateJK(wp1p2, lp1, lp2, curve, 0, iterations);
                snap = GenerateJK(wp1p2, lp1, lp2, curve, width * 2, iterations);

                J.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateJK(wp1p2, lp2, lp1, curve, width, iterations);
                path = GenerateJK(wp1p2, lp2, lp1, curve, 0, iterations);
                snap = GenerateJK(wp1p2, lp2, lp1, curve, width * 2, iterations);

                K.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                C.index = 2; D.index = 1; G.index = 0; H.index = 3; I.index = 4; J.index = 5; K.index = 6;

                Beziers = new Bezier[7] { C, D, G, H, I, J, K};

                return Beziers;
            }

            if (Math.Round(wp1p2.X * 1000) == 0f || Math.Round(wp1p2.Y * 1000) == 0f)
            {

                draw = GenerateEF(wp1p2, lp1, lp2, curve, width, iterations);
                path = GenerateEF(wp1p2, lp1, lp2, curve, 0, iterations);
                snap = GenerateEF(wp1p2, lp1, lp2, curve, width * 2, iterations);

                E.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateEF(wp1p2, lp2, lp1, curve, width, iterations);
                path = GenerateEF(wp1p2, lp2, lp1, curve, 0, iterations);
                snap = GenerateEF(wp1p2, lp2, lp1, curve, width * 2, iterations);

                F.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);


                draw = GenerateG(lp1, lp2, curve, width, iterations);
                path = GenerateG(lp1, lp2, curve, 0, iterations);
                snap = GenerateG(lp1, lp2, curve, width * 2, iterations);

                G.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateJK(wp1p2, lp1, lp2, curve, width, iterations);
                path = GenerateJK(wp1p2, lp1, lp2, curve, 0, iterations);
                snap = GenerateJK(wp1p2, lp1, lp2, curve, width * 2, iterations);

                J.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateJK(wp1p2, lp2, lp1, curve, width, iterations);
                path = GenerateJK(wp1p2, lp2, lp1, curve, 0, iterations);
                snap = GenerateJK(wp1p2, lp2, lp1, curve, width * 2, iterations);

                K.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateLM(wp1p2, lp1, lp2, curve, width, iterations);
                path = GenerateLM(wp1p2, lp1, lp2, curve, 0, iterations);
                snap = GenerateLM(wp1p2, lp1, lp2, curve, width * 2, iterations);

                L.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

                draw = GenerateLM(wp1p2, lp2, lp1, curve, width, iterations);
                path = GenerateLM(wp1p2, lp2, lp1, curve, 0, iterations);
                snap = GenerateLM(wp1p2, lp2, lp1, curve, width * 2, iterations);

                M.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);


                E.index = 0; F.index = 1; G.index = 2; J.index = 3; K.index = 4; L.index = 5; M.index = 6;

                Beziers = new Bezier[7] {E, F, G, J, K, L, M};

                return Beziers;
            }

            draw = GenerateAB(wp1p2, lp1, lp2, curve, width, iterations);
            path = GenerateAB(wp1p2, lp1, lp2, curve, 0, iterations);
            snap = GenerateAB(wp1p2, lp1, lp2, curve, width * 2, iterations);

            A.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateAB(wp1p2, lp2, lp1, curve, width, iterations);
            path = GenerateAB(wp1p2, lp2, lp1, curve, 0, iterations);
            snap = GenerateAB(wp1p2, lp2, lp1, curve, width * 2, iterations);

            B.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateCD(lp1, lp2, curve, width, iterations);
            path = GenerateCD(lp1, lp2, curve, 0, iterations);
            snap = GenerateCD(lp1, lp2, curve, width * 2, iterations);

            C.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateCD(lp2, lp1, curve, width, iterations);
            path = GenerateCD(lp2, lp1, curve, 0, iterations);
            snap = GenerateCD(lp2, lp1, curve, width * 2, iterations);

            D.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateEF(wp1p2, lp1, lp2, curve, width, iterations);
            path = GenerateEF(wp1p2, lp1, lp2, curve, 0, iterations);
            snap = GenerateEF(wp1p2, lp1, lp2, curve, width * 2, iterations);

            E.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateEF(wp1p2, lp2, lp1, curve, width, iterations);
            path = GenerateEF(wp1p2, lp2, lp1, curve, 0, iterations);
            snap = GenerateEF(wp1p2, lp2, lp1, curve, width * 2, iterations);

            F.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateHI(wp1p2, lp1, lp2, curve, width, iterations);
            path = GenerateHI(wp1p2, lp1, lp2, curve, 0, iterations);
            snap = GenerateHI(wp1p2, lp1, lp2, curve, width * 2, iterations);

            H.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateHI(wp1p2, lp2, lp1, curve, width, iterations);
            path = GenerateHI(wp1p2, lp2, lp1, curve, 0, iterations);
            snap = GenerateHI(wp1p2, lp2, lp1, curve, width * 2, iterations);

            I.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateJK(wp1p2, lp1, lp2, curve, width, iterations);
            path = GenerateJK(wp1p2, lp1, lp2, curve, 0, iterations);
            snap = GenerateJK(wp1p2, lp1, lp2, curve, width * 2, iterations);

            J.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateJK(wp1p2, lp2, lp1, curve, width, iterations);
            path = GenerateJK(wp1p2, lp2, lp1, curve, 0, iterations);
            snap = GenerateJK(wp1p2, lp2, lp1, curve, width * 2, iterations);

            K.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateLM(wp1p2, lp1, lp2, curve, width, iterations);
            path = GenerateLM(wp1p2, lp1, lp2, curve, 0, iterations);
            snap = GenerateLM(wp1p2, lp1, lp2, curve, width * 2, iterations);

            L.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            draw = GenerateLM(wp1p2, lp2, lp1, curve, width, iterations);
            path = GenerateLM(wp1p2, lp2, lp1, curve, 0, iterations);
            snap = GenerateLM(wp1p2, lp2, lp1, curve, width * 2, iterations);

            M.Set(MathF.ToOrigin(draw.c0, wp1), draw.A, draw.B, MathF.ToOrigin(path.c0, wp1), path.A, path.B, MathF.ToOrigin(snap.c0, wp1), snap.A, snap.B);

            A.index = 0; B.index = 1; C.index = 2; D.index = 3; E.index = 4; F.index = 5; H.index = 6; I.index = 7; J.index = 8; K.index = 9; L.index = 10; M.index = 11;
            Beziers = new Bezier[12] {A, B, C, D, E, F, H, I ,J, K, L, M};

            return Beziers;
        }

        private static (Vector2 c0, Vector2[] A, Vector2[] B) GenerateAB(Vector2 wp1p2, Vector2 lp1, Vector2 lp2, float curve, float width, int iterations)
        {
            Vector2 lp1p2 = lp1 - lp2;
            float lp1p2x = Math.Sign(lp1p2.X);
            float lp1p2y = Math.Sign(lp1p2.Y);

            Vector2 c1, c0a, c1a, c2a;
            Vector2 c2, c0b, c1b, c2b;

            float r0, r1, r2;

            Vector2 c0;
            Vector2[] A = new Vector2[(int)iterations + 2];
            Vector2[] B = new Vector2[(int)iterations + 2];

            if (Math.Abs(wp1p2.X) > Math.Abs(wp1p2.Y))
            {

                c0 = new Vector2(lp1.X - lp1p2.Y * lp1p2y * lp1p2x, lp2.Y);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X - c1ac2a.Y * lp1p2y * lp1p2x, c2a.Y);
                c0b = new Vector2(c1b.X - c1bc2b.Y * lp1p2y * lp1p2x, c2b.Y);
            }
            else
            {

                c0 = new Vector2(lp2.X, lp1.Y - lp1p2.X * lp1p2y * lp1p2x);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c2a.X, c1a.Y - c1ac2a.X * lp1p2y * lp1p2x);
                c0b = new Vector2(c2b.X, c1b.Y - c1bc2b.X * lp1p2y * lp1p2x);
            }

            for (float i = 1f; i < iterations + 1; i++)
            {
                var t = ((i - 1) / (iterations - 1));

                A[(int)i] = (float)Math.Pow((1 - t), 3) * c1a + 3 * (float)Math.Pow((1 - t), 2) * t * c0a + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0a + (float)Math.Pow(t, 3f) * c2a;
                B[(int)i] = (float)Math.Pow((1 - t), 3) * c1b + 3 * (float)Math.Pow((1 - t), 2) * t * c0b + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0b + (float)Math.Pow(t, 3f) * c2b;
            }

            return (c0, A, B);
        }

        private static (Vector2 c0, Vector2[] A, Vector2[] B) GenerateCD(Vector2 lp1, Vector2 lp2, float curve, float width, int iterations)
        {
            Vector2 c1, c0a, c1a, c2a;
            Vector2 c2, c0b, c1b, c2b;

            float r0, r1, r2;

            Vector2 c0;
            Vector2[] A = new Vector2[(int)iterations + 2];
            Vector2[] B = new Vector2[(int)iterations + 2];

            c0 = new Vector2(lp1.X, lp2.Y);
            r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

            r1 = (r0) / Vector2.Distance(c0, lp1); c1 = Vector2.Lerp(c0, lp1, r1);
            r2 = (r0) / Vector2.Distance(c0, lp2); c2 = Vector2.Lerp(c0, lp2, r2);


            c1a = c1 + MathF.LookAt(c1, c0, width);
            c1b = c1 - MathF.LookAt(c1, c0, width);

            c2a = c2 - MathF.LookAt(c2, c0, width);
            c2b = c2 + MathF.LookAt(c2, c0, width);

            A[0] = lp1 + MathF.LookAt(lp1, c0, width);
            B[0] = lp1 - MathF.LookAt(lp1, c0, width);

            A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
            B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

            c0a = new Vector2(c1a.X, c2a.Y);
            c0b = new Vector2(c1b.X, c2b.Y);

            for (float i = 1f; i < iterations + 1; i++)
            {
                var t = (i - 1) / (iterations - 1);

                A[(int)i] = (float)Math.Pow((1f - t), 2f) * c1a + ((2f * (1f - t)) * t * c0a) + ((float)Math.Pow(t, 2f) * c2a);
                B[(int)i] = (float)Math.Pow((1f - t), 2f) * c1b + ((2f * (1f - t)) * t * c0b) + ((float)Math.Pow(t, 2f) * c2b);
            }

            return (c0, A, B);
        }
        private static (Vector2 c0, Vector2[] A, Vector2[] B) GenerateEF(Vector2 wp1p2, Vector2 lp1, Vector2 lp2, float curve, float width, int iterations)
        {
            Vector2 lp1p2 = lp1 - lp2;

            float lp1p2x = MathF.Sign(lp1p2.X);
            float lp1p2y = MathF.Sign(lp1p2.Y);

            Vector2 c1, c0a, c1a, c2a;
            Vector2 c2, c0b, c1b, c2b;

            float r0, r1, r2;

            Vector2 c0;
            Vector2[] A = new Vector2[(int)iterations + 2];
            Vector2[] B = new Vector2[(int)iterations + 2];

            if (Math.Abs(wp1p2.X) > Math.Abs(wp1p2.Y))
            {

                c0 = new Vector2(lp1.X - lp1p2.Y * lp1p2y * lp1p2x, lp2.Y);
                c0 = new Vector2(((lp2 + c0) / 2).X, lp2.Y + ((lp2.X - c0.X) / 2) * lp1p2y * lp1p2x);

                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X - c1ac2a.Y * lp1p2y * lp1p2x, c2a.Y);
                c0a = new Vector2(((c2a + c0a) / 2).X, (c2a.Y + ((c2a.X - c0a.X) / 2) * lp1p2y * lp1p2x));

                c0b = new Vector2(c1b.X - c1bc2b.Y * lp1p2y * lp1p2x, c2b.Y);
                c0b = new Vector2(((c2b + c0b) / 2).X, (c2b.Y + ((c2b.X - c0b.X) / 2) * lp1p2y * lp1p2x));
            }
            else
            {

                c0 = new Vector2(lp2.X, lp1.Y - lp1p2.X * lp1p2y * lp1p2x);
                c0 = new Vector2(lp2.X + ((lp2.Y - c0.Y) / 2) * lp1p2y * lp1p2x, ((lp2 + c0) / 2).Y);

                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c2a.X, c1a.Y - c1ac2a.X * lp1p2y * lp1p2x);
                c0a = new Vector2(c2a.X + ((c2a.Y - c0a.Y) / 2) * lp1p2y * lp1p2x, ((c2a + c0a) / 2).Y);

                c0b = new Vector2(c2b.X, c1b.Y - c1bc2b.X * lp1p2y * lp1p2x);
                c0b = new Vector2(c2b.X + ((c2b.Y - c0b.Y) / 2) * lp1p2y * lp1p2x, ((c2b + c0b) / 2).Y);
            }

            for (float i = 1f; i < iterations + 1; i++)
            {
                var t = ((i - 1) / (iterations - 1));

                A[(int)i] = (float)Math.Pow((1f - t), 2f) * c1a + ((2f * (1f - t)) * t * c0a) + ((float)Math.Pow(t, 2f) * c2a);
                B[(int)i] = (float)Math.Pow((1f - t), 2f) * c1b + ((2f * (1f - t)) * t * c0b) + ((float)Math.Pow(t, 2f) * c2b);
            }

            return (c0, A, B);
        }

        private static (Vector2 c0, Vector2[] A, Vector2[] B) GenerateG(Vector2 lp1, Vector2 lp2, float curve, float width, int iterations)
        {
            Vector2 c0;
            Vector2[] A = new Vector2[2];
            Vector2[] B = new Vector2[2];

            c0 = (lp1 + lp2) / 2;

            A[0] = lp1 + MathF.LookAt(lp1, lp2, width);
            B[0] = lp1 - MathF.LookAt(lp1, lp2, width);

            A[1] = lp2 + MathF.LookAt(lp1, lp2, width);
            B[1] = lp2 - MathF.LookAt(lp1, lp2, width);

            return (c0, A, B);
        }

        private static (Vector2 c0, Vector2[] A, Vector2[] B) GenerateHI(Vector2 wp1p2, Vector2 lp1, Vector2 lp2, float curve, float width, int iterations)
        {

            Vector2 lp1p2 = lp1 - lp2;
            float lp1p2x = MathF.Sign(lp1p2.X);
            float lp1p2y = MathF.Sign(lp1p2.Y);

            Vector2 c1, c0a, c1a, c2a, c0a1, c0a2;
            Vector2 c2, c0b, c1b, c2b, c0b1, c0b2;

            float r0, r1, r2;

            Vector2 c0;
            Vector2[] A = new Vector2[(int)iterations + 2];
            Vector2[] B = new Vector2[(int)iterations + 2];

            if (Math.Abs(wp1p2.X) > Math.Abs(wp1p2.Y))
            {
                c0 = new Vector2(lp1.X + lp1p2.Y * lp1p2y * lp1p2x, lp2.Y);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X + c1ac2a.Y * lp1p2y * lp1p2x, c2a.Y);

                c0a1 = Vector2.Lerp(c1a, c0a, 0.5f);
                c0a2 = Vector2.Lerp(c2a, c0a, 0.5f);

                c0b = new Vector2(c1b.X + c1bc2b.Y * lp1p2y * lp1p2x, c2b.Y);

                c0b1 = Vector2.Lerp(c1b, c0b, 0.5f);
                c0b2 = Vector2.Lerp(c2b, c0b, 0.5f);

                Vector2 midpoint = Vector2.Lerp(c0a2, c0b2, 0.5f);

                Vector2 c0a2m = new Vector2(midpoint.X, c0a2.Y);
                Vector2 c0b2m = new Vector2(midpoint.X, c0b2.Y);

                c0a2 = Vector2.Lerp(c0a2, c0a2m, 0.5f);
                c0b2 = Vector2.Lerp(c0b2, c0b2m, 0.5f);
            }
            else
            {
                c0 = new Vector2(lp1.X , lp2.Y - lp1p2.X * lp1p2y * lp1p2x);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X, c2a.Y - c1ac2a.X * lp1p2y * lp1p2x);

                c0a1 = Vector2.Lerp(c1a, c0a, 0.5f);
                c0a2 = Vector2.Lerp(c2a, c0a, 0.5f);

                c0b = new Vector2(c1b.X, c2b.Y - c1bc2b.X * lp1p2y * lp1p2x);

                c0b1 = Vector2.Lerp(c1b, c0b, 0.5f);
                c0b2 = Vector2.Lerp(c2b, c0b, 0.5f);

                Vector2 midpoint = Vector2.Lerp(c0a1, c0b1, 0.5f);

                Vector2 c0a1m = new Vector2(c0a1.X, midpoint.Y);
                Vector2 c0b1m = new Vector2(c0b1.X, midpoint.Y);

                c0a1 = Vector2.Lerp(c0a1, c0a1m, 0.5f);
                c0b1 = Vector2.Lerp(c0b1, c0b1m, 0.5f);
            }

            for (float i = 1f; i < iterations + 1; i++)
            {
                var t = ((i - 1) / (iterations - 1));

                A[(int)i] = (float)Math.Pow((1 - t), 3) * c1a + 3 * (float)Math.Pow((1 - t), 2) * t * c0a1 + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0a2 + (float)Math.Pow(t, 3f) * c2a;
                B[(int)i] = (float)Math.Pow((1 - t), 3) * c1b + 3 * (float)Math.Pow((1 - t), 2) * t * c0b1 + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0b2 + (float)Math.Pow(t, 3f) * c2b;
            }

            return (c0, A, B);
        }

        private static (Vector2 c0, Vector2[] A, Vector2[] B) GenerateJK(Vector2 wp1p2, Vector2 lp1, Vector2 lp2, float curve, float width, int iterations)
        {

            Vector2 lp1p2 = lp1 - lp2;

            Vector2 c1, c0a, c1a, c2a, c0a1, c0a2;
            Vector2 c2, c0b, c1b, c2b, c0b1, c0b2;

            float r0, r1, r2;

            Vector2 c0;
            Vector2[] A = new Vector2[(int)iterations + 2];
            Vector2[] B = new Vector2[(int)iterations + 2];

            if (Math.Abs(wp1p2.X) > Math.Abs(wp1p2.Y))
            {
                c0 = new Vector2(lp1.X, lp2.Y - lp1p2.X);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X, c2a.Y - c1ac2a.X);

                c0a1 = Vector2.Lerp(c1a, c0a, 0.5f);
                c0a2 = Vector2.Lerp(c2a, c0a, 0.5f);

                c0b = new Vector2(c1b.X, c2b.Y - c1bc2b.X);

                c0b1 = Vector2.Lerp(c1b, c0b, 0.5f);
                c0b2 = Vector2.Lerp(c2b, c0b, 0.5f);

                Vector2 midpoint = Vector2.Lerp(c0a1, c0b1, 0.5f);

                Vector2 c0a1m = new Vector2(c0a1.X, midpoint.Y);
                Vector2 c0b1m = new Vector2(c0b1.X, midpoint.Y);

                c0a1 = Vector2.Lerp(c0a1, c0a1m, 0.5f);
                c0b1 = Vector2.Lerp(c0b1, c0b1m, 0.5f);
            }
            else
            {
                c0 = new Vector2(lp1.X + lp1p2.Y, lp2.Y);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X + c1ac2a.Y, c2a.Y);

                c0a1 = Vector2.Lerp(c1a, c0a, 0.5f);
                c0a2 = Vector2.Lerp(c2a, c0a, 0.5f);

                c0b = new Vector2(c1b.X + c1bc2b.Y, c2b.Y);

                c0b1 = Vector2.Lerp(c1b, c0b, 0.5f);
                c0b2 = Vector2.Lerp(c2b, c0b, 0.5f);

                Vector2 midpoint = Vector2.Lerp(c0a2, c0b2, 0.5f);

                Vector2 c0a2m = new Vector2(midpoint.X, c0a2.Y);
                Vector2 c0b2m = new Vector2(midpoint.X, c0b2.Y);

                c0a2 = Vector2.Lerp(c0a2, c0a2m, 0.5f);
                c0b2 = Vector2.Lerp(c0b2, c0b2m, 0.5f);
            }

            for (float i = 1f; i < iterations + 1; i++)
            {
                var t = ((i - 1) / (iterations - 1));

                A[(int)i] = (float)Math.Pow((1 - t), 3) * c1a + 3 * (float)Math.Pow((1 - t), 2) * t * c0a1 + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0a2 + (float)Math.Pow(t, 3f) * c2a;
                B[(int)i] = (float)Math.Pow((1 - t), 3) * c1b + 3 * (float)Math.Pow((1 - t), 2) * t * c0b1 + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0b2 + (float)Math.Pow(t, 3f) * c2b;
            }

            return (c0, A, B);
        }

        private static (Vector2 c0, Vector2[] A, Vector2[] B) GenerateLM(Vector2 wp1p2, Vector2 lp1, Vector2 lp2, float curve, float width, int iterations)
        {

            Vector2 lp1p2 = lp1 - lp2;

            Vector2 c1, c0a, c1a, c2a, c0a1, c0a2;
            Vector2 c2, c0b, c1b, c2b, c0b1, c0b2;

            float r0, r1, r2;

            Vector2 c0;
            Vector2[] A = new Vector2[(int)iterations + 2];
            Vector2[] B = new Vector2[(int)iterations + 2];

            if (Math.Abs(wp1p2.X) > Math.Abs(wp1p2.Y))
            {
                c0 = new Vector2(lp1.X, lp2.Y + lp1p2.X);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X, c2a.Y + c1ac2a.X);

                c0a1 = Vector2.Lerp(c1a, c0a, 0.5f);
                c0a2 = Vector2.Lerp(c2a, c0a, 0.5f);

                c0b = new Vector2(c1b.X, c2b.Y + c1bc2b.X);

                c0b1 = Vector2.Lerp(c1b, c0b, 0.5f);
                c0b2 = Vector2.Lerp(c2b, c0b, 0.5f);

                Vector2 midpoint = Vector2.Lerp(c0a1, c0b1, 0.5f);

                Vector2 c0a1m = new Vector2(c0a1.X, midpoint.Y);
                Vector2 c0b1m = new Vector2(c0b1.X, midpoint.Y);

                c0a1 = Vector2.Lerp(c0a1, c0a1m, 0.5f);
                c0b1 = Vector2.Lerp(c0b1, c0b1m, 0.5f);
            }
            else
            {
                c0 = new Vector2(lp1.X - lp1p2.Y, lp2.Y);
                r0 = Math.Clamp(Math.Clamp(curve, 0, Math.Clamp(curve, 0, Vector2.Distance(lp1, c0))), 0, Vector2.Distance(lp2, c0));

                r1 = (r0 + 0.001f) / (float)(Vector2.Distance(lp1, c0) + 0.001f); c1 = Vector2.Lerp(c0, lp1, r1);
                r2 = (r0 + 0.001f) / (float)(Vector2.Distance(lp2, c0) + 0.001f); c2 = Vector2.Lerp(c0, lp2, r2);

                c1a = c1 + MathF.LookAt(c1, c0, width);
                c1b = c1 - MathF.LookAt(c1, c0, width);

                c2a = c2 - MathF.LookAt(c2, c0, width);
                c2b = c2 + MathF.LookAt(c2, c0, width);

                A[0] = lp1 + MathF.LookAt(lp1, c0, width);
                B[0] = lp1 - MathF.LookAt(lp1, c0, width);

                A[(int)iterations + 1] = lp2 - MathF.LookAt(lp2, c0, width);
                B[(int)iterations + 1] = lp2 + MathF.LookAt(lp2, c0, width);

                Vector2 c1ac2a = c1a - c2a;
                Vector2 c1bc2b = c1b - c2b;

                c0a = new Vector2(c1a.X - c1ac2a.Y, c2a.Y);

                c0a1 = Vector2.Lerp(c1a, c0a, 0.5f);
                c0a2 = Vector2.Lerp(c2a, c0a, 0.5f);

                c0b = new Vector2(c1b.X - c1bc2b.Y, c2b.Y);

                c0b1 = Vector2.Lerp(c1b, c0b, 0.5f);
                c0b2 = Vector2.Lerp(c2b, c0b, 0.5f);

                Vector2 midpoint = Vector2.Lerp(c0a2, c0b2, 0.5f);

                Vector2 c0a2m = new Vector2(midpoint.X, c0a2.Y);
                Vector2 c0b2m = new Vector2(midpoint.X, c0b2.Y);

                c0a2 = Vector2.Lerp(c0a2, c0a2m, 0.5f);
                c0b2 = Vector2.Lerp(c0b2, c0b2m, 0.5f);
            }

            for (float i = 1f; i < iterations + 1; i++)
            {
                var t = ((i - 1) / (iterations - 1));

                A[(int)i] = (float)Math.Pow((1 - t), 3) * c1a + 3 * (float)Math.Pow((1 - t), 2) * t * c0a1 + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0a2 + (float)Math.Pow(t, 3f) * c2a;
                B[(int)i] = (float)Math.Pow((1 - t), 3) * c1b + 3 * (float)Math.Pow((1 - t), 2) * t * c0b1 + 3 * (1 - t) * (float)Math.Pow(t, 2) * c0b2 + (float)Math.Pow(t, 3f) * c2b;
            }

            return (c0, A, B);
        }
    }
}