using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Silk.NET.Vulkan;

namespace Xirface
{
    public class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Size { get; set; }
        public float Depth { get; set; }

        public Mesh<VertexPositionColor>? Mesh;
        public Shader Shader;
        

        public Entity(Shader shader, Vector2 position, Vector2 origin, float depth)
        {
            this.Origin = origin;
            this.Position = position;
            this.Depth = depth;

            this.Shader = shader;
        }


        public virtual void Draw(GraphicsManager graphics, CommandBuffer cmd, Camera Camera, Shader shader)
        {
            

            shader.SetView(Matrix4x4.CreateTranslation(-Camera.Position.X, -Camera.Position.Y,0));
            shader.SetWorld(Matrix4x4.CreateScale(1, 1, 1) * Matrix4x4.CreateRotationZ(0) * Matrix4x4.CreateTranslation(new Vector3(Position.X - Origin.X, Position.Y - Origin.Y, Depth)));
            shader.SetProjection(Matrix4x4.CreateOrthographic(1.920f * Camera.Zoom, 1.080f * Camera.Zoom, 0, -1f));

            shader.Apply(cmd);

            Mesh!.Buffer(graphics);
            Mesh!.Draw(graphics,cmd);
        }


    }
}
