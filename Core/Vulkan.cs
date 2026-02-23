using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Semaphore = Silk.NET.Vulkan.Semaphore;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Xirface
{
    public unsafe class Vulkan : IDisposable
    {
        public Vk Vk;
        public Instance Instance;
        public PhysicalDevice PhysicalDevice;
        public Device Device;
        public Queue GraphicsQueue;
        public uint GraphicsQueueFamilyIndex;

        public KhrSurface KhrSurface;
        public KhrSwapchain KhrSwapchain;
        public SurfaceKHR Surface;
        public SwapchainKHR Swapchain;
        public Format SwapchainFormat;
        public Extent2D SwapchainExtent;
        public Image[] SwapchainImages;
        public ImageView[] SwapchainImageViews;

        public RenderPass RenderPass;
        public Framebuffer[] Framebuffers;

        public CommandPool CommandPool;
        public CommandBuffer[] CommandBuffers;

        public Semaphore[] ImageAvailableSemaphores;
        public Semaphore[] RenderFinishedSemaphores;
        public Fence[] InFlightFences;
        public const int MaxFramesInFlight = 2;

        public int CurrentFrame = 0;

        

        public unsafe Vulkan(IWindow window)
        {
            Vk = Vk.GetApi();

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Railmap"),
                ApplicationVersion = Vk.MakeVersion(1, 0, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("Xirface"),
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
            if (Vk.CreateInstance(&createInfo, null, &instance) != Result.Success)
            {
                throw new Exception("Vulkan failure");
            }
                
            Instance = instance;

            Debug.WriteLine("Vulkan initialized");

            Marshal.FreeHGlobal((IntPtr)layerName);

            uint deviceCount = 0;
            Vk.EnumeratePhysicalDevices(Instance, &deviceCount, null);

            if (deviceCount == 0) throw new Exception("Vulkan failed to find a GPU");

            PhysicalDevice[] devices = new PhysicalDevice[deviceCount];
            fixed (PhysicalDevice* devicesPtr = devices)
            {
                Vk.EnumeratePhysicalDevices(Instance, &deviceCount, devicesPtr);
            }

            PhysicalDevice = devices[0];

            PhysicalDeviceProperties properties;
            Vk.GetPhysicalDeviceProperties(PhysicalDevice, &properties);

            string deviceName = Marshal.PtrToStringAnsi((IntPtr)properties.DeviceName)!;
            Debug.WriteLine($"Vulkan is using {deviceName}");

            uint queueFamilyCount = 0;
            Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyCount, null);

            QueueFamilyProperties[] queueFamilies = new QueueFamilyProperties[queueFamilyCount];
            fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyCount, queueFamiliesPtr);
            }

            GraphicsQueueFamilyIndex = uint.MaxValue;

            for (uint i = 0; i < queueFamilyCount; i++)
            {
                if ((queueFamilies[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
                {
                    GraphicsQueueFamilyIndex = i;
                    Debug.WriteLine($"Vulkan found graphics queue family at index {i}");
                    break;
                }
            }

            if (GraphicsQueueFamilyIndex == uint.MaxValue)
                throw new Exception("Vulkan failed to find a graphics queue family");

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

            Device device;
            if (Vk.CreateDevice(PhysicalDevice, &deviceCreateInfo, null, &device) != Result.Success)
                throw new Exception("Vulkan failed to create a logical device");
            Device = device;

            Debug.WriteLine("Vulkan successfully created a logical device");

            Marshal.FreeHGlobal((IntPtr)swapchainExtName);

            Queue graphicsQueue;
            Vk.GetDeviceQueue(Device, GraphicsQueueFamilyIndex, 0, &graphicsQueue);
            GraphicsQueue = graphicsQueue;

            Debug.WriteLine("Vulkan retrieved graphics queue successfully");

            Surface = window.VkSurface!.Create<AllocationCallbacks>(Instance.ToHandle(), null).ToSurface();

            Debug.WriteLine("Vulkan surface created successfully");

            if (!Vk.TryGetInstanceExtension<KhrSurface>(Instance, out KhrSurface khrSurface))
                throw new Exception("Vulkan did not find KHR_surface extension");
            KhrSurface = khrSurface;

            if (!Vk.TryGetDeviceExtension<KhrSwapchain>(Instance, Device, out KhrSwapchain khrSwapchain))
                throw new Exception("Vulkan did not find KHR_swapchain");
            KhrSwapchain = khrSwapchain;

            Bool32 presentSupport = false;
            KhrSurface.GetPhysicalDeviceSurfaceSupport(PhysicalDevice, GraphicsQueueFamilyIndex, Surface, &presentSupport);

            if (!presentSupport)
                throw new Exception("Vulkan graphics queue does not support presentation");

            Debug.WriteLine("Vulkan graphics queue supports presentation");

            KhrSurface.GetPhysicalDeviceSurfaceCapabilities(PhysicalDevice, Surface, out SurfaceCapabilitiesKHR capabilities);

            uint formatCount = 0;
            KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, Surface, &formatCount, null);

            SurfaceFormatKHR[] formats = new SurfaceFormatKHR[formatCount];
            fixed (SurfaceFormatKHR* formatsPtr = formats)
            {
                KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, Surface, &formatCount, formatsPtr);
            }

            Debug.WriteLine($"Vulkan found {formatCount} formats");

            uint presentModeCount = 0;
            KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, Surface, &presentModeCount, null);

            PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
            fixed (PresentModeKHR* presentModesPtr = presentModes)
            {
                KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, Surface, &presentModeCount, presentModesPtr);
            }

            Debug.WriteLine($"Vulkan found {presentModeCount} present modes");

            SurfaceFormatKHR format = formats[0];

            foreach (SurfaceFormatKHR availableFormat in formats)
            {
                if (availableFormat.Format == Format.B8G8R8A8Srgb &&
                    availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    format = availableFormat;
                    Debug.WriteLine("Vulkan found preferred format");
                    break;
                }
            }

            SwapchainFormat = format.Format;
            Debug.WriteLine($"Vulkan using {SwapchainFormat} as format");

            PresentModeKHR presentMode = PresentModeKHR.FifoKhr;

            foreach (PresentModeKHR availablePresentMode in presentModes)
            {
                if (availablePresentMode == PresentModeKHR.MailboxKhr)
                {
                    presentMode = availablePresentMode;
                    Debug.WriteLine("Vulkan found preferred present mode");
                    break;
                }
            }

            Debug.WriteLine($"Vulkan using {presentMode} as present mode");

            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                SwapchainExtent = capabilities.CurrentExtent;
            }
            else
            {
                SwapchainExtent = new Extent2D
                {
                    Width = Math.Clamp((uint)window.Size.X, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width),
                    Height = Math.Clamp((uint)window.Size.Y, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height),
                };
            }

            Debug.WriteLine($"Vulkan swap extent is {SwapchainExtent.Width}x{SwapchainExtent.Height}");

            uint imageCount = capabilities.MinImageCount + 1;
            if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
                imageCount = capabilities.MaxImageCount;

            Debug.WriteLine($"Vulkan swap chain image count is {imageCount}");

            SwapchainCreateInfoKHR swapchainCreateInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = Surface,
                MinImageCount = imageCount,
                ImageFormat = SwapchainFormat,
                ImageColorSpace = format.ColorSpace,
                ImageExtent = SwapchainExtent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
                ImageSharingMode = SharingMode.Exclusive,
                PreTransform = capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,
                OldSwapchain = default,
            };

            SwapchainKHR swapchain;
            if (KhrSwapchain.CreateSwapchain(Device, &swapchainCreateInfo, null, out swapchain) != Result.Success)
                throw new Exception("Vulkan failed to create a swap chain");
            Swapchain = swapchain;

            Debug.WriteLine("Vulkan created a swap chain successfully");

            uint swapchainImageCount = 0;
            KhrSwapchain.GetSwapchainImages(Device, Swapchain, &swapchainImageCount, null);

            SwapchainImages = new Image[swapchainImageCount];
            fixed (Image* swapchainImagesPtr = SwapchainImages)
            {
                KhrSwapchain.GetSwapchainImages(Device, Swapchain, &swapchainImageCount, swapchainImagesPtr);
            }

            Debug.WriteLine($"Vulkan retrieved {swapchainImageCount} swap chain images");

            SwapchainImageViews = new ImageView[swapchainImageCount];

            for (int i = 0; i < swapchainImageCount; i++)
            {
                ImageViewCreateInfo imageViewCreateInfo = new()
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = SwapchainImages[i],
                    ViewType = ImageViewType.Type2D,
                    Format = SwapchainFormat,
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

                if (Vk.CreateImageView(Device, &imageViewCreateInfo, null, out SwapchainImageViews[i]) != Result.Success)
                    throw new Exception($"Vulkan failed to create image view {i}");
            }

            Debug.WriteLine($"Vulkan created {swapchainImageCount} image views");

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
            if (Vk.CreateRenderPass(Device, &renderPassCreateInfo, null, &renderPass) != Result.Success)
                throw new Exception("Vulkan failed to create a render pass");
            RenderPass = renderPass;

            Debug.WriteLine("Vulkan created a render pass successfully");

            Debug.WriteLine("Vulkan successfully created");

            CreateFramebuffers();
            CreateCommandPool();    
            CreateSyncObjects();
        }

        private void CreateFramebuffers()
        {
            Framebuffers = new Framebuffer[SwapchainImageViews.Length];

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
                    if (Vk.CreateFramebuffer(Device, &framebufferCreateInfo, null, framebufferPtr) != Result.Success)
                    {
                        throw new Exception($"Vulkan failed to create framebuffer {i}");
                    }
                }
            }

            Debug.WriteLine($"Vulkan successfully created {Framebuffers.Length} framebuffers");
        }

        private void CreateCommandPool()
        {
            CommandPoolCreateInfo poolCreateInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
                QueueFamilyIndex = GraphicsQueueFamilyIndex
            };

            fixed (CommandPool* poolPtr = &CommandPool)
            {
                if (Vk.CreateCommandPool(Device, &poolCreateInfo, null, poolPtr) != Result.Success)
                    throw new Exception("Vulkan failed to create command pool");
            }

            Debug.WriteLine("Vulkan created command pool");

            CommandBuffers = new CommandBuffer[Framebuffers.Length];

            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = CommandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)CommandBuffers.Length
            };

            fixed (CommandBuffer* commandBuffersPtr = CommandBuffers)
            {
                if (Vk.AllocateCommandBuffers(Device, &allocInfo, commandBuffersPtr) != Result.Success)
                    throw new Exception("Vulkan failed to allocate command buffers");
            }

            Debug.WriteLine($"Vulkan allocated {CommandBuffers.Length} command buffers");
        }

        private void CreateSyncObjects()
        {
            ImageAvailableSemaphores = new Semaphore[MaxFramesInFlight];
            RenderFinishedSemaphores = new Semaphore[SwapchainImages.Length];
            InFlightFences = new Fence[MaxFramesInFlight];

            SemaphoreCreateInfo semaphoreInfo = new() { SType = StructureType.SemaphoreCreateInfo };
            FenceCreateInfo fenceInfo = new() { SType = StructureType.FenceCreateInfo, Flags = FenceCreateFlags.SignaledBit };

            for (int i = 0; i < MaxFramesInFlight; i++)
            {
                fixed (Semaphore* s = &ImageAvailableSemaphores[i])
                    Vk.CreateSemaphore(Device, &semaphoreInfo, null, s);
            }
            for (int i = 0; i < SwapchainImages.Length; i++)  // use SwapchainImages.Length here
            {
                fixed (Semaphore* s = &RenderFinishedSemaphores[i])
                    Vk.CreateSemaphore(Device, &semaphoreInfo, null, s);
            }

            for (int i = 0; i < MaxFramesInFlight; i++)
            {
                fixed (Fence* f = &InFlightFences[i])
                    Vk.CreateFence(Device, &fenceInfo, null, f);
            }
        }

        public unsafe void Begin(out CommandBuffer cmd, out uint imageIndex)
        {
            Vk.WaitForFences(Device, 1, ref InFlightFences[CurrentFrame], true, ulong.MaxValue);

            uint index = 0;
            KhrSwapchain.AcquireNextImage(Device, Swapchain, ulong.MaxValue,
                ImageAvailableSemaphores[CurrentFrame], default, &index);
            imageIndex = index;

            Vk.ResetFences(Device, 1, ref InFlightFences[CurrentFrame]);

            cmd = CommandBuffers[CurrentFrame];
            Vk.ResetCommandBuffer(cmd, 0);

            CommandBufferBeginInfo beginInfo = new() { SType = StructureType.CommandBufferBeginInfo };
            Vk.BeginCommandBuffer(cmd, &beginInfo);

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

            Vk.CmdBeginRenderPass(cmd, &renderPassInfo, SubpassContents.Inline);

            Viewport viewport = new() { X = 0, Y = 0, Width = SwapchainExtent.Width, Height = SwapchainExtent.Height, MinDepth = 0f, MaxDepth = 1f };
            Rect2D scissor = new() { Offset = new Offset2D(0, 0), Extent = SwapchainExtent };
            Vk.CmdSetViewport(cmd, 0, 1, &viewport);
            Vk.CmdSetScissor(cmd, 0, 1, &scissor);
        }

        public unsafe void End(CommandBuffer cmd, uint imageIndex)
        {
            Vk.CmdEndRenderPass(cmd);
            Vk.EndCommandBuffer(cmd);

            Semaphore waitSemaphore = ImageAvailableSemaphores[CurrentFrame];
            Semaphore signalSemaphore = RenderFinishedSemaphores[imageIndex];
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

            Vk.QueueSubmit(GraphicsQueue, 1, &submitInfo, InFlightFences[CurrentFrame]);

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

            KhrSwapchain.QueuePresent(GraphicsQueue, &presentInfo);
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
            Vk.AllocateCommandBuffers(Device, &allocInfo, &cmd);

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit
            };

            Vk.BeginCommandBuffer(cmd, &beginInfo);
            return cmd;
        }

        public unsafe void EndSingleTimeCommands(CommandBuffer cmd)
        {
            Vk.EndCommandBuffer(cmd);

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &cmd
            };

            Vk.QueueSubmit(GraphicsQueue, 1, &submitInfo, default);
            Vk.QueueWaitIdle(GraphicsQueue);
            Vk.FreeCommandBuffers(Device, CommandPool, 1, &cmd);
        }

        public void CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, out Silk.NET.Vulkan.Buffer buffer, out DeviceMemory memory)
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
                if (Vk.CreateBuffer(Device, &bufferCreateInfo, null, bufferPtr) != Result.Success)
                {
                    throw new Exception("Vulkan failed to create a buffer");
                }
            }

            MemoryRequirements memoryRequirments;
            Vk.GetBufferMemoryRequirements(Device, buffer, &memoryRequirments);


            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirments.Size,
                MemoryTypeIndex = FindMemoryType(memoryRequirments.MemoryTypeBits, properties),
            };

            fixed (DeviceMemory* memoryPtr = &memory)
            {
                if (Vk.AllocateMemory(Device, &memoryAllocateInfo, null, memoryPtr) != Result.Success)
                {
                    throw new Exception("Vulkan failed to allocate buffer memory");
                }
            }

            Vk.BindBufferMemory(Device, buffer, memory, 0);
        }

        private uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
        {
            PhysicalDeviceMemoryProperties memoryProperties;
            Vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, &memoryProperties);

            for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if ((typeFilter & (1 << (int)i)) != 0 &&
                    (memoryProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
                    return i;
            }

            throw new Exception("Vulkan failed to find a suitable memory type");
        }

        public void Dispose()
        {
            for (int i = 0; i < MaxFramesInFlight; i++)
            {
                Vk.DestroySemaphore(Device, RenderFinishedSemaphores[i], null);
                Vk.DestroyFence(Device, InFlightFences[i], null);
            }

            Vk.DestroyCommandPool(Device, CommandPool, null);

            foreach (var framebuffer in Framebuffers)
                Vk.DestroyFramebuffer(Device, framebuffer, null);

            Vk.DestroyRenderPass(Device, RenderPass, null);

            foreach (var imageView in SwapchainImageViews)
                Vk.DestroyImageView(Device, imageView, null);

            KhrSwapchain.DestroySwapchain(Device, Swapchain, null);
            Vk.DestroyDevice(Device, null);
            KhrSurface.DestroySurface(Instance, Surface, null);
            Vk.DestroyInstance(Instance, null);
            Vk.Dispose();
        }
    }
}