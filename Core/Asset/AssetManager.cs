using Silk.NET.Vulkan;
using StbImageSharp;

namespace Xirface
{
    public partial class AssetManager
    {
        public Dictionary<string, object> Assets;
        private GraphicsManager graphics;

        public AssetManager(GraphicsManager graphics)
        {
            Assets = new();
            this.graphics = graphics;
        }

        public unsafe T Load<T>(string path) where T : class
        {
            if (Assets.TryGetValue(path, out var cached)) return (cached as T)!;

            Buffer stagingBuffer;
            DeviceMemory stagingMemory;

            if (typeof(T) == typeof(Texture2D))
            {
                Texture2D texture = new();

                using FileStream stream = File.OpenRead(path);
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                texture.Width = image.Width;
                texture.Height = image.Height;

                long size = (long)image.Width * (long)image.Height * 4;

                graphics.CreateBuffer((ulong)size, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, out stagingBuffer, out stagingMemory); //MAY BE FIXED

                void* data;


                graphics.Vulkan!.MapMemory(graphics.Device, stagingMemory, 0, (ulong)size, MemoryMapFlags.None, &data);

                fixed (byte* ptr = image.Data)
                {
                    System.Buffer.MemoryCopy(ptr, data, size, size);
                }


                graphics.Vulkan.UnmapMemory(graphics.Device, stagingMemory);

                ImageCreateInfo imageCreateInfo = new()
                {
                    SType = StructureType.ImageCreateInfo,
                    ImageType = ImageType.Type2D,
                    Extent = new Extent3D((uint)texture.Width, (uint)texture.Height, 1),
                    MipLevels = 1,
                    ArrayLayers = 1,
                    Format = Format.R8G8B8A8Srgb,
                    Tiling = ImageTiling.Optimal,
                    InitialLayout = ImageLayout.Undefined,
                    Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
                    SharingMode = SharingMode.Exclusive,
                    Samples = SampleCountFlags.Count1Bit,
                };

                graphics.Vulkan.CreateImage(graphics.Device, &imageCreateInfo, null, out texture.Image);


                graphics.Vulkan.GetImageMemoryRequirements(graphics.Device, texture.Image, out MemoryRequirements memoryRequirements);

                MemoryAllocateInfo allocateInfo = new()
                {
                    SType = StructureType.MemoryAllocateInfo,
                    AllocationSize = memoryRequirements.Size,
                    MemoryTypeIndex = graphics.GetMemoryType(memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
                };

                graphics.Vulkan.AllocateMemory(graphics.Device, &allocateInfo, null, out texture.Memory);
                graphics.Vulkan.BindImageMemory(graphics.Device, texture.Image, texture.Memory, 0);

                Texture2D.TransitionImageLayout(graphics, texture.Image, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

                Texture2D.CopyBufferToImage(graphics, stagingBuffer, texture.Image, (uint)texture.Width, (uint)texture.Height);

                Texture2D.TransitionImageLayout(graphics, texture.Image, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

                graphics.Vulkan.DestroyBuffer(graphics.Device, stagingBuffer, null);
                graphics.Vulkan.FreeMemory(graphics.Device, stagingMemory, null);

                ImageViewCreateInfo imageViewCreateInfo = new()
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = texture.Image,
                    ViewType = ImageViewType.Type2D,
                    Format = Format.R8G8B8A8Srgb,
                    SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),

                };

                graphics.Vulkan.CreateImageView(graphics.Device, &imageViewCreateInfo, null, out texture.ImageView);

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

                graphics.Vulkan.CreateSampler(graphics.Device, &samplerCreateInfo, null, out texture.Sampler);

                Assets[path] = texture;
                return (texture as T)!;
            }

            throw new Exception($"Unknown asset type");
        }

        public T Load<T>(string vertex, string fragment, Type vertexType) where T : class
        {
            if (Assets.TryGetValue(fragment, out var cached)) return (cached as T)!;
            Shader shader = new(graphics);
            shader.Buffer(vertex, fragment, vertexType);
            Assets[fragment] = shader;
            return (shader as T)!;

            throw new Exception($"Unknown asset type");
        }
    }
}