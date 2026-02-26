using Silk.NET.Vulkan;

namespace Xirface
{
    public unsafe class Texture2D
    {
        public int Width;
        public int Height;

        public Image Image;
        public ImageView ImageView;
        public DeviceMemory Memory;

        public Sampler Sampler;

        public static void TransitionImageLayout(GraphicsManager graphics, Image image, ImageLayout oldLayout, ImageLayout newLayout)
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

        public static unsafe void CopyBufferToImage(GraphicsManager graphics,Buffer buffer, Image image, uint width, uint height)
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
