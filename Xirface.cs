using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Xirface
{
    public abstract class Xirface
    {
        protected GraphicsManager GraphicsManager;
        protected AssetManager AssetManager;
        protected InputManager InputManager;
        protected Camera Camera;
        protected IWindow Window;
        protected ClearColorValue ClearColor {  get; set; } = new ClearColorValue(0,0,0,1f);

        protected abstract void Load();
        protected abstract void Update(double delta);
        protected abstract void Draw(double delta, CommandBuffer cmd, uint imageIndex);
        protected virtual void Resize(Vector2D<int> size) { }

        public void Run(WindowOptions? options = null)
        {
            var windowOptions = WindowOptions.DefaultVulkan with
            {
                Size = new(1366, 768),
                Title = "Xirface",
                API = new GraphicsAPI(ContextAPI.Vulkan, ContextProfile.Core, ContextFlags.Default, new APIVersion(1, 2))
            };

            Window = Silk.NET.Windowing.Window.Create(windowOptions);

            Window.Load += () =>
            {
                Camera = new(windowOptions.Size.X, windowOptions.Size.Y);

                GraphicsManager = new GraphicsManager(Window);
                AssetManager = new AssetManager(GraphicsManager);

                IInputContext input = Window.CreateInput();
                InputManager = new InputManager(input.Mice[0], input.Keyboards[0]);

                Load();
            };

            Window.Update += delta => {

     
                Update(delta);
                };

            Window.Render += delta =>
            {
                if (Window.FramebufferSize.X == 0 || Window.FramebufferSize.Y == 0) return;
                if (!GraphicsManager!.Begin(out var cmd, out var imageIndex, Window, ClearColor)) return;

                Draw(delta, cmd, imageIndex);
                if (!GraphicsManager!.End(cmd, imageIndex, Window)) return;
            };

            Window.FramebufferResize += size =>
            {
                if (Window.FramebufferSize.X == 0 || Window.FramebufferSize.Y == 0)
                    return;

                GraphicsManager!.RecreateSwapchain(Window);
                Resize(size);
            };

            Window.Run();
        }
    }
}