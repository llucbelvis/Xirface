using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace Xirface
{
    public interface IVertex
    {
        static abstract VertexInputBindingDescription Binding();
        static abstract VertexInputAttributeDescription[] Attributes();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor : IVertex
    {
        public Vector3 Position;  // 12 bytes
        public Color Color;       // 16 bytes

        public VertexPositionColor(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public static VertexInputBindingDescription Binding() => new()
        {
            Binding = 0,
            Stride = 28,
            InputRate = VertexInputRate.Vertex
        };

        public static VertexInputAttributeDescription[] Attributes() => new[]
        {
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 0,
                Format = Format.R32G32B32Sfloat,
                Offset = 0
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 1,
                Format = Format.R32G32B32A32Sfloat,
                Offset = 12
            }
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColorTexture : IVertex
    {
        public Vector3 Position;  // 12 bytes
        public Color Color;       // 16 bytes
        public Vector2 TexCoord;  // 8 bytes 

        public VertexPositionColorTexture(Vector3 position, Color color, Vector2 texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }

        public static VertexInputBindingDescription Binding() => new()
        {
            Binding = 0,
            Stride = 36,
            InputRate = VertexInputRate.Vertex
        };

        public static VertexInputAttributeDescription[] Attributes() => new[]
        {
                new VertexInputAttributeDescription
                {
                    Binding = 0, Location = 0,
                    Format = Format.R32G32B32Sfloat,
                    Offset = 0
                },
                new VertexInputAttributeDescription
                {
                    Binding = 0, Location = 1,
                    Format = Format.R32G32B32A32Sfloat,
                    Offset = 12
                },
                new VertexInputAttributeDescription
                {
                    Binding = 0, Location = 2,
                    Format = Format.R32G32Sfloat,
                    Offset = 28
                }
        };
    }

    public struct VertexPositionColorFont : IVertex
    {
        public Vector3 Position;  // 12 bytes
        public Color Color;       // 16 bytes                      
        public Vector2 TexCoord;  // 8 bytes
        public short Side; // 2 bytes
        public short Curved;   // 2 bytes

        public VertexPositionColorFont(Vector3 position, Color color, Vector2 texCoord, short side, short curved)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
            Side = side;
            Curved = curved;
        }

        public static VertexInputBindingDescription Binding() => new()
        {
            Binding = 0,
            Stride = 40,
            InputRate = VertexInputRate.Vertex
        };

        public static VertexInputAttributeDescription[] Attributes() => new[]
        {
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 0,
                Format = Format.R32G32B32Sfloat,
                Offset = 0
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 1,
                Format = Format.R32G32B32A32Sfloat,
                Offset = 12
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 2,
                Format = Format.R32G32Sfloat,
                Offset = 28
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 3,
                Format = Format.R16Sint,
                Offset = 36
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 4,
                Format = Format.R16Sint,
                Offset = 38
            }
        };
    }


}




