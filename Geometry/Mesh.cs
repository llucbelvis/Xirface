using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
namespace Xirface
{
    public partial class Mesh<TVertex> where TVertex : unmanaged, IVertex
    {
        public uint[]? Indices;
        public TVertex[]? Vertices;


        public Physics.BodyType BodyType;
        public Vector2[]? Body;
        public Vector2 Bounds;

        private Silk.NET.Vulkan.Buffer vertexBuffer;
        private DeviceMemory vertexMemory;

        private Silk.NET.Vulkan.Buffer indexBuffer;
        private DeviceMemory indexMemory;

        public bool dirty;
        public bool buffered; 


        public unsafe void Buffer(Vulkan graphics)
        {
            if (dirty)
            {
                if (Vertices == null || Indices == null || IsEmpty()) return;

                if (buffered)
                {
                    graphics.Vk.DestroyBuffer(graphics.Device, vertexBuffer, null);
                    graphics.Vk.FreeMemory(graphics.Device, vertexMemory, null);
                    graphics.Vk.DestroyBuffer(graphics.Device, indexBuffer, null);
                    graphics.Vk.FreeMemory(graphics.Device, indexMemory, null);
                }
                CreateBuffer(graphics, Vertices, BufferUsageFlags.VertexBufferBit, out vertexBuffer, out vertexMemory);
                CreateBuffer(graphics, Indices, BufferUsageFlags.IndexBufferBit, out indexBuffer, out indexMemory);

                dirty = false;
                buffered = true;
            }
        }

        public bool IsEmpty()
        {
            return (Vertices?.Length ?? 0) <= 3 || (Indices?.Length ?? 0) <= 3;
        }

        public void Dirty() => dirty = true;

        public unsafe void Draw(Vulkan graphics, CommandBuffer cmd)
        {
            if (!buffered) return;

            Buffer buffer = vertexBuffer;
            ulong offset = 0;
            graphics.Vk.CmdBindVertexBuffers(cmd, 0, 1, &buffer, &offset);
            graphics.Vk.CmdBindIndexBuffer(cmd, indexBuffer, 0, IndexType.Uint32);
            graphics.Vk.CmdDrawIndexed(cmd, (uint)Indices!.Length, 1, 0, 0, 0);

        }

        public unsafe void Dispose(Vulkan graphics)
        {
            if (buffered)
            {
                graphics.Vk.DestroyBuffer(graphics.Device, vertexBuffer, null);
                graphics.Vk.FreeMemory(graphics.Device, vertexMemory, null);
                graphics.Vk.DestroyBuffer(graphics.Device, indexBuffer, null);
                graphics.Vk.FreeMemory(graphics.Device, indexMemory, null);
            }

                Indices = null;
            Vertices = null;
        }

    }

    
}
