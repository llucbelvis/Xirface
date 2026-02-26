using System.Numerics;


namespace Xirface
{

    public partial class Mesh
    {
        public static Mesh<VertexPositionColorTexture> Texture(Vector2 size, Color color, Texture2D texture)
        {
            Mesh<VertexPositionColorTexture> Mesh = new();
            Mesh.Indices = [0, 1, 2, 1, 3, 2];

            Mesh.Vertices = [
                new(new Vector3(0,0,0), color , new Vector2(0, 1)),
                new(new Vector3(0,texture.Height * size.Y,0), color ,new Vector2(0,0)),
                new(new Vector3(texture.Width * size.X, 0,0), color  ,new Vector2(1, 1)),
                new(new Vector3(texture.Width * size.X, texture.Height * size.Y,0), color,new Vector2(1, 0)),
            ];

            Mesh.Body = [
                new Vector2(0, 0),
                new Vector2(0, size.Y),
                new Vector2(size.X, size.Y),
                new Vector2(size.X, 0)
            ];

            Mesh.Dirty();

            return Mesh;
        }

    }
}