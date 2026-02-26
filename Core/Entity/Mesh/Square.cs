using System.Numerics;

namespace Xirface
{
    public partial class Mesh
    {
        public static Mesh<VertexPositionColor> Square(Vector2 size,  Color fillColor)
        {
            Mesh<VertexPositionColor> Mesh = new();
            Mesh.Indices = [0,1,2,0,2,3];

            Mesh.Vertices = [
                new VertexPositionColor(new Vector3(0,0,0), fillColor),
                new VertexPositionColor(new Vector3(0,size.Y,0), fillColor),
                new VertexPositionColor(new Vector3(size.X,size.Y,0), fillColor),
                new VertexPositionColor(new Vector3(size.X,0,0), fillColor)
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