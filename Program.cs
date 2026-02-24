using Silk.NET.Windowing;
using Silk.NET.Vulkan;
using Silk.NET.Input;
using System.Numerics;
using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Vulkan.Extensions.KHR;

new Xirface.Main();

namespace Xirface
{
    public class Main
    {
        private GraphicsManager? graphicsManager;
        private AssetManager? assetManager;
        private InputManager? inputManager;
        private InterfaceManager? interfaceManager;
        

        public Main()
        {
            var options = WindowOptions.DefaultVulkan with
            {
                Size = new(1366, 768),
                Title = "Xirface",
                API = new GraphicsAPI(ContextAPI.Vulkan, ContextProfile.Core, ContextFlags.Default, new APIVersion(1, 2))
            };

            var window = Window.Create(options);

            window.Load += () =>
            {
                graphicsManager = new GraphicsManager(window);
                assetManager = new AssetManager(graphicsManager);
                //assetManager.Load<Shader>("shaders\\vertex\\positioncolortexture.vert.spv", "shaders\\fragment\\texture.frag.spv", typeof(VertexPositionColorTexture));
            };

            window.Update += delta =>
            {
            };

            window.Render += delta =>
            {
                graphicsManager!.Begin(out var cmd, out var imageIndex);


                //Entity e = new(assetManager.assets["shaders\\fragment\\texture.frag.spv"] as Shader, new Vector2(0, 0), new Vector2(0, 0), 0);
                //e.Mesh = Mesh<VertexPositionColorTexture>.Square(new Vector2(100,100), Color.White());

                graphicsManager!.End(cmd, imageIndex);
            };

            window.FramebufferResize += size =>
            {
                
            };

            window.Run();
        }
    }
}