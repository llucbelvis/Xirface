using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
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
            public Color Color;       // 4 bytes

            public VertexPositionColor(Vector3 position, Color color)
            {
                Position = position;
                Color = color;
            }

            public static VertexInputBindingDescription Binding() => new()
            {
                Binding = 0,
                Stride = 16,
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
                Format = Format.R8G8B8A8Unorm,
                Offset = 12
            }
        };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexPositionColorTexture : IVertex
        {
            public Vector3 Position;  // 12 bytes
            public Color Color;       // 4 bytes
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
                Stride = 24,
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
                    Format = Format.R8G8B8A8Unorm,
                    Offset = 12
                },
                new VertexInputAttributeDescription
                {
                    Binding = 0, Location = 2,
                    Format = Format.R32G32Sfloat,
                    Offset = 16
                }
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexFont : IVertex
        {
            public Vector3 Position;  // 12 bytes
            public Color Color;       // 4 bytes
            public Vector2 TexCoord;  // 8 bytes
            public short Curve;       // 2 bytes
            public short Side;        // 2 bytes 

            public VertexFont(Vector3 position, Color color, Vector2 texCoord, short curve, short side)
            {
                Position = position;
                Color = color;
                TexCoord = texCoord;
                Curve = curve;
                Side = side;
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
                Format = Format.R8G8B8A8Unorm,
                Offset = 12
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 2,
                Format = Format.R32G32Sfloat,
                Offset = 16
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 3,
                Format = Format.R16Sint,
                Offset = 24
            },
            new VertexInputAttributeDescription
            {
                Binding = 0, Location = 4,
                Format = Format.R16Sint,
                Offset = 26
            }
        };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    

    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Black() => new Color(0,0,0,255);
        public static Color White() => new Color(255, 255, 255, 255);
    }

