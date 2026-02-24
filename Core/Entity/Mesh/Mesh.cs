using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Silk.NET.Vulkan;

namespace Xirface
{
    public partial class Mesh<TVertex> where TVertex : unmanaged, IVertex
    {
        public uint[]? Indices;
        public TVertex[]? Vertices;


        public Vector2[]? Body;
        public Vector2 Bounds;

        private Buffer vertexBuffer;
        private DeviceMemory vertexMemory;

        private Buffer indexBuffer;
        private DeviceMemory indexMemory;

        public bool dirty;
        public bool buffered; 


        public unsafe void Buffer(GraphicsManager graphics)
        {
            if (dirty)
            {
                if (Vertices == null || Indices == null || IsEmpty()) return;

                if (buffered)
                {
                    graphics.Vulkan!.DestroyBuffer(graphics.Device, vertexBuffer, null);
                    graphics.Vulkan.FreeMemory(graphics.Device, vertexMemory, null);
                    graphics.Vulkan.DestroyBuffer(graphics.Device, indexBuffer, null);
                    graphics.Vulkan.FreeMemory(graphics.Device, indexMemory, null);
                }
                CreateBuffer(graphics!, Vertices, BufferUsageFlags.VertexBufferBit, out vertexBuffer, out vertexMemory);
                CreateBuffer(graphics!, Indices, BufferUsageFlags.IndexBufferBit, out indexBuffer, out indexMemory);

                dirty = false;
                buffered = true;
            }
        }

        public bool IsEmpty()
        {
            return (Vertices?.Length ?? 0) <= 3 || (Indices?.Length ?? 0) <= 3;
        }

        public void Dirty() => dirty = true;

        public unsafe void Draw(GraphicsManager graphics, CommandBuffer cmd)
        {
            if (!buffered) return;

            Buffer buffer = vertexBuffer;
            ulong offset = 0;
            graphics.Vulkan!.CmdBindVertexBuffers(cmd, 0, 1, &buffer, &offset);
            graphics.Vulkan.CmdBindIndexBuffer(cmd, indexBuffer, 0, IndexType.Uint32);
            graphics.Vulkan.CmdDrawIndexed(cmd, (uint)Indices!.Length, 1, 0, 0, 0);

        }
        private unsafe void CreateBuffer<T>(GraphicsManager graphics, T[] data, BufferUsageFlags usage, out Buffer buffer, out DeviceMemory memory) where T : unmanaged
        {
            ulong size = (ulong)(sizeof(T) * data.Length);

            graphics.CreateBuffer(
                size,
                BufferUsageFlags.TransferSrcBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out Buffer stagingBuffer,
                out DeviceMemory stagingMemory
            );

            void* mapped;

            graphics.Vulkan.MapMemory(graphics.Device, stagingMemory, 0, size, 0, &mapped);
            fixed (T* verticesPtr = data)
            {
                System.Buffer.MemoryCopy(verticesPtr, mapped, size, size);
            }

            graphics.Vulkan.UnmapMemory(graphics.Device, stagingMemory);

            graphics.CreateBuffer(
                size,
                BufferUsageFlags.TransferDstBit | usage,
                MemoryPropertyFlags.DeviceLocalBit,
                out buffer,
                out memory
                );

            CommandBuffer cmd = graphics.BeginSingleTimeCommands();
            BufferCopy copyRegion = new() { Size = size };
            graphics.Vulkan.CmdCopyBuffer(cmd, stagingBuffer, buffer, 1, &copyRegion);
            graphics.EndSingleTimeCommands(cmd);

            graphics.Vulkan.DestroyBuffer(graphics.Device, stagingBuffer, null);
            graphics.Vulkan.FreeMemory(graphics.Device, stagingMemory, null);
        }

        public unsafe void Dispose(GraphicsManager graphics)
        {
            if (buffered)
            {
                graphics.Vulkan!.DestroyBuffer(graphics.Device, vertexBuffer, null);
                graphics.Vulkan.FreeMemory(graphics.Device, vertexMemory, null);
                graphics.Vulkan.DestroyBuffer(graphics.Device, indexBuffer, null);
                graphics.Vulkan.FreeMemory(graphics.Device, indexMemory, null);
            }

                Indices = null;
            Vertices = null;
        }

    }

    
}
