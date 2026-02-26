global using Buffer = Silk.NET.Vulkan.Buffer;
global using Semaphore = Silk.NET.Vulkan.Semaphore;
global using Vulkan = Silk.NET.Vulkan.Vk;

namespace Xirface
{
    public partial class AssetManager { }
    public partial class InterfaceManager { }
    public partial class GraphicsManager { }
    public partial class InputManager { }
    public struct Color
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public Color(byte r, byte g, byte b, byte a)
        {
            R = MathF.Pow(r / 255.0f, 2.2f);
            G = MathF.Pow(g / 255.0f, 2.2f);
            B = MathF.Pow(b / 255.0f, 2.2f);
            A = a / 255.0f;
        }

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Black => new Color(0, 0, 0, 255);
        public static Color White => new Color(255, 255, 255, 255);
        public static Color Red => new Color(255, 0, 0, 255);
        public static Color Green => new Color(0, 128, 0, 255);
        public static Color Blue => new Color(0, 0, 255, 255);
        public static Color Yellow => new Color(255, 255, 0, 255);
        public static Color Cyan => new Color(0, 255, 255, 255);
        public static Color Magenta => new Color(255, 0, 255, 255);
        public static Color Orange => new Color(255, 165, 0, 255);
        public static Color Purple => new Color(128, 0, 128, 255);
        public static Color Pink => new Color(255, 192, 203, 255);
        public static Color Brown => new Color(139, 69, 19, 255);
        public static Color Gray => new Color(128, 128, 128, 255);
        public static Color LightGray => new Color(211, 211, 211, 255);
        public static Color DarkGray => new Color(64, 64, 64, 255);
        public static Color Lime => new Color(0, 255, 0, 255);
        public static Color Transparent => new Color(0, 0, 0, 0);
    }
}
