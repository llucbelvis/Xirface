using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

using Silk.NET.OpenGL;

namespace Xirface
{
    public class Texture : Root
    {
        public Texture2D Texture2D { get; set; }
        public new Mesh<VertexPositionColorTexture> Mesh;
        public Texture(string id, Vector2 position, Vector2 origin, Vector2 size, float depth, Color fillColor, bool visible, HashSet<Root> children, Texture2D texture2D)
        : base(id, position, origin, size, depth, fillColor, visible, children)
        {
            Texture2D = texture2D;
            Mesh = Mesh<VertexPositionColorTexture>.Square(size, fillColor, texture2D);
        }

        public override void Refresh()
        {
            Mesh = Mesh<VertexPositionColorTexture>.Square(Size, FillColor, Texture2D);
        }

        public override void Draw(Camera Camera, Vector2 world)
        {
            

            switch (positioning)
            {
                case Positioning.Hierarchical:

                    Shader.SetView(Matrix4x4.CreateTranslation(new Vector3(-Camera.Position, 0)) * Matrix4x4.CreateScale(Camera.Zoom, Camera.Zoom, 1));
                    Shader.SetWorld(Matrix4x4.CreateScale(1, 1, 1) * Matrix4x4.CreateRotationZ(0) * Matrix4x4.CreateTranslation(world.X + Position.X - Origin.X - (Camera.Width / 2), world.Y + Position.Y + Origin.Y - (Camera.Height / 2), Depth));

                    break;

                case Positioning.Absolute:

                    Shader.SetView(Matrix4x4.Identity);
                    Shader.SetWorld(Matrix4x4.CreateScale(1, 1, 1) * Matrix4x4.CreateRotationZ(0) * Matrix4x4.CreateTranslation(Position.X - Origin.X - (Camera.Width / 2), Position.Y + Origin.Y - (Camera.Height / 2), Depth));

                    break;

                case Positioning.Zero:

                    Shader.SetView(Matrix4x4.Identity);
                    Shader.SetWorld(Matrix4x4.CreateTranslation(-960, -540, Depth));

                    break;
            }

            Shader.SetProjection(Matrix4x4.CreateOrthographic(1920f, 1080f, -1f, 1f));

            Shader.SetTexture(Texture2D);
            Shader.SetColor(FillColor);


            

            world += Position;

            foreach (Root Child in Children)
            {
                Child.Draw(Camera, world);
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