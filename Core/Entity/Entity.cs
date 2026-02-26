using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Xirface
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Transform
    {
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public unsafe void Update(void* mapped)
        {
            Transform t = new()
            {
                View = View,
                Projection = Projection,
            };

            System.Buffer.MemoryCopy(&t, mapped, sizeof(Transform), sizeof(Transform));
        }
    }

    public struct DeviceBuffer
    {
        public Buffer Buffer;
        public DeviceMemory Memory;
        public unsafe void* Mapped;

        public unsafe void Dispose(GraphicsManager graphics)
        {
            graphics.Vulkan.UnmapMemory(graphics.Device, Memory);
            graphics.Vulkan.DestroyBuffer(graphics.Device, Buffer, null);
            graphics.Vulkan.FreeMemory(graphics.Device, Memory, null);
        }
    }

    public class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Size { get; set; }
        public float Depth { get; set; }
        public Mesh<VertexPositionColor>? Mesh;
        public Shader Shader;

        private Transform transform;
        private DeviceBuffer deviceBuffer;

        protected DescriptorSet descriptorSet;
        public unsafe Entity(Shader shader, GraphicsManager graphics, Vector2 position, Vector2 origin)
        {
            this.Shader = shader;
            this.Origin = origin;
            this.Position = position;
            //this.Depth = depth;

            shader.CreateUniformBuffer(out deviceBuffer.Buffer, out deviceBuffer.Memory, out deviceBuffer.Mapped);
            descriptorSet = shader.AllocateDescriptorSet(deviceBuffer.Buffer);
        }

        public unsafe void SetView(Matrix4x4 value) { transform.View = value; transform.Update(deviceBuffer.Mapped);}
        public unsafe void SetProjection(Matrix4x4 value) { transform.Projection = value; transform.Update(deviceBuffer.Mapped); }
        public unsafe void SetWorld(Matrix4x4 value, GraphicsManager graphics, CommandBuffer cmd) => graphics.Vulkan.CmdPushConstants( cmd,Shader.PipelineLayout,ShaderStageFlags.VertexBit, 0,(uint)sizeof(Matrix4x4), &value);

        public unsafe virtual void Draw(GraphicsManager graphics, CommandBuffer cmd, Camera camera, IWindow window)
        {
            SetView(Matrix4x4.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0));
            SetProjection(Matrix4x4.CreateOrthographic(window.Size.X * camera.Zoom, -window.Size.Y * camera.Zoom, -1, 1f));
            SetWorld(Matrix4x4.CreateScale(1, 1, 1)* Matrix4x4.CreateRotationZ(0)* Matrix4x4.CreateTranslation(new Vector3(Position.X - Origin.X, Position.Y - Origin.Y, Depth)), graphics, cmd);

            Shader.Apply(cmd, descriptorSet);
            Mesh!.Buffer(graphics);
            Mesh!.Draw(graphics, cmd);
        }
    }
}