
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;


namespace Xirface
{
    public class String : Entity
    {
        public enum Alignment
        {
            BottomLeft, BottomCenter, BottomRight,
            CenterLeft, CenterCenter, CenterRight,
            TopLeft, TopCenter, TopRight,
        }
        public string Content {  get; set; }
        public Font Font { get; set; }
        public float FontSize { get; set; }
        public Color Color { get; set; }

        public new Mesh<VertexFont> Mesh;
		public Alignment Align { get; set; }
		public Vector2 AlignmentOffset;

        public String(Shader<VertexPositionColor> shader, Vector2 position, Vector2 origin, Vector2 size, float depth, Color color, Font font, string content, float fontSize, Alignment align)
        : base(shader, position, origin, depth)
        {
            Size = size;
            Color = color;

            Font = font;
            FontSize = fontSize;
            Content = content;

            Align = align;

            Mesh = Mesh<VertexFont>.Text(size, color, font, content);
            this.AlignmentOffset = UpdateAlignment(size, new Vector2(Mesh.Vertices.Max(vertex => vertex.Position.X) * (FontSize / Font.unitsPerEm), Mesh.Vertices.Max(v => v.Position.Y) * (FontSize / Font.unitsPerEm)), align);
        }

        public new void Draw(Camera camera)
        {

            Shader.SetView(Matrix4x4.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0) * Matrix4x4.CreateScale(camera.Zoom, camera.Zoom, 1));
            Shader.SetWorld(Matrix4x4.CreateScale(FontSize / Font.unitsPerEm, FontSize / Font.unitsPerEm, 1) * Matrix4x4.CreateTranslation(AlignmentOffset.X + Position.X - Origin.X, AlignmentOffset.Y + Position.Y - Origin.Y, Depth));
            Shader.SetProjection(Matrix4x4.CreateOrthographic(camera.Width, camera.Height, -1f, 1f));
    
        }

        public static Vector2 UpdateAlignment(Vector2 bounds, Vector2 size, Alignment align) => align switch
		{
			Alignment.BottomLeft => Vector2.Zero,
			Alignment.BottomCenter => new Vector2(bounds.X / 2 - size.X / 2, 0),
			Alignment.BottomRight => new Vector2(bounds.X - size.X, 0),
			Alignment.CenterLeft => new Vector2(0, bounds.Y / 2 - size.Y / 2),
			Alignment.CenterCenter => new Vector2(bounds.X / 2 - size.X / 2, bounds.Y / 2 - size.Y / 2),
			Alignment.CenterRight => new Vector2(bounds.X - size.X, bounds.Y / 2 - size.Y / 2),
			Alignment.TopLeft => new Vector2(0, bounds.Y - size.Y),
			Alignment.TopCenter => new Vector2(bounds.X / 2 - size.X / 2, bounds.Y - size.Y),
			Alignment.TopRight => new Vector2(bounds.X - size.X, bounds.Y - size.Y),
			_ => Vector2.Zero
		};
	}
}
