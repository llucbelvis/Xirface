
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Xirface
{
    public class Frame : Root
    {
        public bool Clicked;
        public bool Active { get; set; }
        public float Curve { get; set; }
        public float Stroke { get; set; }
        public Color StrokeColor { get; set; }

        public Frame(string id, Vector2 position, Vector2 origin, Vector2 size, float depth, Color fillColor, bool active, bool visible, HashSet<Root> children, float curve, float stroke, Color strokeColor)
        : base(id, position, origin, size, depth, fillColor, visible, children)
        {
            Active = active;
            Curve = curve;
            Stroke = Math.Min(stroke, 1);
            StrokeColor = strokeColor;

            if (Curve == 0)
                Mesh = Mesh<VertexPositionColor>.Square(Size, FillColor);
            
            else
                Mesh = Mesh<VertexPositionColor>.Square(Size, StrokeColor, Curve, Stroke, FillColor, 12);
            
        }

        public override void Refresh()
        {
            if (Curve == 0)
                Mesh = Mesh<VertexPositionColor>.Square(Size, FillColor);
            else
                Mesh = Mesh<VertexPositionColor>.Square(Size, StrokeColor, Curve, Stroke, FillColor, 12);

        }
        public override void Draw(Camera camera, Vector2 world)
        {
            
            switch (positioning)
            {
                case Positioning.Hierarchical:
                    Shader.SetView(Matrix4x4.CreateTranslation(new Vector3(-camera.Position, 0)) * Matrix4x4.CreateScale(camera.Zoom, camera.Zoom, 1));
                    Shader.SetWorld(Matrix4x4.CreateTranslation(new Vector3(world.X + Position.X - Origin.X - (camera.Width / 2), world.Y + Position.Y - Origin.Y - (camera.Height / 2), Depth)));
                    break;

                case Positioning.Absolute:
                    Shader.SetView(Matrix4x4.Identity);
                    Shader.SetWorld(Matrix4x4.CreateTranslation(new Vector3(Position.X - Origin.X - 960, Position.Y - Origin.Y - 540, Depth)));
                    break;

                case Positioning.Zero:
                    Shader.SetView(Matrix4x4.Identity);
                    Shader.SetWorld(Matrix4x4.CreateTranslation(new Vector3(-960, -540, Depth)));
                    break;
            }

            Shader.SetProjection(Matrix4x4.CreateOrthographic(1920f, 1080f, -1f, 1f));
            
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