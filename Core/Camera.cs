using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;


namespace Xirface
{
    public class Camera
    {
        public Vector2 Position;
        public float Zoom = 1;
        public int Width;
        public int Height;

        
        public Camera(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
