using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Xirface
{
    public partial class Mesh<TVertex> where TVertex : unmanaged, IVertex
    {
        public static Mesh<VertexFont> Text(Vector2 size, Color color, Font font, string content)
        {
            Mesh<VertexFont> Mesh = new();

            Mesh.Body = [

                new Vector2(0, 0), 
                new Vector2(0, size.Y), 
                new Vector2(size.X, size.Y),
                new(size.X, 0)
            ];

            List<VertexFont> vertexBuffer = new();
            List<uint> indexBuffer = new();

            uint indexOffset = 0;
            int spacing = 0;
            ushort previousGlyphIndex = 0;

            foreach (char c in content)
            {
                if (!font.CharacterGlyphDict.ContainsKey(c) || font.CharacterGlyphDict[c] == null || font.CharacterGlyphDict[c] == font.CharacterGlyphDict[' '])
                {
                    font.IndexKerningDict.TryGetValue((previousGlyphIndex, font.CharacterGlyphDict[' '].index), out short advance);

                    spacing += font.CharacterGlyphDict[' '].advanceWidth + advance;
                    previousGlyphIndex = font.CharacterGlyphDict[' '].index;
                    continue;
                }
                else
                {
                    font.IndexKerningDict.TryGetValue((previousGlyphIndex, font.CharacterGlyphDict[c].index), out short advance);

                    (spacing, vertexBuffer, indexBuffer, indexOffset) = AddGlyph(font.CharacterGlyphDict[c], spacing, vertexBuffer, indexBuffer, indexOffset, advance, color);
                    previousGlyphIndex = font.CharacterGlyphDict[c].index;
                }
            }

            Mesh.Vertices = vertexBuffer.ToArray();
            Mesh.Indices = indexBuffer.ToArray();
   

            Mesh.Dirty();

            return Mesh;

        }
        private static (int, List<VertexFont>, List<uint>, uint) AddGlyph(Glyph Glyph, int spacing, List<VertexFont> vertices, List<uint> indices, uint indexOffset, short kerning, Color color)
        {
            foreach (uint i in Glyph.indices)
                indices.Add((uint)(i + indexOffset));

            indexOffset += (uint)Glyph.indices.Count;

            spacing += kerning;

            foreach (VertexFont vertex in Glyph.vertices)
                vertices.Add(new VertexFont(new Vector3(vertex.Position.X + spacing, vertex.Position.Y, vertex.Position.Z), color, vertex.TexCoord, vertex.Curve, vertex.Side));

            spacing += Glyph.advanceWidth;

            return (spacing, vertices, indices, indexOffset);
        }
    }
}