using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Xirface
{
    public class Glyph
    {
        public ushort Index;

        private int[] ContourEndIndices;
        private Point[] Points;

        public List<VertexPositionColorFont> Vertices = new();
        public List<int> Indices = new();

        public ushort AdvanceWidth;
        public short LeftSideBearing;

        public struct Contour(int id)
        {
            public int id = id;
            public List<Point> glyphPoints = new();
            public List<Vector2> fixedPoints = new();

            public List<Triangle> triangles = new();
        }

        public struct Point(Vector2 position, bool onCurve)
        {
            public Vector2 position = position;
            public bool onCurve = onCurve;
        }
        public struct Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            public Vector2 a = a;
            public Vector2 b = b;
            public Vector2 c = c;
        }

        public Glyph(ushort index, int[] contourEndIndices, Point[] points, ushort advanceWidth, short leftSideBearing)
        {
            Index = index;
            ContourEndIndices = contourEndIndices;
            Points = points;
            AdvanceWidth = advanceWidth;
            LeftSideBearing = leftSideBearing;
        }

        

        public static Contour CreateContour(Span<Point> points, int id)
        {
            Contour Contour = new Contour(id);

            for (int i = 0; i < points.Length; i++)
            {
                Point curr = points[i];
                Point next = points[(i + 1) % points.Length];

                Contour.glyphPoints.Add(curr);

                if (!curr.onCurve && !next.onCurve)
                {
                    Contour.glyphPoints.Add(new Point((curr.position + next.position) / 2, true));
                }
            }

            return Contour;
        }

        public static Contour SimplifyContour(Contour contour)
        {
            for (int i = 0; i < contour.glyphPoints.Count; i++)
            {
                Point prev = contour.glyphPoints[(i - 1 + contour.glyphPoints.Count) % contour.glyphPoints.Count];
                Point curr = contour.glyphPoints[i];
                Point next = contour.glyphPoints[(i + 1) % contour.glyphPoints.Count];

                if (curr.onCurve)
                    contour.fixedPoints.Add(curr.position);
                else
                {
                    float cross = (curr.position.X - prev.position.X) * (next.position.Y - prev.position.Y) - (curr.position.Y - prev.position.Y) * (next.position.X - prev.position.X);

                    if (cross > 0)
                    {
                        contour.fixedPoints.Add(curr.position);
                    }

                    contour.triangles.Add(new Triangle(prev.position, curr.position, next.position));
                }
            }

            return contour;
        }

        (bool intersected, float t, Vector2 intersection) Ray(Vector2 p, Vector2 a, Vector2 b)
        {
            if (Math.Abs(a.Y - b.Y) < 1e-6f) return (false, 0, Vector2.Zero);

            if (p.Y < Math.Min(a.Y, b.Y) || p.Y >= Math.Max(a.Y, b.Y))
                return (false, 0, Vector2.Zero);

            float A = -a.Y + b.Y;
            float B = p.Y - a.Y;
            float t = B / A;
    
            float X = (a.X+(a.X-b.X)*t);

            if (t >= 0 && t <= 1 && X >= p.X)
            {
                return (true, X - p.X, new Vector2(X, p.Y));
            }

            return (false, 0, Vector2.Zero);
        }

        public struct RayResult()
        {
            public Vector2 p0, p1, intersection;
            public float t;
        }

        (bool intersected, List<RayResult>) Intersection(Vector2 vertex, Vector2[] contour)
        {
            int i = 0;
            List<RayResult> results = new();

            if (contour.Length == 0)
                return (false, results);

            while (i < contour.Length)
            {
                Vector2 p0 = contour[i];

                Vector2 p1 = contour[(i + 1) % contour.Length];


                (bool intersected, float t, Vector2 intersection) = Ray(vertex, p0, p1);

                if (intersected)
                    results.Add(new RayResult() { p0 = p0, p1 = p1, intersection = intersection, t = t });
                i += 1;
            }

            return (results.Any(), results);
        }

        (bool intersected, List<RayResult>) Intersection(Point vertex, Point[] contour)
        {
            int i = 0;
            List<RayResult> results = new();

            if (contour.Length == 0)
                return (false, results);

            while (i < contour.Length)
            {
                Vector2 p0 = contour[i].position;

                Vector2 p1 = contour[(i + 1) % contour.Length].position;


                (bool intersected, float t, Vector2 intersection) = Ray(vertex.position, p0, p1);

                if (intersected)
                    results.Add(new RayResult() { p0 = p0, p1 = p1, intersection = intersection, t = t });
                i += 1;
            }

            return (results.Any(), results);
        }

        bool IsPointInTriangle(Vector2 p, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float E = 1e-6f;

            double cross1 = Cross(v2 - v1, p - v1);
            double cross2 = Cross(v3 - v2, p - v2);
            double cross3 = Cross(v1 - v3, p - v3);

            bool hasNeg = (cross1 < 0) || (cross2 < E) || (cross3 <0);
            bool hasPos = (cross1 > -0) || (cross2 > -E) || (cross3 > -0);

            return !(hasNeg && hasPos);
        }
        bool IsClockwise(List<Vector2> contour)
        {
            float sum = 0;
            for (int i = 0; i < contour.Count; i++)
            {
                Vector2 curr = contour[i];
                Vector2 next = contour[(i + 1) % contour.Count];
                sum += (next.X - curr.X) * (next.Y + curr.Y);
            }
            return sum > 0;
        }

        static bool IsClockwise(Contour contour)
        {
            float sum = 0;
            for (int i = 0; i < contour.glyphPoints.Count; i++)
            {
                Vector2 curr = contour.glyphPoints[i].position;
                Vector2 next = contour.glyphPoints[(i + 1) % contour.glyphPoints.Count].position;
                sum += (next.X - curr.X) * (next.Y + curr.Y);
            }
            return sum > 0;
        }

        static double Cross(Vector2 a, Vector2 b)
        {
            return (double)a.X * b.Y - (double)a.Y * b.X;
        }

        Vector2[] GetTriangles(List<Vector2> contour)
        {
            contour = new List<Vector2>(contour);
            int offset = 0;
            List<Vector2> triangles = new();
            int maxAttempts = contour.Count * 100;
            int attempts = 0;

            bool isClockwise = IsClockwise(contour);
            while (contour.Count > 3 && attempts < maxAttempts)
            {
                attempts++;

                Vector2 prev = contour[(offset - 1 + contour.Count) % contour.Count];
                Vector2 curr = contour[offset];
                Vector2 next = contour[(offset + 1) % contour.Count];
                
;                bool pointOnTriangle = false;

                foreach (Vector2 p in contour)
                {
                    if (((p - prev).LengthSquared() < 1e-6f) || ((p - curr).LengthSquared() < 1e-6f) || ((p - next).LengthSquared() < 1e-6f))
                        continue;

                    if (IsPointInTriangle(p, prev, curr, next))
                    {
                        pointOnTriangle = true;
                        break;
                    }
                }

                double cross = Cross(curr - prev, next - prev);
                bool isConvex = isClockwise ? (cross < -1e-6f) : (cross >1e-6f);

                if (isConvex && !pointOnTriangle)
                {
                    triangles.Add(prev);
                    triangles.Add(curr);
                    triangles.Add(next);

                    contour.RemoveAt(offset);

                    isClockwise = IsClockwise(contour);

                    offset = (offset - 1 + contour.Count) % contour.Count;
                }
                else
                {
                    offset = (offset + 1) % contour.Count;
                }

            }


            if (contour.Count == 3)
            {
                triangles.Add(contour[0]);
                triangles.Add(contour[1]);
                triangles.Add(contour[2]);
            }

            return triangles.ToArray();
        }

        private static int CountIntersections(List<(Contour c, List<Vector2> i)> intersections)
        {
            int i = 0;

            foreach (var intersectonGroup in intersections)
            {
                i += intersectonGroup.i.Count;
            }

            return i; 
        }
        public Dictionary<Contour, List<Contour>> SortContours(List<Contour> contours)
        {
            Dictionary<Contour, List<(Contour c, List<Vector2> i)>> contourIntersections = new();

            foreach (Contour contour in contours)
            {
                List<(Contour, List<Vector2> i)> intersections = new();

                foreach(Contour otherContour in contours)
                {
                    if (otherContour.glyphPoints == contour.glyphPoints) continue;

                    (bool intersected, List<RayResult> results) = Intersection(contour.glyphPoints.MaxBy(p => p.position.X), otherContour.glyphPoints.ToArray());
                    if (intersected) intersections.Add((otherContour, results.Select(r => r.intersection).Distinct().ToList()));
                }

                contourIntersections.Add(contour, intersections);
            }

            Dictionary<Contour, List<Contour>> Hierarchy = new();

                foreach (Contour contour in contours)
            {
                if (CountIntersections(contourIntersections[contour]) == 0)
                {
                    List<Contour> holes = new();
                    
                    Hierarchy.Add(contour, new List<Contour>());
                }
            }
            
            foreach(Contour suspectedContour in contours)
            {
                if (Hierarchy.ContainsKey(suspectedContour)) continue;

                if (CountIntersections(contourIntersections[suspectedContour]) == 1)
                {
                    if (Hierarchy.ContainsKey(contourIntersections[suspectedContour].First().c))
                    {
                        if (IsClockwise(contourIntersections[suspectedContour].First().c) != IsClockwise(suspectedContour))
                        {
                            Hierarchy[contourIntersections[suspectedContour].First().c].Add(suspectedContour); 
                            continue;
                        }
                            
                    }
                    Hierarchy.Add(suspectedContour, new List<Contour>());
                             
                } else if (CountIntersections(contourIntersections[suspectedContour]) % 2 == 0)
                {
                    Hierarchy.Add(suspectedContour, new List<Contour>());
                }
                else
                {
                    (Contour contour, Vector2 intersection) minIntersection = (default, new Vector2(float.MaxValue, 0));

                    foreach ((Contour intersectedContour, List<Vector2> intersections) in contourIntersections[suspectedContour])
                    {
                        foreach (Vector2 i in intersections)
                        {
                            if (i.X < minIntersection.intersection.X && Vector2.Distance(suspectedContour.glyphPoints.MaxBy(p => p.position.X).position, i) > 1e-6f)
                            {
                                minIntersection = (intersectedContour, i);
                            }
                        }
                    }

                    if (Hierarchy.ContainsKey(minIntersection.contour))
                    {
                        if (IsClockwise(minIntersection.contour) != IsClockwise(suspectedContour))
                        {
                            Hierarchy[minIntersection.contour].Add(suspectedContour);
                        }
                        else
                        {
                            Hierarchy.Add(suspectedContour, new List<Contour>());
                        }
                            
                    }
                }
            }

            return Hierarchy;
        }

        public List<Contour> GetBridgedContours(Dictionary<Contour, List<Contour>> Hierarchy)
        {
            List<Contour> contours = new();

            for (int c = 0; c < Hierarchy.Count; c++)
            {
                Contour contour = Hierarchy.Keys.ToList()[c];

                var sortedHoles = Hierarchy[contour].OrderByDescending(h => h.fixedPoints.Max(v => v.X)).ToList();
                

                for (int h = 0; h < sortedHoles.Count; h++)
                {
                    Contour hole = sortedHoles[h];
                    Vector2 vertex = hole.fixedPoints.MaxBy(vertex => vertex.X);

                    (bool intersected, List<RayResult> results) = Intersection(vertex, contour.fixedPoints.ToArray());
                    Vector2 bridgePoint = Vector2.Zero;

                    if (!intersected) continue;

                    List<Vector2> reflex = new();

                    RayResult edge = results.MinBy(e => e.t);

                    if (Vector2.Distance(edge.p0, edge.intersection) < 1e-6f)
                        bridgePoint = edge.p0;

                    else if (Vector2.Distance(edge.p1, edge.intersection) < 1e-6f)
                        bridgePoint = edge.p1;
                    else
                    {
                        bridgePoint = (edge.p0.X > edge.p1.X) ? edge.p0 : edge.p1;

                        for (int i = 0; i < contour.fixedPoints.Count; i++)
                        {

                            Vector2 prev = contour.fixedPoints[(i - 1 + contour.fixedPoints.Count) % contour.fixedPoints.Count];
                            Vector2 curr = contour.fixedPoints[i];
                            Vector2 next = contour.fixedPoints[(i + 1) % contour.fixedPoints.Count];

                            Vector2 edgeA = curr - prev;
                            Vector2 edgeB = next - curr;

                            float cross = edgeA.X * edgeB.Y - edgeA.Y * edgeB.X;

                            if (IsClockwise(contour.fixedPoints) && cross > 1e-6f || !IsClockwise(contour.fixedPoints) && cross < 1e-6f)
                                reflex.Add(curr);
                        }

                        List<Vector2> inPoints = new();

                        foreach (Vector2 point in reflex)
                        {
                            if (IsPointInTriangle(point, vertex, edge.intersection, bridgePoint))
                            {
                                inPoints.Add(point);
                            }
                        }

                        if (inPoints.Count != 0)
                        {
                            float minAngle = float.MaxValue;
                            Vector2 visible = Vector2.Zero;

                            foreach (Vector2 point in inPoints)
                            {
                                Vector2 vp = vertex - point;
                                vp = Vector2.Normalize(vp);

                                float dotProduct = Vector2.Dot(new Vector2(1, 0), vp);
                                float angle = (float)Math.Acos(dotProduct);

                                if (angle < minAngle)
                                {
                                    minAngle = angle;
                                    visible = point;
                                }
                            }

                            bridgePoint = visible;
                        }
                    }

                    Contour merge = new Contour(contour.id);
                    merge.triangles = new List<Triangle>(contour.triangles);
                    merge.triangles.AddRange(hole.triangles);

                    foreach (Vector2 point in contour.fixedPoints)
                    {
                        merge.fixedPoints.Add(point);

                        if (Vector2.Distance(point, bridgePoint) < 1e-4f)
                        {
                            int startIndex = hole.fixedPoints.IndexOf(vertex);
                            for (int i = 0; i < hole.fixedPoints.Count; i++)
                            {
                                int idx = (startIndex + i) % hole.fixedPoints.Count;
                                merge.fixedPoints.Add(hole.fixedPoints[idx]);
                            }

                            merge.fixedPoints.Add(vertex);
                            merge.fixedPoints.Add(point);

                        }
                    }

                    contour = merge;
                }
                
                contours.Add(contour);
            }

            return contours;
        }

        public Glyph Load()
        {
            int contourStartIndex = 0;
            List<Contour> Contours = new();
            Dictionary<Contour, List<Contour>> Hierarchy = new();
            int id = 0;

            foreach (int contourEndIndex in ContourEndIndices)
            {
                int numPointsInContour = contourEndIndex - contourStartIndex + 1;
                Span<Point> points = Points.AsSpan(contourStartIndex, numPointsInContour);

 
                Contours.Add(CreateContour(points,id));

                contourStartIndex = contourEndIndex + 1;
            }

            Hierarchy = SortContours(Contours);

            for (int o = 0; o < Hierarchy.Keys.Count; o++)
            {
                List<Contour> holes = new();
                Hierarchy.Keys.ToArray()[o] = SimplifyContour(Hierarchy.Keys.ToArray()[o]);

                for (int i = 0; i < Hierarchy[Hierarchy.Keys.ToArray()[o]].Count; i++)
                {
                    Hierarchy[Hierarchy.Keys.ToArray()[o]][i] = SimplifyContour(Hierarchy[Hierarchy.Keys.ToArray()[o]][i]);
                }
            }

            Contours = GetBridgedContours(Hierarchy);

            int offset = 0;

            foreach (Contour contour in Contours)
            {
                for (int i = 0; i < contour.triangles.Count; i++)
                {
                    Triangle triangle = contour.triangles[i];
                    List<Vector2> points = new();

                    points.Add(triangle.a); points.Add(triangle.b); points.Add(triangle.c);

                    bool side = IsClockwise(points);

                    if (!side)
                    {
                        points.Reverse();
                    }

                    Vertices.Add(new VertexPositionColorFont(new Vector3(points[0], 0),  Color.Black, new Vector2(0, 0), 1, side ? (short)1 : (short)-1));
                    Vertices.Add(new VertexPositionColorFont(new Vector3(points[1], 0), Color.Black, new Vector2(0.5f, 0),  1, side ? (short)1 : (short)-1));
                    Vertices.Add(new VertexPositionColorFont(new Vector3(points[2], 0), Color.Black,new Vector2(1, 1), 1, side ? (short)1 : (short)-1));

                    Indices.Add(offset + i * 3 + 0);
                    Indices.Add(offset + i * 3 + 1);
                    Indices.Add(offset + i * 3 + 2);
                }

                offset += contour.triangles.Count * 3;
            }

            foreach (Contour contour in Contours)
            {
                Vector2[] triangles = GetTriangles(contour.fixedPoints);

                for (int i = 0; i < triangles.Length / 3; i++)
                {
                    Vertices.Add(new VertexPositionColorFont(new Vector3(triangles[i * 3 + 0], 0), Color.Black, Vector2.Zero, -1, 1));
                    Vertices.Add(new VertexPositionColorFont(new Vector3(triangles[i * 3 + 1], 0), Color.Black, Vector2.Zero, -1, 1));
                    Vertices.Add(new VertexPositionColorFont(new Vector3(triangles[i * 3 + 2], 0), Color.Black, Vector2.Zero, -1, 1));

                    Indices.Add(offset + i * 3 + 0);
                    Indices.Add(offset + i * 3 + 1);
                    Indices.Add(offset + i * 3 + 2);
                }

                offset += triangles.Length;


            }

            return this;
        }

        public struct CoordsOnCurve
        {
            public int[] coords;
            public bool[] onCurve;
        }

        public static Glyph GetSimpleGlyph(Reader Reader, ushort index, uint[] GlyphLocation, uint glyphLocation, ushort advanceWidth, short leftSideBearing)
        {
            Reader.GoTo(glyphLocation);
            short numberOfContours = (short)Reader.ReadUInt16();

            if (numberOfContours == 0)
            {
                return new Glyph(index, Array.Empty<int>(), Array.Empty<Point>(), 0, 0);
            }
            if (numberOfContours < 0)
            {
                return GetCompoundGlyph(Reader, index, GlyphLocation, glyphLocation, advanceWidth, leftSideBearing);
            }

            int[] contourEndIndices = new int[numberOfContours];
            Reader.SkipBytes(8);

            for (int i = 0; i < contourEndIndices.Length; i++)
                contourEndIndices[i] = Reader.ReadUInt16();

            int numPoints = contourEndIndices[^1] + 1;
            byte[] flags = new byte[numPoints];
            Reader.SkipBytes(Reader.ReadUInt16());

            for (int i = 0; i < numPoints; i++)
            {
                byte flag = Reader.ReadByte();
                flags[i] = flag;

                if (Reader.FlagBitIsSet(flag, 3))
                {
                    byte repeatCount = Reader.ReadByte();
                    for (int j = 0; j < repeatCount && i < numPoints; j++)
                    {
                        i++;
                        flags[i] = flag;
                    }
                }

            }

            CoordsOnCurve xCoords = ReadCoordinates(Reader, flags, readingX: true);
            CoordsOnCurve yCoords = ReadCoordinates(Reader, flags, readingX: false);

            Point[] Points = new Point[xCoords.coords.Length];

            for (int i = 0; i < xCoords.coords.Length; i++)
            {
                Points[i] = new Point
                {
                    position = new Vector2(xCoords.coords[i], yCoords.coords[i]),
                    onCurve = xCoords.onCurve[i],
                };
            }

            return new Glyph(index, contourEndIndices, Points, advanceWidth, leftSideBearing);
        }

        static Glyph GetCompoundGlyph(Reader Reader, ushort index, uint[] GlyphLocation, uint glyphLocation, ushort advanceWidth, short leftSideBearing)
        {
            Reader.GoTo(glyphLocation);
            Reader.SkipBytes(2 * 5);

            List<Point> allPoints = new();
            List<int> allContourEndIndices = new();

            while (true)
            {
                (Glyph componentGlyph, bool isLast) = GetNextComponentGlyph(Reader, index, GlyphLocation);

                int indexOffset = allPoints.Count;
                allPoints.AddRange(componentGlyph.Points);

                foreach (int endIndex in componentGlyph.ContourEndIndices)
                {
                    allContourEndIndices.Add(endIndex + indexOffset);
                }

                if (isLast) break;
            }

            return new Glyph(index, allContourEndIndices.ToArray(), allPoints.ToArray(), advanceWidth, leftSideBearing);
        }

        static (Glyph simpleGlyph, bool isLast) GetNextComponentGlyph(Reader Reader, ushort index, uint[] GlyphLocation)
        {
            uint flags = Reader.ReadUInt16();
            uint glyphIndex = Reader.ReadUInt16();

            uint previousLocation = Reader.GetULocation();

            Glyph Glyph = GetSimpleGlyph(Reader, index, GlyphLocation, GlyphLocation[glyphIndex], 0, 0);
            Reader.GoTo(previousLocation);

            if (!Reader.FlagBitIsSet(flags, 1)) ;
            double offsetX = Reader.FlagBitIsSet(flags, 0) ? Reader.ReadInt16() : Reader.ReadSByte();
            double offsetY = Reader.FlagBitIsSet(flags, 0) ? Reader.ReadInt16() : Reader.ReadSByte();
            double scaleX = 1; double scaleY = 1;

            if (Reader.FlagBitIsSet(flags, 3))
                scaleX = scaleY = Reader.ReadFixedPoint2Dot14();
            else if (Reader.FlagBitIsSet(flags, 6))
            {
                scaleX = Reader.ReadFixedPoint2Dot14();
                scaleY = Reader.ReadFixedPoint2Dot14();
            }
            else if (Reader.FlagBitIsSet(flags, 7)) ; // TODO 2x2 Matrix

            for (int i = 0; i < Glyph.Points.Length; i++)
            {
                Glyph.Points[i].position.X = (float)(Glyph.Points[i].position.X * scaleX + (offsetX / 10));
                Glyph.Points[i].position.Y = (float)(Glyph.Points[i].position.Y * scaleY + (offsetY / 10));
            }

            return (Glyph, !Reader.FlagBitIsSet(flags, 5));
        }

        static CoordsOnCurve ReadCoordinates(Reader Reader, byte[] flags, bool readingX)
        {
            CoordsOnCurve CoordsOnCurve = new CoordsOnCurve()
            {
                coords = new int[flags.Length],
                onCurve = new bool[flags.Length],
            };

            int offsetSizeFlagBit = readingX ? 1 : 2;
            int offsetSignOrSkipBit = readingX ? 4 : 5;


            for (int i = 0; i < CoordsOnCurve.coords.Length; i++)
            {
                CoordsOnCurve.coords[i] = CoordsOnCurve.coords[Math.Max(0, i - 1)];
                byte flag = flags[i];


                bool onCurve = Reader.FlagBitIsSet(flag, 0);
                CoordsOnCurve.onCurve[i] = onCurve;

                if (Reader.FlagBitIsSet(flag, offsetSizeFlagBit))
                {
                    byte offset = Reader.ReadByte();
                    int sign = Reader.FlagBitIsSet(flag, offsetSignOrSkipBit) ? 1 : -1;
                    CoordsOnCurve.coords[i] += (offset * sign);
                }
                else if (!Reader.FlagBitIsSet(flag, offsetSignOrSkipBit))
                {
                    short signedOffset = (short)Reader.ReadUInt16();
                    CoordsOnCurve.coords[i] += signedOffset;
                }

            }

            return CoordsOnCurve;
        }

        
    }
}
