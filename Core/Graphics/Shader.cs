using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Xirface
{
    [StructLayout(LayoutKind.Sequential)]
    struct Transform
    {
        public Matrix4x4 World;
        public Matrix4x4 View;
        public Matrix4x4 Projection;
    }


    public unsafe class Shader 
    {
        private Transform transform;

        private Texture2D? texture;

        private GraphicsManager graphics;

        private ShaderModule vertexModule, fragmentModule;

        private DescriptorSetLayout descriptorSetLayout;
        private DescriptorPool descriptorPool;

        public DescriptorSet DescriptorSet;
        public Pipeline Pipeline;
        public PipelineLayout PipelineLayout;


        private Buffer transformBuffer;
        private DeviceMemory transformMemory;
        private void* transformMapped;

        public Shader(GraphicsManager graphics, string vertexPath, string fragmentPath, Type vertexType)
        {
            this.graphics = graphics;

            transform = new();
            vertexModule = CreateShaderModule(vertexPath);
            fragmentModule = CreateShaderModule(fragmentPath);

            descriptorSetLayout = CreateDescriptorSetLayout();
            PipelineLayout = CreatePipelineLayout();
            Pipeline = CreatePipeline(vertexType);

            CreateUniformBuffers();

            descriptorPool = CreateDescriptorPool();
            DescriptorSet = CreateDescriptorSet();

            SetTexture(new Texture2D(graphics, "textures\\trains.png"));
        }

        public void SetView(Matrix4x4 value) { transform.View = value; UpdateTransform(); }
        public void SetWorld(Matrix4x4 value) { transform.World = value; UpdateTransform(); }
        public void SetProjection(Matrix4x4 value) { transform.Projection = value; UpdateTransform(); }

        public unsafe void SetTexture(Texture2D texture)
        {
            this.texture = texture;

            DescriptorImageInfo imageInfo = new()
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                ImageView = texture.ImageView,
                Sampler = texture.Sampler,
            };

            WriteDescriptorSet write = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = DescriptorSet,
                DstBinding = 1,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                PImageInfo = &imageInfo
            };

            graphics.Vulkan.UpdateDescriptorSets(graphics.Device, 1, &write, 0, null);
        }

        public unsafe void Apply(CommandBuffer cmd)
        {
            graphics.Vulkan.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, Pipeline);

            fixed (DescriptorSet* setPtr = &DescriptorSet)
            {
                graphics.Vulkan.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, PipelineLayout, 0, 1, setPtr, 0, null);
            }
        }

        private unsafe ShaderModule CreateShaderModule(string path)
        {
            byte[] spv = File.ReadAllBytes(path);
            ShaderModuleCreateInfo createInfo = new()
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)spv.Length,
            };
            ShaderModule module;
            fixed (byte* spvPtr = spv)
            {
                createInfo.PCode = (uint*)spvPtr;
                if (graphics.Vulkan.CreateShaderModule(graphics.Device, &createInfo, null, &module) != Result.Success)
                {
                    throw new Exception($"Vulkan failed to create a shader module from {path}");
                }

            }
            return module;
        }

        private unsafe void UpdateTransform()
        {
            Transform transform = new()
            {
                View = this.transform.View,
                World = this.transform.World,
                Projection = this.transform.Projection,
            };

            System.Buffer.MemoryCopy(&transform, transformMapped, sizeof(Transform), sizeof(Transform));
        }

        private DescriptorSetLayout CreateDescriptorSetLayout()
        {
            DescriptorSetLayout descriptorSetLayout;

            DescriptorSetLayoutBinding[] bindings = new DescriptorSetLayoutBinding[2];

            bindings[0] = new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.VertexBit,
            };

            bindings[1] = (new DescriptorSetLayoutBinding
            {
                Binding = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.FragmentBit
            });


            fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
            {
                uint bindingCount = 2;

                DescriptorSetLayoutCreateInfo layoutCreateInfo = new()
                {
                    SType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = bindingCount,
                    PBindings = bindingsPtr
                };

                if (graphics.Vulkan!.CreateDescriptorSetLayout(graphics.Device, &layoutCreateInfo, null, &descriptorSetLayout) != Result.Success)
                    throw new Exception("Vulkan failed to create descriptor set layout");
            }

            return descriptorSetLayout;
        }

        private unsafe PipelineLayout CreatePipelineLayout()
        {
            PipelineLayout pipelineLayout;

            fixed (DescriptorSetLayout* layoutPtr = &descriptorSetLayout)
            {
                PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    SetLayoutCount = 1,
                    PSetLayouts = layoutPtr
                };

                if (graphics.Vulkan!.CreatePipelineLayout(graphics.Device, &pipelineLayoutCreateInfo, null, &pipelineLayout) != Result.Success)
                    throw new Exception("Vulkan failed to create pipeline layout");
            }

            return pipelineLayout;
        }

        private unsafe Pipeline CreatePipeline(Type vertexType)
        {
            Pipeline pipeline;

            PipelineShaderStageCreateInfo vertexStage = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vertexModule,
                PName = (byte*)Marshal.StringToHGlobalAnsi("main")
            };

            PipelineShaderStageCreateInfo fragmentStage = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = fragmentModule,
                PName = (byte*)Marshal.StringToHGlobalAnsi("main")
            };

            PipelineShaderStageCreateInfo[] stages = { vertexStage, fragmentStage };

            var bindingDescription = (VertexInputBindingDescription)vertexType
                .GetMethod("Binding")!.Invoke(null, null)!;

            var attributesDescription = (VertexInputAttributeDescription[])vertexType
                .GetMethod("Attributes")!.Invoke(null, null)!;

            fixed (VertexInputAttributeDescription* attributesPtr = attributesDescription)
            {
                PipelineVertexInputStateCreateInfo vertexInputInfo = new()
                {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexBindingDescriptionCount = 1,
                    PVertexBindingDescriptions = &bindingDescription,
                    VertexAttributeDescriptionCount = (uint)attributesDescription.Length,
                    PVertexAttributeDescriptions = attributesPtr,
                };

                PipelineInputAssemblyStateCreateInfo inputAssembly = new()
                {
                    SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                    Topology = PrimitiveTopology.TriangleList,
                    PrimitiveRestartEnable = false,
                };

                DynamicState[] dynamicsStates = { DynamicState.Viewport, DynamicState.Scissor };

                fixed (DynamicState* dynamicsStatesPtr = dynamicsStates)
                {
                    PipelineDynamicStateCreateInfo dynamicState = new()
                    {
                        SType = StructureType.PipelineDynamicStateCreateInfo,
                        DynamicStateCount = (uint)dynamicsStates.Length,
                        PDynamicStates = dynamicsStatesPtr,
                    };

                    PipelineViewportStateCreateInfo viewportState = new()
                    {
                        SType = StructureType.PipelineViewportStateCreateInfo,
                        ViewportCount = 1,
                        ScissorCount = 1,
                    };

                    PipelineRasterizationStateCreateInfo rasterizer = new()
                    {
                        SType = StructureType.PipelineRasterizationStateCreateInfo,
                        DepthClampEnable = false,
                        RasterizerDiscardEnable = false,
                        PolygonMode = PolygonMode.Fill,
                        LineWidth = 1,
                        CullMode = CullModeFlags.None,
                        FrontFace = FrontFace.Clockwise,
                        DepthBiasEnable = false,
                    };

                    PipelineMultisampleStateCreateInfo multisampling = new()
                    {
                        SType = StructureType.PipelineMultisampleStateCreateInfo,
                        SampleShadingEnable = false,
                        RasterizationSamples = SampleCountFlags.Count1Bit,
                    };

                    PipelineColorBlendAttachmentState colorBlendAttachment = new()
                    {
                        ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                                         ColorComponentFlags.BBit | ColorComponentFlags.ABit,
                        BlendEnable = true,
                        SrcColorBlendFactor = BlendFactor.SrcAlpha,
                        DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                        ColorBlendOp = BlendOp.Add,
                        SrcAlphaBlendFactor = BlendFactor.One,
                        DstAlphaBlendFactor = BlendFactor.Zero,
                        AlphaBlendOp = BlendOp.Add,
                    };

                    PipelineColorBlendStateCreateInfo colorBlending = new()
                    {
                        SType = StructureType.PipelineColorBlendStateCreateInfo,
                        LogicOpEnable = false,
                        AttachmentCount = 1,
                        PAttachments = &colorBlendAttachment,
                    };

                    fixed (PipelineShaderStageCreateInfo* stagesPtr = stages)
                    {
                        GraphicsPipelineCreateInfo pipelineCreateInfo = new()
                        {
                            SType = StructureType.GraphicsPipelineCreateInfo,
                            StageCount = 2,
                            PStages = stagesPtr,
                            PVertexInputState = &vertexInputInfo,
                            PInputAssemblyState = &inputAssembly,
                            PViewportState = &viewportState,
                            PRasterizationState = &rasterizer,
                            PMultisampleState = &multisampling,
                            PColorBlendState = &colorBlending,
                            PDynamicState = &dynamicState,
                            Layout = PipelineLayout,
                            RenderPass = graphics.RenderPass,
                            Subpass = 0,
                        };

                        if (graphics.Vulkan!.CreateGraphicsPipelines(graphics.Device, default, 1, &pipelineCreateInfo, null, &pipeline) != Result.Success)
                            throw new Exception("VULKAN: Failed to create a graphics pipeline");
                    }


                }

            }

            graphics!.Vulkan.DestroyShaderModule(graphics.Device, vertexModule, null);
            graphics.Vulkan.DestroyShaderModule(graphics.Device, fragmentModule, null);

            return pipeline;
        }


        private unsafe void CreateUniformBuffers()
        {
            graphics.CreateBuffer(
                (ulong)sizeof(Transform),
                BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out transformBuffer,
                out transformMemory
            );

            void* mapped;

            graphics.Vulkan!.MapMemory(graphics.Device, transformMemory, 0, (ulong)sizeof(Transform), 0, &mapped);
            transformMapped = mapped;
        }

        private unsafe DescriptorPool CreateDescriptorPool()
        {
            DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[2];
            DescriptorPool descriptorPool;

            poolSizes[0] = new DescriptorPoolSize()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
            };

            poolSizes[1] = new DescriptorPoolSize()
            {
                Type = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
            };


            fixed (DescriptorPoolSize* poolSizePtr = poolSizes)
            {
                uint poolSizeCount = 2;

                DescriptorPoolCreateInfo poolCreateInfo = new()
                {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = poolSizeCount,
                    PPoolSizes = poolSizePtr,
                    MaxSets = 1,
                };

                if (graphics.Vulkan!.CreateDescriptorPool(graphics.Device, &poolCreateInfo, null, &descriptorPool) != Result.Success)
                {
                    throw new Exception("VULKAN: Failed to create a descriptor pool");
                }
            }

            return descriptorPool;
        }

        private unsafe DescriptorSet CreateDescriptorSet()
        {
            DescriptorSet descriptorSet;

            fixed (DescriptorSetLayout* layoutPtr = &descriptorSetLayout)
            {
                DescriptorSetAllocateInfo allocInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = descriptorPool,
                    DescriptorSetCount = 1,
                    PSetLayouts = layoutPtr,
                };

                if (graphics.Vulkan!.AllocateDescriptorSets(graphics.Device, &allocInfo, &descriptorSet) != Result.Success)
                    throw new Exception("VULKAN: Failed to allocate a descritor set");

            }

            DescriptorBufferInfo transformBufferInfo = new()
            {
                Buffer = transformBuffer,
                Offset = 0,
                Range = (ulong)sizeof(Transform),
            };

            WriteDescriptorSet transformWrite = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 0,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &transformBufferInfo,
            };

            graphics.Vulkan.UpdateDescriptorSets(graphics.Device, 1, &transformWrite, 0, null);

            return descriptorSet;
        }

        public unsafe void Dispose()
        {
            graphics.Vulkan!.UnmapMemory(graphics.Device, transformMemory);
            graphics.Vulkan.DestroyBuffer(graphics.Device, transformBuffer, null);
            graphics.Vulkan.FreeMemory(graphics.Device, transformMemory, null);

            graphics.Vulkan.DestroyDescriptorPool(graphics.Device, descriptorPool, null);
            graphics.Vulkan.DestroyDescriptorSetLayout(graphics.Device, descriptorSetLayout, null);
            graphics.Vulkan.DestroyPipeline(graphics.Device, Pipeline, null);
            graphics.Vulkan.DestroyPipelineLayout(graphics.Device, PipelineLayout, null);
        }
    }
}