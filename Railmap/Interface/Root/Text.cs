using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Xirface
{
    public class Text : Root
    {
        public string Content { get; set;} 
        public Font Font { get; set; }
        public float FontSize { get; set; }
        public String.Alignment Alignment { get; set; }
        public Vector2 AlignmentOffset;

        public new Mesh<VertexFont> Mesh;

        public Text(string id, Vector2 position, Vector2 origin, Vector2 size, float depth, Color fillColor, bool visible, HashSet<Root> children, Font font, string content, float fontSize, String.Alignment alignment)
        : base(id, position, origin, size, depth, fillColor, visible,children)
        {
            Font = font;
            FontSize = fontSize;
            Alignment = alignment;
            Content = content;
            Mesh = Mesh<VertexFont>.Text(size, fillColor, font, content);

            
            AlignmentOffset = String.UpdateAlignment(size, new Vector2(Mesh.Vertices.Max(v => v.Position.X) * (FontSize / font.unitsPerEm), Mesh.Vertices.Max(v => v.Position.Y) * (FontSize / font.unitsPerEm)), alignment);
        }

        public override void Refresh()
        {
            if (Content.Length <= 0 )
            {
                Mesh = null;
                AlignmentOffset = Vector2.Zero;

                return;
            }

            Mesh = Mesh<VertexFont>.Text(Size, FillColor, Font, Content);

            if (Mesh.Vertices.Length > 0)
                AlignmentOffset = String.UpdateAlignment(Size, new Vector2(Mesh.Vertices.Max(vertex => vertex.Position.X) * (FontSize / Font.unitsPerEm), Mesh.Vertices.Max(v => v.Position.Y) * (FontSize / Font.unitsPerEm)), Alignment);
        }




        public override void Draw(Camera camera, Vector2 world)
        {
            if (Mesh is null) return;


            switch (positioning)
            {
                case Positioning.Hierarchical:
                    Shader.SetView(Matrix4x4.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0) * Matrix4x4.CreateScale(camera.Zoom, camera.Zoom, 1));
                    Shader.SetWorld(Matrix4x4.CreateScale(FontSize / Font.unitsPerEm, FontSize / Font.unitsPerEm, 1) * Matrix4x4.CreateTranslation(world.X + AlignmentOffset.X + Position.X - Origin.X - (camera.Width / 2), world.Y + AlignmentOffset.Y + Position.Y - Origin.Y - (camera.Height / 2), Depth));
                    break;

                case Positioning.Absolute:
                    Shader.SetView(Matrix4x4.Identity);
                    Shader.SetWorld(Matrix4x4.CreateScale(FontSize / Font.unitsPerEm, FontSize / Font.unitsPerEm, 1) * Matrix4x4.CreateTranslation(AlignmentOffset.X + Position.X - Origin.X - (camera.Width / 2), AlignmentOffset.Y + Position.Y - Origin.Y - (camera.Height / 2), Depth));
                    break;

                case Positioning.Zero:
                    Shader.SetView(Matrix4x4.Identity);
                    Shader.SetWorld(Matrix4x4.CreateTranslation(-camera.Width / 2, -camera.Height / 2, Depth));
                    break;
            }

            Shader.SetProjection(Matrix4x4.CreateOrthographic(camera.Width, camera.Height, -1f, 1f));
          

            world += Position;

            foreach (Root child in Children)
            {
                child.Draw(camera, world);
            }
        }



        public override void Buffer()
        {
           
        }

        public override void AssignShader()
        {
        }
    }
}