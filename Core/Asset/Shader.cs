using Silk.NET.Vulkan;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Xirface
{
    public unsafe class Shader
    {
        private GraphicsManager graphics;
        private ShaderModule vertexModule, fragmentModule;
        private DescriptorSetLayout descriptorSetLayout;
        private DescriptorPool descriptorPool;
        private Type vertexType;

        public Pipeline Pipeline;
        public PipelineLayout PipelineLayout;

        public Shader(GraphicsManager graphics)
        {
            this.graphics = graphics;
        }

        public void Buffer(string vertexPath, string fragmentPath, Type vertexType)
        {
            this.vertexType = vertexType;

            vertexModule = CreateShaderModule(vertexPath);
            fragmentModule = CreateShaderModule(fragmentPath);

            descriptorSetLayout = CreateDescriptorSetLayout(vertexType);
            PipelineLayout = CreatePipelineLayout();
            Pipeline = CreatePipeline(vertexType);

            descriptorPool = CreateDescriptorPool(vertexType);
        }
        public void CreateUniformBuffer(out Buffer buffer, out DeviceMemory memory, out void* mapped)
        {
            graphics.CreateBuffer(
                (ulong)sizeof(Transform),
                BufferUsageFlags.UniformBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out buffer,
                out memory
            );

            void* m;
            graphics.Vulkan!.MapMemory(graphics.Device, memory, 0, (ulong)sizeof(Transform), 0, &m);
            mapped = m;
        }

        public DescriptorSet AllocateDescriptorSet(Buffer uniformBuffer)
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
                    throw new Exception("VULKAN: Failed to allocate a descriptor set");
            }

            DescriptorBufferInfo bufferInfo = new()
            {
                Buffer = uniformBuffer,
                Offset = 0,
                Range = (ulong)sizeof(Transform),
            };

            WriteDescriptorSet write = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 0,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &bufferInfo,
            };

            graphics.Vulkan.UpdateDescriptorSets(graphics.Device, 1, &write, 0, null);

            return descriptorSet;
        }

        public void SetTexture(DescriptorSet descriptorSet, Texture2D texture)
        {
            graphics.Vulkan.DeviceWaitIdle(graphics.Device);

            DescriptorImageInfo imageInfo = new()
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                ImageView = texture.ImageView,
                Sampler = texture.Sampler,
            };

            WriteDescriptorSet write = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 1,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                PImageInfo = &imageInfo
            };

            graphics.Vulkan.UpdateDescriptorSets(graphics.Device, 1, &write, 0, null);
        }

        public void Apply(CommandBuffer cmd, DescriptorSet descriptorSet)
        {
            graphics.Vulkan.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, Pipeline);

            graphics.Vulkan.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, PipelineLayout, 0, 1, &descriptorSet, 0, null);
        }

        private ShaderModule CreateShaderModule(string path)
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
                    throw new Exception($"Vulkan failed to create a shader module from {path}");
            }
            return module;
        }

        private DescriptorSetLayout CreateDescriptorSetLayout(Type vertexType)
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

            if (vertexType == typeof(VertexPositionColorTexture))
            {
                bindings[1] = new DescriptorSetLayoutBinding
                {
                    Binding = 1,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 1,
                    StageFlags = ShaderStageFlags.FragmentBit,
                };
            }

            fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
            {
                uint bindingCount = vertexType != typeof(VertexPositionColorTexture) ? 1u : 2u;

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

        private PipelineLayout CreatePipelineLayout()
        {
            PipelineLayout pipelineLayout;

            PushConstantRange pushConstantRange = new()
            {
                StageFlags = ShaderStageFlags.VertexBit,
                Offset = 0,
                Size = (uint)sizeof(Matrix4x4)
            };

            fixed (DescriptorSetLayout* layoutPtr = &descriptorSetLayout)
            {
                PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    SetLayoutCount = 1,
                    PSetLayouts = layoutPtr,
                    PushConstantRangeCount = 1,
                    PPushConstantRanges = &pushConstantRange
                };

                if (graphics.Vulkan!.CreatePipelineLayout(graphics.Device, &pipelineLayoutCreateInfo, null, &pipelineLayout) != Result.Success)
                    throw new Exception("Vulkan failed to create pipeline layout");
            }

            return pipelineLayout;
        }

        private Pipeline CreatePipeline(Type vertexType)
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

                    PipelineDepthStencilStateCreateInfo depthStencil = new()
                    {
                        SType = StructureType.PipelineDepthStencilStateCreateInfo,
                        DepthTestEnable = false,
                        DepthWriteEnable = false,
                        DepthCompareOp = CompareOp.Less,
                        DepthBoundsTestEnable = false,
                        StencilTestEnable = false,
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
                            PDepthStencilState = &depthStencil,
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

            graphics.Vulkan.DestroyShaderModule(graphics.Device, vertexModule, null);
            graphics.Vulkan.DestroyShaderModule(graphics.Device, fragmentModule, null);

            return pipeline;
        }

        private DescriptorPool CreateDescriptorPool(Type vertexType)
        {
            DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[2];
            DescriptorPool descriptorPool;

            poolSizes[0] = new DescriptorPoolSize()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = 100,
            };

            if (vertexType == typeof(VertexPositionColorTexture))
            {
                poolSizes[1] = new DescriptorPoolSize()
                {
                    Type = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 100,
                };
            }

            fixed (DescriptorPoolSize* poolSizePtr = poolSizes)
            {
                uint poolSizeCount = vertexType != typeof(VertexPositionColorTexture) ? 1u : 2u;

                DescriptorPoolCreateInfo poolCreateInfo = new()
                {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = poolSizeCount,
                    PPoolSizes = poolSizePtr,
                    MaxSets = 100,
                };

                if (graphics.Vulkan!.CreateDescriptorPool(graphics.Device, &poolCreateInfo, null, &descriptorPool) != Result.Success)
                    throw new Exception("VULKAN: Failed to create a descriptor pool");
            }

            return descriptorPool;
        }

        public void Dispose()
        {
            graphics.Vulkan.DestroyDescriptorPool(graphics.Device, descriptorPool, null);
            graphics.Vulkan.DestroyDescriptorSetLayout(graphics.Device, descriptorSetLayout, null);
            graphics.Vulkan.DestroyPipeline(graphics.Device, Pipeline, null);
            graphics.Vulkan.DestroyPipelineLayout(graphics.Device, PipelineLayout, null);
        }
    }
}