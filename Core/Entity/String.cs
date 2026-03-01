using System.Numerics;

namespace Xirface
{
    public class String : Entity
    {
        public List<VertexPositionColorFont> Vertices = new();
        public List<uint> Indices = new();

        public String(GraphicsManager graphics,Shader shader, Vector2 position, Vector2 origin, Color color, Font font, string content)
         : base(graphics, shader, position, origin)
        {
            uint indexOffset = 0;
            int spacing = 0;
            ushort previousGlyphIndex = 0;

            foreach (char c in content)
            {
                if (!font.CharacterGlyphDict.ContainsKey(c) || font.CharacterGlyphDict[c] == null || font.CharacterGlyphDict[c] == font.CharacterGlyphDict[' '])
                {
                    font.IndexKerningDict.TryGetValue((previousGlyphIndex, font.CharacterGlyphDict[' '].Index), out short advance);

                    spacing += font.CharacterGlyphDict[' '].AdvanceWidth + advance;
                    previousGlyphIndex = font.CharacterGlyphDict[' '].Index;
                    continue;
                }
                else
                {
                    font.IndexKerningDict.TryGetValue((previousGlyphIndex, font.CharacterGlyphDict[c].Index), out short advance);

                    (spacing, indexOffset) = AddGlyph(font.CharacterGlyphDict[c], spacing, indexOffset, advance, color);
                    previousGlyphIndex = font.CharacterGlyphDict[c].Index;
                }
            }
        }
        private (int, uint) AddGlyph(Glyph Glyph, int spacing, uint indexOffset, short kerning, Color color)
        {
            foreach (uint i in Glyph.Indices)
                Indices.Add((uint)(i + indexOffset));

            indexOffset += (uint)Glyph.Indices.Count;

            spacing += kerning;

            foreach (VertexPositionColorFont vertex in Glyph.Vertices)
                Vertices.Add(new VertexPositionColorFont(new Vector3(vertex.Position.X + spacing, vertex.Position.Y, vertex.Position.Z), vertex.Color, vertex.TexCoord, vertex.Curved, vertex.Side));

            spacing += Glyph.AdvanceWidth;

            return (spacing, indexOffset);
        }
    }
}