using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Xirface
{
    public partial class GraphicsManager
    {
        public Vulkan? Vulkan;
        public Instance Instance;
        public PhysicalDevice PhysicalDevice;
        public Device Device;
        public Queue GraphicsQueue;
        public uint GraphicsQueueFamilyIndex;

        public KhrSurface? KhrSurface;
        public KhrSwapchain? KhrSwapchain;
        public SurfaceKHR Surface;
        public SwapchainKHR Swapchain;
        public Format SwapchainFormat;
        public Extent2D SwapchainExtent;
        public Image[]? SwapchainImages;
        public ImageView[]? SwapchainImageViews;

        public RenderPass RenderPass;
        public Framebuffer[]? Framebuffers;

        public CommandPool CommandPool;
        public CommandBuffer[]? CommandBuffers;

        public Semaphore[]? ImageAvailableSemaphores;
        public Semaphore[]? RenderFinishedSemaphores;
        public Fence[]? InFlightFences;
        public const int MaxFramesInFlight = 2;

        public int CurrentFrame = 0;

        public GraphicsManager(IWindow window)
        {
            Vulkan = GetGraphicsApi();

            Instance = CreateInstance(window);

            PhysicalDevice = GetPhysicalDevice();

            GraphicsQueueFamilyIndex = GetPhysicalDeviceQueueFamily();

            Device = CreateLogicalDevice();

            GraphicsQueue = GetGraphicsQueue();

            Surface = CreateSurface(window);

            VerifyKHRSurfaceExtension();

            VerifyKHRSwapchainExtension();

            VerifyPhysicalDeviceSurfaceSupport();

            (SwapchainFormat,SwapchainExtent, Swapchain, SwapchainImages, SwapchainImageViews) = CreateSwapchain(window);

            RenderPass = CreateRenderPass();

            Framebuffers = CreateFramebuffers();

            CommandBuffers = CreateCommandPool();

            (ImageAvailableSemaphores, RenderFinishedSemaphores, InFlightFences) = CreateSyncObjects();
        }

        private Vulkan GetGraphicsApi()
        {
            return Vulkan.GetApi();
        }

        private unsafe Instance CreateInstance(IWindow window)
        {
            IntPtr appName = Marshal.StringToHGlobalAnsi("Railmap");
            IntPtr engineName = Marshal.StringToHGlobalAnsi("Xirface");

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)appName,
                ApplicationVersion = Vk.MakeVersion(1, 0, 0),
                PEngineName = (byte*)engineName,
                EngineVersion = Vk.MakeVersion(1, 0, 0),
                ApiVersion = Vk.Version12
            };

            var extension = window.VkSurface!.GetRequiredExtensions(out var extCount);

            byte* layerName = (byte*)Marshal.StringToHGlobalAnsi("VK_LAYER_KHRONOS_validation");

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = extCount,
                PpEnabledExtensionNames = extension,
                EnabledLayerCount = 1,
                PpEnabledLayerNames = &layerName
            };

            Instance instance;
            if (Vulkan!.CreateInstance(&createInfo, null, &instance) != Result.Success)
            {
                throw new Exception("VULKAN: Failed to create instance");
            }

            Marshal.FreeHGlobal((IntPtr)layerName);
            Marshal.FreeHGlobal(appName);
            Marshal.FreeHGlobal(engineName);

            return instance;
        }

        private unsafe PhysicalDevice GetPhysicalDevice()
        {
            uint deviceCount = 0;
            Vulkan!.EnumeratePhysicalDevices(Instance, &deviceCount, null);

            if (deviceCount == 0) throw new Exception("VULKAN: Failed to find any physical device");

            PhysicalDevice[] devices = new PhysicalDevice[deviceCount];
            fixed (PhysicalDevice* devicesPtr = devices)
            {
                Vulkan.EnumeratePhysicalDevices(Instance, &deviceCount, devicesPtr);
            }

            //PhysicalDeviceProperties properties;
            //Vulkan.GetPhysicalDeviceProperties(devices[0], &properties);

            return devices[0];
        }

        private unsafe uint GetPhysicalDeviceQueueFamily()
        {
            uint queueFamilyCount = 0;
            Vulkan!.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyCount, null);

            QueueFamilyProperties[] queueFamilies = new QueueFamilyProperties[queueFamilyCount];
            fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                Vulkan.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyCount, queueFamiliesPtr);
            }

            GraphicsQueueFamilyIndex = uint.MaxValue;

            for (uint i = 0; i < queueFamilyCount; i++)
            {
                if ((queueFamilies[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
                {
                    GraphicsQueueFamilyIndex = i;  
                    break;
                }
            }

            if (GraphicsQueueFamilyIndex == uint.MaxValue)
                throw new Exception("VULKAN: Failed to find a graphics queue family");

            return GraphicsQueueFamilyIndex;
        }

        private unsafe Device CreateLogicalDevice()
        {
            Device device;
            float queuePriority = 1.0f;

            DeviceQueueCreateInfo queueCreateInfo = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = GraphicsQueueFamilyIndex,
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };

            byte* swapchainExtName = (byte*)Marshal.StringToHGlobalAnsi("VK_KHR_swapchain");

            DeviceCreateInfo deviceCreateInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo,
                EnabledExtensionCount = 1,
                PpEnabledExtensionNames = &swapchainExtName
            };

            
            if (Vulkan!.CreateDevice(PhysicalDevice, &deviceCreateInfo, null, &device) != Result.Success)
                throw new Exception("VULKAN: Failed to create a logical device");
            

            Marshal.FreeHGlobal((IntPtr)swapchainExtName);

            return device;
        }

        private unsafe Queue GetGraphicsQueue()
        {
            Queue graphicsQueue;
            Vulkan!.GetDeviceQueue(Device, GraphicsQueueFamilyIndex, 0, &graphicsQueue);
            return graphicsQueue;
        }
        private unsafe SurfaceKHR CreateSurface(IWindow window)
        {
           return window.VkSurface!.Create<AllocationCallbacks>(Instance.ToHandle(), null).ToSurface();
        }

        private void VerifyKHRSurfaceExtension()
        {
            if (!Vulkan!.TryGetInstanceExtension<KhrSurface>(Instance, out KhrSurface))
                throw new Exception("VULKAN: Failed to find KHR_surface extension");
        }

        private void VerifyKHRSwapchainExtension()
        {
            if (!Vulkan!.TryGetDeviceExtension<KhrSwapchain>(Instance, Device, out KhrSwapchain))
                throw new Exception("VULKAN: Failed to find KHR_swapchain");
        }

        private unsafe void VerifyPhysicalDeviceSurfaceSupport()
        {
            Bool32 presentSupport = false;
            if (KhrSurface!.GetPhysicalDeviceSurfaceSupport(PhysicalDevice, GraphicsQueueFamilyIndex, Surface, &presentSupport) != Result.Success)
                throw new Exception("VULKAN: Failed to query physical device surface support");
            if (!presentSupport)
                throw new Exception("VULKAN: Physical device does not support presentation");
        }

        private unsafe (Format, Extent2D, SwapchainKHR, Image[], ImageView[]) CreateSwapchain(IWindow window)
        {
            Format format; Extent2D extent; SwapchainKHR swapchain; Image[] images; ImageView[] imageViews;

            KhrSurface!.GetPhysicalDeviceSurfaceCapabilities(PhysicalDevice, Surface, out SurfaceCapabilitiesKHR capabilities);

            uint formatCount = 0;
            KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, Surface, &formatCount, null);

            SurfaceFormatKHR[] formats = new SurfaceFormatKHR[formatCount];
            fixed (SurfaceFormatKHR* formatsPtr = formats)
            {
                KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, Surface, &formatCount, formatsPtr);
            }

            uint presentModeCount = 0;
            KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, Surface, &presentModeCount, null);

            PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
            fixed (PresentModeKHR* presentModesPtr = presentModes)
            {
                KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, Surface, &presentModeCount, presentModesPtr);
            }

            SurfaceFormatKHR surfaceFormat = formats[0];

            foreach (SurfaceFormatKHR availableFormat in formats)
            {
                if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    surfaceFormat = availableFormat;
                    break;
                }
            }

            format = surfaceFormat.Format;

            PresentModeKHR presentMode = PresentModeKHR.FifoKhr;

            foreach (PresentModeKHR availablePresentMode in presentModes)
            {
                if (availablePresentMode == PresentModeKHR.MailboxKhr)
                {
                    presentMode = availablePresentMode;
                    break;
                }
            }

            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                extent = capabilities.CurrentExtent;
            }
            else
            {
                extent = new Extent2D
                {
                    Width = Math.Clamp((uint)window.Size.X, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width),
                    Height = Math.Clamp((uint)window.Size.Y, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height),
                };
            }

            uint imageCount = capabilities.MinImageCount + 1;
            if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
                imageCount = capabilities.MaxImageCount;

            SwapchainCreateInfoKHR swapchainCreateInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = Surface,
                MinImageCount = imageCount,
                ImageFormat = format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
                ImageSharingMode = SharingMode.Exclusive,
                PreTransform = capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,
                OldSwapchain = default,
            };

            
            if (KhrSwapchain!.CreateSwapchain(Device, &swapchainCreateInfo, null, out swapchain) != Result.Success)
                throw new Exception("VULKAN: Failed to create a swapchain");

            uint swapchainImageCount = 0;
            KhrSwapchain.GetSwapchainImages(Device, swapchain, &swapchainImageCount, null);

            images = new Image[swapchainImageCount];
            fixed (Image* swapchainImagesPtr = images)
            {
                KhrSwapchain.GetSwapchainImages(Device, swapchain, &swapchainImageCount, swapchainImagesPtr);
            }

            imageViews = new ImageView[swapchainImageCount];

            for (int i = 0; i < swapchainImageCount; i++)
            {
                ImageViewCreateInfo imageViewCreateInfo = new()
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = images[i],
                    ViewType = ImageViewType.Type2D,
                    Format = format,
                    Components = new ComponentMapping
                    {
                        R = ComponentSwizzle.Identity,
                        G = ComponentSwizzle.Identity,
                        B = ComponentSwizzle.Identity,
                        A = ComponentSwizzle.Identity,
                    },
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1,
                    }
                };

                if (Vulkan!.CreateImageView(Device, &imageViewCreateInfo, null, out imageViews[i]) != Result.Success)
                    throw new Exception($"Vulkan failed to create image view {i}"); 
            }
            return (format, extent, swapchain, images, imageViews);
        }

        private unsafe RenderPass CreateRenderPass()
        {
            AttachmentDescription colorAttachment = new()
            {
                Format = SwapchainFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };

            AttachmentReference colorAttachmentReference = new()
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            SubpassDescription subpass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachmentReference,
            };

            SubpassDependency dependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.ColorAttachmentWriteBit,
            };

            RenderPassCreateInfo renderPassCreateInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorAttachment,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency,
            };

            RenderPass renderPass;
            if (Vulkan!.CreateRenderPass(Device, &renderPassCreateInfo, null, &renderPass) != Result.Success)
                throw new Exception("VULKAN: Failed to create a render pass");

            return renderPass;
        }

        private unsafe Framebuffer[] CreateFramebuffers()
        {
            Framebuffers = new Framebuffer[SwapchainImageViews!.Length];

            for (int i = 0; i < SwapchainImageViews.Length; i++)
            {
                ImageView attachment = SwapchainImageViews[i];

                FramebufferCreateInfo framebufferCreateInfo = new()
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = RenderPass,
                    AttachmentCount = 1,
                    PAttachments = &attachment,
                    Width = SwapchainExtent.Width,
                    Height = SwapchainExtent.Height,
                    Layers = 1
                };

                fixed (Framebuffer* framebufferPtr = &Framebuffers[i])
                {
                    if (Vulkan!.CreateFramebuffer(Device, &framebufferCreateInfo, null, framebufferPtr) != Result.Success)
                    {
                        throw new Exception($"VULKAN: Failed to create framebuffer {i}");
                    }
                }
            }

            return Framebuffers;
        }

        private unsafe CommandBuffer[] CreateCommandPool()
        {
            CommandBuffer[] commandBuffers;

            CommandPoolCreateInfo poolCreateInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
                QueueFamilyIndex = GraphicsQueueFamilyIndex
            };

            fixed (CommandPool* poolPtr = &CommandPool)
            {
                if (Vulkan!.CreateCommandPool(Device, &poolCreateInfo, null, poolPtr) != Result.Success)
                    throw new Exception("VULKAN: Failed to create command pool");
            }


            commandBuffers = new CommandBuffer[Framebuffers!.Length];

            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = CommandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)commandBuffers.Length
            };

            fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
            {
                if (Vulkan!.AllocateCommandBuffers(Device, &allocInfo, commandBuffersPtr) != Result.Success)
                    throw new Exception("VULKAN: Failed to allocate command buffers");
            }

            return commandBuffers;
        }

        private unsafe (Semaphore[], Semaphore[], Fence[]) CreateSyncObjects()
        {
            Semaphore[] imageAvailableSemaphores = new Semaphore[MaxFramesInFlight];
            Semaphore[] renderFinishedSemaphores = new Semaphore[SwapchainImages!.Length];
            Fence[] inFlightFences = new Fence[MaxFramesInFlight];

            SemaphoreCreateInfo semaphoreInfo = new() { SType = StructureType.SemaphoreCreateInfo };
            FenceCreateInfo fenceInfo = new() { SType = StructureType.FenceCreateInfo, Flags = FenceCreateFlags.SignaledBit };

            for (int i = 0; i < MaxFramesInFlight; i++)
            {
                fixed (Semaphore* s = &imageAvailableSemaphores[i])
                    Vulkan!.CreateSemaphore(Device, &semaphoreInfo, null, s);
            }
            for (int i = 0; i < SwapchainImages.Length; i++) 
            {
                fixed (Semaphore* s = &renderFinishedSemaphores[i])
                    Vulkan!.CreateSemaphore(Device, &semaphoreInfo, null, s);
            }

            for (int i = 0; i < MaxFramesInFlight; i++)
            {
                fixed (Fence* f = &inFlightFences[i])
                    Vulkan!.CreateFence(Device, &fenceInfo, null, f);
            }

            return (imageAvailableSemaphores, renderFinishedSemaphores, inFlightFences);
        }

        public unsafe void Begin(out CommandBuffer cmd, out uint imageIndex)
        {
            Vulkan!.WaitForFences(Device, 1, ref InFlightFences![CurrentFrame], true, ulong.MaxValue);

            uint index = 0;
            KhrSwapchain!.AcquireNextImage(Device, Swapchain, ulong.MaxValue,
                ImageAvailableSemaphores![CurrentFrame], default, &index);
            imageIndex = index;

            Vulkan!.ResetFences(Device, 1, ref InFlightFences[CurrentFrame]);

            cmd = CommandBuffers![CurrentFrame];
            Vulkan!.ResetCommandBuffer(cmd, 0);

            CommandBufferBeginInfo beginInfo = new() { SType = StructureType.CommandBufferBeginInfo };
            Vulkan!.BeginCommandBuffer(cmd, &beginInfo);

            ClearValue clearValue = new() { Color = new ClearColorValue(1f, 0f, 0f, 1f) };

            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = RenderPass,
                Framebuffer = Framebuffers[imageIndex],
                RenderArea = new Rect2D { Offset = new Offset2D(0, 0), Extent = SwapchainExtent },
                ClearValueCount = 1,
                PClearValues = &clearValue
            };

            Vulkan!.CmdBeginRenderPass(cmd, &renderPassInfo, SubpassContents.Inline);

            Viewport viewport = new() { X = 0, Y = 0, Width = SwapchainExtent.Width, Height = SwapchainExtent.Height, MinDepth = 0f, MaxDepth = 1f };
            Rect2D scissor = new() { Offset = new Offset2D(0, 0), Extent = SwapchainExtent };
            Vulkan!.CmdSetViewport(cmd, 0, 1, &viewport);
            Vulkan!.CmdSetScissor(cmd, 0, 1, &scissor);
        }

        public unsafe void End(CommandBuffer cmd, uint imageIndex)
        {
            Vulkan!.CmdEndRenderPass(cmd);
            Vulkan!.EndCommandBuffer(cmd);

            Semaphore waitSemaphore = ImageAvailableSemaphores![CurrentFrame];
            Semaphore signalSemaphore = RenderFinishedSemaphores![imageIndex];
            PipelineStageFlags waitStage = PipelineStageFlags.ColorAttachmentOutputBit;

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &waitSemaphore,
                PWaitDstStageMask = &waitStage,
                CommandBufferCount = 1,
                PCommandBuffers = &cmd,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = &signalSemaphore
            };

            Vulkan!.QueueSubmit(GraphicsQueue, 1, &submitInfo, InFlightFences![CurrentFrame]);

            SwapchainKHR swapchain = Swapchain;
            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &signalSemaphore,
                SwapchainCount = 1,
                PSwapchains = &swapchain,
                PImageIndices = &imageIndex
            };

            KhrSwapchain!.QueuePresent(GraphicsQueue, &presentInfo);
            CurrentFrame = (CurrentFrame + 1) % MaxFramesInFlight;
        }

        public unsafe CommandBuffer BeginSingleTimeCommands()
        {
            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = CommandPool,
                CommandBufferCount = 1
            };

            CommandBuffer cmd;
            Vulkan!.AllocateCommandBuffers(Device, &allocInfo, &cmd);

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit
            };

            Vulkan.BeginCommandBuffer(cmd, &beginInfo);
            return cmd;
        }

        public unsafe void EndSingleTimeCommands(CommandBuffer cmd)
        {
            Vulkan!.EndCommandBuffer(cmd);

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &cmd
            };

            Vulkan!.QueueSubmit(GraphicsQueue, 1, &submitInfo, default);
            Vulkan.QueueWaitIdle(GraphicsQueue);
            Vulkan.FreeCommandBuffers(Device, CommandPool, 1, &cmd);
        }

        public unsafe void CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, out Silk.NET.Vulkan.Buffer buffer, out DeviceMemory memory)
        {
            BufferCreateInfo bufferCreateInfo = new()
            {
                SType = StructureType.BufferCreateInfo,
                Size = size,
                Usage = usage,
                SharingMode = SharingMode.Exclusive,
            };

            fixed (Buffer* bufferPtr = &buffer)
            {
                if (Vulkan!.CreateBuffer(Device, &bufferCreateInfo, null, bufferPtr) != Result.Success)
                {
                    throw new Exception("VULKAN: Failed to create a buffer");
                }
            }

            MemoryRequirements memoryRequirments;
            Vulkan.GetBufferMemoryRequirements(Device, buffer, &memoryRequirments);


            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirments.Size,
                MemoryTypeIndex = GetMemoryType(memoryRequirments.MemoryTypeBits, properties),
            };

            fixed (DeviceMemory* memoryPtr = &memory)
            {
                if (Vulkan.AllocateMemory(Device, &memoryAllocateInfo, null, memoryPtr) != Result.Success)
                {
                    throw new Exception("VULKAN: Failed to allocate buffer memory");
                }
            }

            Vulkan.BindBufferMemory(Device, buffer, memory, 0);
        }

        public unsafe uint GetMemoryType(uint typeFilter, MemoryPropertyFlags properties)
        {
            PhysicalDeviceMemoryProperties memoryProperties;
            Vulkan!.GetPhysicalDeviceMemoryProperties(PhysicalDevice, &memoryProperties);

            for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if ((typeFilter & (1 << (int)i)) != 0 &&
                    (memoryProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
                    return i;
            }

            throw new Exception("VULKAN: Failed to find a suitable memory type");
        }

        public unsafe void Dispose()
        {
            for (int i = 0; i < MaxFramesInFlight; i++)
            {
                Vulkan!.DestroySemaphore(Device, ImageAvailableSemaphores![i], null);
            }

            for (int i = 0; i < SwapchainImages!.Length; i++)
            {
                Vulkan!.DestroySemaphore(Device, RenderFinishedSemaphores![i], null);
            }

            for (int i = 0; i < MaxFramesInFlight; i++)
            {
                Vulkan!.DestroyFence(Device, InFlightFences![i], null);
            }

            Vulkan!.DestroyCommandPool(Device, CommandPool, null);

            foreach (var framebuffer in Framebuffers!)
                Vulkan.DestroyFramebuffer(Device, framebuffer, null);

            Vulkan.DestroyRenderPass(Device, RenderPass, null);

            foreach (var imageView in SwapchainImageViews!)
                Vulkan.DestroyImageView(Device, imageView, null);

            KhrSwapchain!.DestroySwapchain(Device, Swapchain, null);
            Vulkan.DestroyDevice(Device, null);
            KhrSurface!.DestroySurface(Instance, Surface, null);
            Vulkan.DestroyInstance(Instance, null);
            Vulkan.Dispose();
        }
    }
}
