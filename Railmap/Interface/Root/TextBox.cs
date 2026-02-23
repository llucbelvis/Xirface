using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;



namespace Xirface
{
    public class TextBox : Text
    {
        public bool Active;
        

        public new Mesh<VertexFont> Mesh;

        public TextBox(string id, Vector2 position, Vector2 origin, Vector2 size, float depth, Color fillColor, bool visible, HashSet<Root> children, Font font, string content, float fontSize, String.Alignment alignment)
        : base(id, position, origin, size, depth, fillColor, visible, children, font, content, fontSize, alignment)
        {
            
        }

        
    }
}