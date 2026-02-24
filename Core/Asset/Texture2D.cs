using Silk.NET.Vulkan;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xirface
{
    public unsafe class Texture2D
    {
        public int Width;
        public int Height;

        Buffer stagingBuffer;
        DeviceMemory stagingMemory;

        public Image Image;
        public ImageView ImageView;
        public DeviceMemory Memory;

        public Sampler Sampler;

     
        public unsafe Texture2D(GraphicsManager graphics, string path)
        {
            using FileStream stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            Width = image.Width;
            Height = image.Height;

            long size = (long)image.Width * (long)image.Height * 4;
            
            graphics.CreateBuffer((ulong)size, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit, out stagingBuffer, out stagingMemory); //MAY BE FIXED

            void* data;


            graphics.Vulkan!.MapMemory(graphics.Device, stagingMemory, 0, (ulong)size, MemoryMapFlags.None, &data);

            fixed(byte* ptr = image.Data)
            {
                System.Buffer.MemoryCopy(ptr, data, size, size);
            }

            
            graphics.Vulkan.UnmapMemory(graphics.Device, stagingMemory);

            ImageCreateInfo imageCreateInfo = new()
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new Extent3D((uint)Width, (uint)Height, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Format = Format.R8G8B8A8Srgb,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
                SharingMode = SharingMode.Exclusive,
                Samples = SampleCountFlags.Count1Bit,
            };

            graphics.Vulkan.CreateImage(graphics.Device, &imageCreateInfo, null, out Image);

      
            graphics.Vulkan.GetImageMemoryRequirements(graphics.Device, Image, out MemoryRequirements memoryRequirements);

            MemoryAllocateInfo allocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = graphics.GetMemoryType(memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
            };

            graphics.Vulkan.AllocateMemory(graphics.Device, &allocateInfo, null, out Memory);
            graphics.Vulkan.BindImageMemory(graphics.Device, Image, Memory, 0);

            TransitionImageLayout(graphics, Image, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

            CopyBufferToImage(graphics, stagingBuffer, Image, (uint)Width, (uint)Height);

            TransitionImageLayout(graphics, Image, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

            graphics.Vulkan.DestroyBuffer(graphics.Device, stagingBuffer, null);
            graphics.Vulkan.FreeMemory(graphics.Device, stagingMemory, null);

            ImageViewCreateInfo imageViewCreateInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = Image,
                ViewType = ImageViewType.Type2D,
                Format = Format.R8G8B8A8Srgb,
                SubresourceRange = new ImageSubresourceRange( ImageAspectFlags.ColorBit, 0, 1, 0, 1),

            };

            graphics.Vulkan.CreateImageView(graphics.Device, &imageViewCreateInfo, null, out ImageView);

            SamplerCreateInfo samplerCreateInfo = new()
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,

                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                AnisotropyEnable = false,
                MaxAnisotropy = 16f,
                BorderColor = BorderColor.IntOpaqueBlack,
                UnnormalizedCoordinates = false,
                MipmapMode = SamplerMipmapMode.Linear,

            };

            graphics.Vulkan.CreateSampler(graphics.Device, &samplerCreateInfo, null, out Sampler);
        }

        void TransitionImageLayout(GraphicsManager graphics, Image image, ImageLayout oldLayout, ImageLayout newLayout)
        {
            var cmd = graphics.BeginSingleTimeCommands();

            PipelineStageFlags srcStage, dstStage;
            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = image,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    LevelCount = 1,
                    LayerCount = 1,
                }
            };

            if (oldLayout == ImageLayout.Undefined &&
                newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                srcStage = PipelineStageFlags.TopOfPipeBit;
                dstStage = PipelineStageFlags.TransferBit;
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal &&
                     newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStage = PipelineStageFlags.TransferBit;
                dstStage = PipelineStageFlags.FragmentShaderBit;
            }
            else throw new Exception("VULKAN: Unsupported layout transition");

            unsafe
            {
                graphics.Vulkan!.CmdPipelineBarrier(cmd,
                    srcStage, dstStage, 0,
                    0, null,
                    0, null,
                    1, &barrier);
            }

            graphics.EndSingleTimeCommands(cmd);
        }

        private unsafe void CopyBufferToImage(GraphicsManager graphics,Buffer buffer, Image image, uint width, uint height)
        {
            var cmd = graphics.BeginSingleTimeCommands();

            BufferImageCopy region = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource = new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    LayerCount = 1,
                },
                ImageOffset = new Offset3D(0, 0, 0),
                ImageExtent = new Extent3D(width, height, 1),
            };

                graphics.Vulkan!.CmdCopyBufferToImage(cmd, buffer, image,ImageLayout.TransferDstOptimal,1, region);

            graphics.EndSingleTimeCommands(cmd);
        }



        public void Dispose()
        {
           
        }
    }
}
