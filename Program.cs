using Silk.NET.Windowing;
using Silk.NET.Vulkan;
using Xirface;
using Silk.NET.Input;
using System.Numerics;
using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Vulkan.Extensions.KHR;

Vulkan? graphics = null;
Interface? ìnterface;
AssetManager? assetManager;
Input? input;
Shader<VertexPositionColor>? shader = null;

Entity entity = null;

unsafe
{


    var options = WindowOptions.DefaultVulkan with
    {
        Size = new(1366, 768),
        Title = "Xirface",
        API = new GraphicsAPI(ContextAPI.Vulkan, ContextProfile.Core, ContextFlags.Default, new APIVersion(1,2))

    };

    var window = Window.Create(options);
    

    window.Load += () =>
    {
        var context = window.CreateInput();

        graphics = new Vulkan(window);
        assetManager = new(graphics);

        shader = assetManager.Load<Shader<VertexPositionColor>>("shaders/vertex/positioncolor.vert.spv", "shaders/fragment/vertex.frag.spv");
        entity = new Entity(shader, new Vector2(0, 0), new Vector2(0, 0), 0);
        entity.Mesh = Mesh<VertexPositionColor>.Square(new Vector2(100,100), Color.White());
    };


    window.Update += delta =>
    {

    };


    window.Render += delta =>
    {
        graphics!.Begin(out var cmd, out var imageIndex);

        entity!.Mesh.Buffer(graphics);
        entity!.Draw(graphics, cmd, new Camera(1366, 768), shader!);


        graphics!.End(cmd, imageIndex);
        
    };

    window.FramebufferResize += size =>
    {
        
    };

    window.Run();
}