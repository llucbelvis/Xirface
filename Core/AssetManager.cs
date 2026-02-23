using Silk.NET.Vulkan;

namespace Xirface
{
    public class AssetManager
    {
        public Dictionary<string, object> assets;
        private Vulkan graphics;

        public AssetManager(Vulkan graphics)
        {
            assets = new();
            this.graphics = graphics;

            
        }

        public T Load<T>(string path) where T : class
        {
            if (assets.TryGetValue(path, out var cached)) return (cached as T)!;

            if (typeof(T) == typeof(Texture2D))
            {
                Texture2D texture = new Texture2D(path);
                assets[path] = texture;
                return (texture as T)!;
            }

            if (typeof(T) == typeof(Font))
            {
                Font font = new Font(path);
                assets[path] = font;
                return (font as T)!;
            }

            throw new Exception($"Unknown asset type");
        }

        public T Load<T>(string vertex, string fragment) where T : class
        {
            if (assets.TryGetValue(fragment, out var cached)) return (cached as T)!;

            if (typeof(T) == typeof(Shader<VertexPositionColor>))
            {
                Shader<VertexPositionColor> shader = new Shader<VertexPositionColor>(graphics, vertex, fragment, ShaderMode.Vertex);
                assets[fragment] = shader;
                return (shader as T)!;
            }

            throw new Exception($"Unknown asset type");
        }
    }
}