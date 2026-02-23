using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Buffer = Silk.NET.Vulkan.Buffer;

namespace Xirface
{
    public enum ShaderMode
    {
        Texture,
        Vertex,
        Absolute
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Transform
    {
        public Matrix4x4 World;
        public Matrix4x4 View;
        public Matrix4x4 Projection;
    }


    public unsafe class Shader<TVertex> where TVertex : unmanaged, IVertex
    {
        private Matrix4x4 view = Matrix4x4.Identity;
        private Matrix4x4 world = Matrix4x4.Identity;
        private Matrix4x4 projection = Matrix4x4.Identity;

        private Texture2D? texture;

        private Vulkan graphics;
        private ShaderMode mode;

        private ShaderModule vertexModule, fragmentModule;

        private DescriptorSetLayout descriptorSetLayout;
        private DescriptorPool descriptorPool;
        public DescriptorSet DescriptorSet;
        public Pipeline Pipeline;
        public PipelineLayout PipelineLayout;

        
        private Buffer transformBuffer;
        private DeviceMemory transformMemory;
        private void* transformMapped;

        private Buffer colorBuffer;
        private DeviceMemory colorMemory;
        private void* colorMapped;
        public Shader(Vulkan graphics, string vertexPath, string fragmentPath, ShaderMode mode)
        {
            this.graphics = graphics;
            this.mode = mode;

            vertexModule = CreateShaderModule(vertexPath);
            fragmentModule = CreateShaderModule(fragmentPath);

            CreateDescriptorSetLayout();
            CreatePipelineLayout();
            CreatePipeline();
            CreateUniformBuffers();
            CreateDescriptorPool();
            CreateDescriptorSet();
        }

        public void SetView(Matrix4x4 value) { view = value; UpdateTransform();}
        public void SetWorld(Matrix4x4 value) { world = value; UpdateTransform(); }
        public void SetProjection(Matrix4x4 value) { projection = value; UpdateTransform(); }

        public void SetTexture(Texture2D texture)
        {
            if (mode == ShaderMode.Texture && this.texture != texture)
            {
                this.texture = texture;

                DescriptorImageInfo imageInfo = new()
                {
                    ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                    //ImageView = texture.ImageView;
                    //Sampler = texture.Sampler;
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

                graphics.Vk.UpdateDescriptorSets(graphics.Device, 1, &write, 0, null);
            }
        }
       
        public void SetColor(Color value)
        { 
            if (mode == ShaderMode.Absolute)
            {
                Vector4 color = new(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
                System.Buffer.MemoryCopy(&color,colorMapped, sizeof(Vector4), sizeof(Vector4));
            }
        
        
        }

        public void Apply(CommandBuffer cmd)
        {
            graphics.Vk.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, Pipeline);

            fixed (DescriptorSet* setPtr = &DescriptorSet)
            {
                graphics.Vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, PipelineLayout, 0, 1, setPtr, 0, null);
            }
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
                if (graphics.Vk.CreateShaderModule(graphics.Device, &createInfo, null, &module) != Result.Success)
                {
                    throw new Exception($"Vulkan failed to create a shader module from {path}");
                }
                    
            }
            return module;
        }

        private void UpdateTransform()
        {
            Transform transform = new()
            {
                View = view,
                World = world,
                Projection = projection,
            };

            System.Buffer.MemoryCopy(&transform, transformMapped, sizeof(Transform), sizeof(Transform));
        }

        

        private void CreateDescriptorSetLayout()
        {
            DescriptorSetLayoutBinding[] bindings = new DescriptorSetLayoutBinding[2];
            
            bindings[0] =  new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.VertexBit,
            };

            switch (mode)
            {
                case (ShaderMode.Absolute):

                    bindings[1] = (new DescriptorSetLayoutBinding
                    {
                        Binding = 1,
                        DescriptorType = DescriptorType.UniformBuffer,
                        DescriptorCount = 1,
                        StageFlags = ShaderStageFlags.FragmentBit
                    });

                    break;
                case (ShaderMode.Texture):

                    bindings[1] = (new DescriptorSetLayoutBinding
                    {
                        Binding = 1,
                        DescriptorType = DescriptorType.CombinedImageSampler,
                        DescriptorCount = 1,
                        StageFlags = ShaderStageFlags.FragmentBit
                    });

                    break;
            }

            fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
            {
                uint bindingCount = (mode == ShaderMode.Vertex) ? (uint)1 : 2;

                DescriptorSetLayoutCreateInfo layoutCreateInfo = new()
                {
                    SType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = bindingCount,
                    PBindings = bindingsPtr
                };

                fixed (DescriptorSetLayout* layoutPtr = &descriptorSetLayout)
                {
                    if (graphics.Vk.CreateDescriptorSetLayout(graphics.Device, &layoutCreateInfo, null, layoutPtr) != Result.Success)
                        throw new Exception("Vulkan failed to create descriptor set layout");
                }

            }
        }

        private void CreatePipelineLayout()
        {
            fixed (DescriptorSetLayout* layoutPtr = &descriptorSetLayout)
            {
                PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    SetLayoutCount = 1,
                    PSetLayouts = layoutPtr
                };

                fixed (PipelineLayout* pipelineLayoutPtr = &PipelineLayout)
                {
                    if (graphics.Vk.CreatePipelineLayout(graphics.Device, &pipelineLayoutCreateInfo, null, pipelineLayoutPtr) != Result.Success)
                        throw new Exception("Vulkan failed to create pipeline layout");
                }
            }
        }

        private void CreatePipeline()
        {
            PipelineShaderStageCreateInfo vertexStage = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vertexModule,
                PName = (byte*)Marshal.StringToHGlobalAnsi("main")
            };

            PipelineShaderStageCreateInfo fargmentStage = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = fragmentModule,
                PName = (byte*)Marshal.StringToHGlobalAnsi("main")
            };

            PipelineShaderStageCreateInfo[] stages = {vertexStage, fargmentStage};

            VertexInputBindingDescription bindingDescription = TVertex.Binding();
            VertexInputAttributeDescription[] attributeDescription = TVertex.Attributes();

            fixed (VertexInputAttributeDescription* attributesPtr = attributeDescription)
            {
                PipelineVertexInputStateCreateInfo vertexInputInfo = new()
                {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexBindingDescriptionCount = 1,
                    PVertexBindingDescriptions = &bindingDescription,
                    VertexAttributeDescriptionCount = (uint)attributeDescription.Length,
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

                        fixed(Pipeline* pipelinePtr = &Pipeline)
                        {
                            if (graphics.Vk.CreateGraphicsPipelines(graphics.Device, default, 1, &pipelineCreateInfo, null, pipelinePtr) != Result.Success)
                            {
                                throw new Exception("Vulkan failed to create a graphics pipeline");
                            }
                        }
                    }
                }
               
            }

            graphics.Vk.DestroyShaderModule(graphics.Device, vertexModule, null);
            graphics.Vk.DestroyShaderModule(graphics.Device, fragmentModule, null);
        }
        

        private void CreateUniformBuffers()
        {
            graphics.CreateBuffer(
                (ulong)sizeof(Transform),
                BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, 
                out transformBuffer,
                out transformMemory
            );

            void* mapped;

            graphics.Vk.MapMemory(graphics.Device, transformMemory, 0, (ulong)sizeof(Transform), 0, &mapped);
            transformMapped = mapped;

            if (mode == ShaderMode.Absolute)
            {
                graphics.CreateBuffer(
                    (ulong)sizeof(Vector4),
                    BufferUsageFlags.UniformBufferBit,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, 
                    out colorBuffer,
                    out colorMemory
                );

                graphics.Vk.MapMemory(graphics.Device, colorMemory, 0, (ulong)sizeof(Vector4), 0, &mapped);
                colorMapped = mapped;
            }

        }

        private void CreateDescriptorPool()
        {
            DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[2];

            poolSizes[0] = new DescriptorPoolSize()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = mode == ShaderMode.Absolute ? 2u : 1u
            };

            if (mode == ShaderMode.Texture)
            {
                poolSizes[1] = new DescriptorPoolSize()
                {
                    Type = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 1,
                };
            }

            fixed (DescriptorPoolSize* poolSizePtr = poolSizes)
            {
                uint poolSizeCount = mode == ShaderMode.Vertex ? 1u : 2u;

                DescriptorPoolCreateInfo poolCreateInfo = new()
                {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = poolSizeCount,
                    PPoolSizes = poolSizePtr,
                    MaxSets = 1,
                };

                fixed (DescriptorPool* poolPtr = &descriptorPool)
                {
                    if (graphics.Vk.CreateDescriptorPool(graphics.Device, &poolCreateInfo, null, poolPtr) != Result.Success)
                    {
                        throw new Exception("Vulkan failed to create a descriptor pool");
                    }
                }
            }
        }

        private void CreateDescriptorSet()
        {
            fixed (DescriptorSetLayout* layoutPtr = &descriptorSetLayout)
            {
                DescriptorSetAllocateInfo allocInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = descriptorPool,
                    DescriptorSetCount = 1,
                    PSetLayouts = layoutPtr,
                };

                fixed (DescriptorSet* setPtr = &DescriptorSet)
                {
                    if (graphics.Vk.AllocateDescriptorSets(graphics.Device, &allocInfo, setPtr) != Result.Success)
                    {
                        throw new Exception("Vulkan failed to allocate a descritor set");
                    }
                }
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
                DstSet = DescriptorSet,
                DstBinding = 0,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &transformBufferInfo,
            };

            graphics.Vk.UpdateDescriptorSets(graphics.Device, 1, &transformWrite, 0, null);

            if (mode == ShaderMode.Absolute)
            {
                DescriptorBufferInfo colorBufferInfo = new()
                {
                    Buffer = colorBuffer,
                    Offset = 0,
                    Range = (ulong)sizeof(Vector4),
                };

                WriteDescriptorSet colorWrite = new()
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = DescriptorSet,
                    DstBinding = 1,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.UniformBuffer,
                    DescriptorCount = 1,
                    PBufferInfo = &colorBufferInfo,
                };

                graphics.Vk.UpdateDescriptorSets(graphics.Device, 1, &colorWrite, 0, null);
            }
        }

        

        public void Dispose()
        {
            graphics.Vk.UnmapMemory(graphics.Device, transformMemory);
            graphics.Vk.DestroyBuffer(graphics.Device, transformBuffer, null);
            graphics.Vk.FreeMemory(graphics.Device, transformMemory, null);

            if (mode == ShaderMode.Absolute)
            {
                graphics.Vk.UnmapMemory(graphics.Device, colorMemory);
                graphics.Vk.DestroyBuffer(graphics.Device, colorBuffer, null);
                graphics.Vk.FreeMemory(graphics.Device, colorMemory, null);
            }

            graphics.Vk.DestroyDescriptorPool(graphics.Device, descriptorPool, null);
            graphics.Vk.DestroyDescriptorSetLayout(graphics.Device, descriptorSetLayout, null);
            graphics.Vk.DestroyPipeline(graphics.Device, Pipeline, null);
            graphics.Vk.DestroyPipelineLayout(graphics.Device, PipelineLayout, null);
        }
    }
}
