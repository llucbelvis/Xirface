using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using System.Numerics;
using Xirface;

public class Texture : Entity
{
    public new Mesh<VertexPositionColorTexture>? Mesh;
    public Texture2D? Content;

    public unsafe Texture(Shader shader, GraphicsManager graphics, Vector2 position, Vector2 origin, Texture2D? texture)
        : base(shader, graphics, position, origin)
    {
        Content = texture;
        
    }

    public unsafe override void Draw(GraphicsManager graphics, CommandBuffer cmd, Camera camera, IWindow window)
    {
        SetView(Matrix4x4.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0));
        SetProjection(Matrix4x4.CreateOrthographic(window.Size.X * camera.Zoom, -window.Size.Y * camera.Zoom, -1, 1f));
        SetWorld(Matrix4x4.CreateScale(1, 1, 1) * Matrix4x4.CreateRotationZ(0) * Matrix4x4.CreateTranslation(new Vector3(Position.X - Origin.X, Position.Y - Origin.Y, Depth)), graphics, cmd);

        Shader.SetTexture(descriptorSet, Content);

        Shader.Apply(cmd, descriptorSet);
        Mesh!.Buffer(graphics);
        Mesh!.Draw(graphics, cmd);
    }
}