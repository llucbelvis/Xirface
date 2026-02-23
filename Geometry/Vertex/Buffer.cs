using Silk.NET.Windowing;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Text;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Xirface
{
    public partial class Mesh<TVertex> where TVertex : unmanaged, IVertex
    {
        
        private unsafe void CreateBuffer<T>(Vulkan graphics, T[] data, BufferUsageFlags usage,
    out Silk.NET.Vulkan.Buffer buffer, out DeviceMemory memory) where T : unmanaged
        {
            ulong size  = (ulong)(sizeof(T)*data.Length);

            graphics.CreateBuffer(
                size, 
                BufferUsageFlags.TransferSrcBit, 
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out Buffer stagingBuffer,
                out DeviceMemory stagingMemory
            );

            void* mapped;

            graphics.Vk.MapMemory(graphics.Device, stagingMemory, 0, size, 0, &mapped);
            fixed (T* verticesPtr = data)
            {
                System.Buffer.MemoryCopy(verticesPtr, mapped, size, size);
            }

            graphics.Vk.UnmapMemory(graphics.Device, stagingMemory);

            graphics.CreateBuffer(
                size,
                BufferUsageFlags.TransferDstBit | usage,
                MemoryPropertyFlags.DeviceLocalBit,
                out buffer,
                out memory
                );

            CommandBuffer cmd = graphics.BeginSingleTimeCommands();
            BufferCopy copyRegion = new() { Size = size };
            graphics.Vk.CmdCopyBuffer(cmd, stagingBuffer, buffer, 1, &copyRegion);
            graphics.EndSingleTimeCommands(cmd);

            graphics.Vk.DestroyBuffer(graphics.Device, stagingBuffer, null);
            graphics.Vk.FreeMemory(graphics.Device, stagingMemory, null);


        }
    }
}
